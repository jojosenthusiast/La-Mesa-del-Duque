# Arquitectura del Sistema — La Mesa del Duque

## 1. Propósito

Este documento describe la arquitectura de software del sistema **La Mesa del Duque**, un sistema integral de gestión para restaurante. Define la estructura de capas, los patrones utilizados, las decisiones arquitectónicas (ADR), el modelo de dominio y la infraestructura tecnológica que soporta el sistema.

## 2. Visión general

**La Mesa del Duque** sigue una arquitectura en capas (Layered Architecture) con separación estricta de responsabilidades, alineada con los principios SOLID y Domain-Driven Design (DDD) táctico. El sistema se organiza en tres capas principales:

```
┌─────────────────────────────────────────────────────┐
│                   LaMesaDelDuque.Web                 │
│          ASP.NET Core 8 Razor Pages + SignalR        │
│          Bootstrap 5.3, jQuery, CSRF, RBAC           │
├─────────────────────────────────────────────────────┤
│            LaMesaDelDuque.Infraestructura             │
│    EF Core 8, PostgreSQL (Supabase), Repositorios,   │
│              Migraciones, DbContext                  │
├─────────────────────────────────────────────────────┤
│              LaMesaDelDuque.Dominio                   │
│    Entidades, Value Objects, Interfaces, Servicios,   │
│              Lógica de negocio pura                  │
└─────────────────────────────────────────────────────┘
```

### Regla de dependencia

Las dependencias solo apuntan hacia **adentro**:

- `Web` depende de `Infraestructura` y `Dominio`.
- `Infraestructura` depende de `Dominio`.
- `Dominio` **no depende** de ninguna otra capa.

## 3. Capa de Dominio (`LaMesaDelDuque.Dominio`)

### Propósito

Contiene la lógica de negocio pura, independiente de cualquier framework, base de datos o tecnología de presentación.

### Componentes

| Componente       | Descripción                                                  |
|------------------|--------------------------------------------------------------|
| **Entidades**     | Objetos del dominio con identidad propia: `Pedido`, `Producto`, `Receta`, `Mesa`, `Usuario`, `Rol`, `DetallePedido`. |
| **Value Objects** | Objetos sin identidad, definidos por sus atributos: `Dinero` (monto + moneda), `Direccion`. |
| **Interfaces**    | Contratos para repositorios y servicios externos (ej. `IPedidoRepositorio`, `IUsuarioRepositorio`). |
| **Servicios**     | Orquestan la lógica de negocio cuando involucra múltiples entidades (ej. `PedidoServicio`). |
| **Excepciones**   | Excepciones de dominio tipificadas (ej. `PedidoYaPagadoException`). |

### Principios aplicados

- **SOLID**: Cada clase tiene una sola responsabilidad; las interfaces permiten inversión de dependencias.
- **Encapsulamiento**: Las entidades protegen sus invariantes; los setters públicos son mínimos.
- **Ubiquitous Language**: Los nombres de clases, métodos y propiedades reflejan el lenguaje del negocio (restaurante).

## 4. Capa de Infraestructura (`LaMesaDelDuque.Infraestructura`)

### Propósito

Implementa las interfaces definidas en el dominio y gestiona la comunicación con sistemas externos: base de datos, proveedores de autenticación, servicios cloud.

### Componentes

| Componente        | Descripción                                                    |
|-------------------|----------------------------------------------------------------|
| **AppDbContext**   | Contexto de Entity Framework Core que mapea entidades a tablas. |
| **Repositorios**   | Implementan las interfaces del dominio usando EF Core (ej. `PedidoRepositorio`, `UsuarioRepositorio`). |
| **Migraciones**    | Control de versiones del esquema de base de datos.             |
| **Configuraciones**| Configuración de mapeos con Fluent API (`IEntityTypeConfiguration<T>`). |

### Tecnologías

| Tecnología          | Versión | Propósito                                |
|---------------------|---------|------------------------------------------|
| Entity Framework Core | 8     | ORM, mapeo objeto-relacional             |
| Npgsql              | 8       | Proveedor de PostgreSQL para .NET        |
| PostgreSQL (Supabase)| 15+    | Base de datos relacional en la nube      |

## 5. Capa Web (`LaMesaDelDuque.Web`)

### Propósito

