# La Mesa del Duque

Sistema integral de gestión para restaurante desarrollado como proyecto académico de Gestión de la Calidad del Software. Permite administrar pedidos, productos, recetas, mesas, usuarios y roles desde una interfaz web moderna, aplicando estándares internacionales de calidad (ISO/IEC 25010, ISO 27001) y buenas prácticas de ingeniería de software.

## Estado del proyecto

**Base inicial en preparación.** El Sprint 1 se enfocará en punto de venta, productos, recetas, mesas, usuarios, roles e inicio de sesión seguro. Antes de implementar funcionalidad, el repositorio establece arquitectura, documentación, trazabilidad, pruebas y controles de calidad.

## Estructura del repositorio

```
La-Mesa-del-Duque/
├── src/
│   ├── LaMesaDelDuque.Dominio/       # Entidades, value objects, interfaces y lógica de negocio
│   ├── LaMesaDelDuque.Infraestructura/ # EF Core, DbContext, migraciones, repositorios
│   └── LaMesaDelDuque.Web/          # ASP.NET Core Razor Pages, SignalR, Bootstrap 5.3
├── tests/
│   ├── LaMesaDelDuque.Pruebas/      # Pruebas unitarias e integración con xUnit
│   ├── regresion/                    # Suite de pruebas de regresión
│   └── impacto-cambios/              # Matriz de impacto de cambios
├── docs/
│   ├── arquitectura/                 # Documento de arquitectura y ADR (Architecture Decision Records)
│   ├── calidad/                      # Plan de calidad, definición de hecho, trazabilidad, riesgos
│   ├── requisitos/                   # Historias de usuario, criterios de aceptación
│   ├── pruebas/                      # Estrategia de pruebas
│   ├── seguridad/                    # Alcance SGSI, declaración ISO 27001
│   ├── auditoria/                    # Plan de auditoría, checklist de evidencia
│   └── metricas/                     # Indicadores de calidad
├── .github/
│   └── workflows/                    # GitHub Actions — CI/CD, análisis, despliegue
├── LaMesaDelDuque.slnx               # Archivo de solución (.NET 8)
└── README.md
```

## Requisitos del sistema

| Componente         | Versión / Herramienta                  |
|--------------------|----------------------------------------|
| SDK .NET           | 8.0 o superior                         |
| Base de datos      | PostgreSQL 15+ (Supabase)              |
| ORM                | Entity Framework Core 8                |
| Frontend           | Bootstrap 5.3 + jQuery                 |
| Tiempo real        | SignalR (ASP.NET Core)                 |
| Pruebas            | xUnit 2.5 + Coverlet                   |
| CI/CD              | GitHub Actions                         |

## Cómo ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone https://github.com/jojosenthusiast/La-Mesa-del-Duque.git
cd La-Mesa-del-Duque
```

### 2. Configurar la base de datos

Crear un archivo `appsettings.Development.json` en `src/LaMesaDelDuque.Web/` con la cadena de conexión a PostgreSQL (Supabase):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=aws-0-region.pooler.supabase.com;Database=postgres;Username=postgres;Password=TU_CONTRASENA"
  }
}
```

### 3. Aplicar migraciones

```bash
dotnet ef database update --project src/LaMesaDelDuque.Infraestructura --startup-project src/LaMesaDelDuque.Web
```

### 4. Ejecutar la aplicación

```bash
dotnet run --project src/LaMesaDelDuque.Web
```

La aplicación estará disponible en `https://localhost:5001`.

## Cómo ejecutar las pruebas

```bash
dotnet test
```

Para generar informe de cobertura:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Flujo de ramas y Pull Requests

Este proyecto sigue un modelo de ramas estricto:

| Rama          | Propósito                                      |
|---------------|------------------------------------------------|
| `main`        | Rama principal estable. Solo acepta merges vía PR. |
| `feature/*`   | Una rama por cada funcionalidad o historia.      |
| `chore/*`     | Configuración, documentación o mantenimiento.     |
| `hotfix/*`    | Correcciones urgentes sobre `main`.              |

### Reglas de trabajo

1. Todo cambio funcional se desarrolla en una rama `feature/NOMBRE` creada desde `main`.
2. Todo cambio hacia `main` debe entrar mediante Pull Request.
3. Cada PR requiere al menos una revisión de código y la aprobación de los checks automáticos.
4. El título del PR debe seguir el formato convencional: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`.
5. El código en `main` nunca se modifica directamente.

## Gestión de la calidad

El sistema aplica el modelo de calidad ISO/IEC 25010 con enfoque en las siguientes características:

- **Funcionalidad**: cada historia de usuario tiene criterios de aceptación explícitos y trazables.
- **Fiabilidad**: pruebas unitarias, de integración y regresión automatizadas con xUnit.
- **Seguridad**: autenticación con RBAC, protección CSRF, validación de entradas, comunicaciones HTTPS.
- **Mantenibilidad**: arquitectura en capas (Dominio → Infraestructura → Web), principios SOLID, ADR documentados.
- **Portabilidad**: configuración portable mediante variables de entorno y PostgreSQL/Supabase.

La documentación completa del sistema de calidad se encuentra en `docs/calidad/`.

### Definición de Hecho (Definition of Done)

Una historia de usuario se considera terminada cuando:
- El código está implementado y cumple los criterios de aceptación.
- Las pruebas unitarias asociadas pasan (xUnit).
- La revisión de código está completada y aprobada.
- La documentación técnica está actualizada (ADR si aplica).
- La matriz de trazabilidad está actualizada.
- No hay defectos abiertos de severidad alta o crítica.
- El checklist de seguridad está verificado.

Véase `docs/calidad/definicion-de-hecho.md` para el detalle completo.

## Seguridad

El proyecto adopta un Sistema de Gestión de Seguridad de la Información (SGSI) alineado con ISO/IEC 27001. Las medidas implementadas incluyen:

- Autenticación con hash de contraseñas mediante BCrypt.
- Control de acceso basado en roles (RBAC) a nivel de página y endpoint.
- Tokens anti-falsificación (CSRF) en todos los formularios.
- Validación de entradas del lado del servidor con anotaciones de datos.
- HTTPS obligatorio en todos los entornos.
- Principio de menor privilegio en la base de datos.

La documentación del SGSI se encuentra en `docs/seguridad/`.

## Releases

Las versiones se etiquetan siguiendo versionado semántico (`MAJOR.MINOR.PATCH`):

| Canal      | Rama base | Descripción                     |
|------------|-----------|---------------------------------|
| `pre-alpha`| `main`    | Entregas internas del sprint.   |
| `alpha`    | `main`    | Primer entregable funcional.    |
| `beta`     | `main`    | Funcionalidad completa, puliendo defectos. |
| `v1.0.0`   | `main`    | Versión final del proyecto.     |

Cada release incluye un changelog con las historias completadas, defectos corregidos y cambios en la arquitectura.

## Licencia

Proyecto académico. Todos los derechos reservados.