Interfaz de usuario y puntos de entrada HTTP. Implementa la presentación, autenticación, autorización y comunicación en tiempo real.

### Componentes

| Componente          | Descripción                                                  |
|---------------------|--------------------------------------------------------------|
| **Razor Pages**     | Páginas web con modelo code-behind (PageModel).              |
| **SignalR Hubs**    | Comunicación bidireccional en tiempo real para pedidos.      |
| **Middleware**      | Autenticación con cookies, protección CSRF, HSTS, HTTPS.     |
| **wwwroot**         | Archivos estáticos: CSS (Bootstrap), JS (jQuery, SignalR).   |

### Tecnologías

| Tecnología          | Versión    | Propósito                               |
|---------------------|------------|-----------------------------------------|
| ASP.NET Core        | 8.0        | Framework web                           |
| Razor Pages         | —          | Modelo de presentación                  |
| SignalR             | —          | Tiempo real (pedidos, notificaciones)    |
| Bootstrap           | 5.3        | Framework CSS responsivo                |
| jQuery              | 3.x        | Manipulación del DOM, AJAX              |
| BCrypt.Net-Next     | 4.x        | Hash seguro de contraseñas              |

### Configuración de seguridad

- **Autenticación**: Cookies con `HttpOnly`, `Secure`, `SameSite=Strict`. Hash de contraseñas con BCrypt.
- **Autorización**: RBAC mediante `[Authorize(Roles = "...")]`.
- **CSRF**: AntiForgeryToken en todos los formularios POST.
- **HTTPS**: Redirección forzosa + HSTS.
- **Validación**: Data Annotations en el servidor (PageModel) + validación en cliente (jQuery Validation).

## 6. Comunicación entre capas

```
┌──────────────────────────────────────────────────────────────────┐
│  Usuario (Navegador)                                             │
│    │                                                              │
│    │ HTTPS + SignalR (wss://)                                    │
│    ▼                                                              │
│  [LaMesaDelDuque.Web]                                             │
│    │  PageModel llama a servicios de dominio                     │
│    ▼                                                              │
│  [LaMesaDelDuque.Dominio]                                         │
│    │  Servicios de dominio usan interfaces de repositorio        │
│    ▼                                                              │
│  [LaMesaDelDuque.Infraestructura]                                 │
│    │  Repositorios implementan interfaces usando EF Core         │
│    ▼                                                              │
│  [PostgreSQL / Supabase]                                          │
└──────────────────────────────────────────────────────────────────┘
```

## 7. Modelo de dominio (vista simplificada)

```
┌──────────┐      ┌─────────────────┐      ┌──────────┐
│  Usuario │──────│     Pedido      │──────│   Mesa   │
│          │      │                 │      │          │
│  (RBAC)  │      │ - DetallePedido │      │ - estado │
└──────────┘      └────────┬────────┘      └──────────┘
                           │
                    ┌──────▼──────┐
                    │  Producto   │
                    │             │
                    │ - Receta    │
                    └─────────────┘
```

## 8. Pruebas

Las pruebas siguen la misma estructura de capas:

| Capa              | Tipo de prueba         | Framework     |
|-------------------|------------------------|---------------|
| Dominio           | Unitarias              | xUnit         |
| Infraestructura   | Integración            | xUnit         |
| Web               | Integración (WebApplicationFactory) | xUnit |
| Sistema completo  | Regresión, Smoke       | xUnit         |

La cobertura objetivo es ≥ 80% y se mide con Coverlet.

## 9. Despliegue

El sistema está diseñado para despliegue en la nube usando Supabase como base de datos PostgreSQL gestionada. El pipeline de CI/CD en GitHub Actions compila, prueba y genera los artefactos listos para despliegue.

## 10. Decisiones arquitectónicas (ADR)

Las decisiones de arquitectura significativas están documentadas como ADR en `docs/arquitectura/adr/`:

| ADR   | Título                        | Decisión                            |
|-------|-------------------------------|-------------------------------------|
| 0001  | Arquitectura en capas         | Separación estricta: Dominio → Infraestructura → Web |
| 0002  | ASP.NET Razor Pages           | Razor Pages sobre MVC tradicional para productividad |
| 0003  | PostgreSQL / Supabase         | Base de datos relacional gestionada en la nube |

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Responsable**: Arquitecto de software
