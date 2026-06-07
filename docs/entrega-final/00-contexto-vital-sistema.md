# Contexto vital del sistema — La Mesa del Duque

> Documento de contexto para generar PDF/DOCX, manuales y evidencia académica de entrega final.

## 1. Identidad del proyecto

| Campo | Valor |
|---|---|
| Sistema | **La Mesa del Duque** |
| Tipo | Sistema web de gestión gastronómica/restaurante |
| Contexto | Proyecto final académico de Gestión de la Calidad del Software |
| Defensa | 30 de mayo de 2026; simulación en vivo de 30 minutos |
| Repositorio | `https://github.com/jojosenthusiast/La-Mesa-del-Duque` |
| Stack | ASP.NET Core 8, Razor Pages, EF Core, PostgreSQL/Supabase, SignalR, Bootstrap, xUnit |
| Base de datos objetivo | PostgreSQL/Supabase |
| Idioma del producto | Español |

## 2. Propósito del sistema

La Mesa del Duque gestiona la operación de un restaurante desde la toma del pedido hasta cocina, despacho, cobro, inventario, control de mesas, reportes, usuarios, seguridad y auditoría.

Idea central para defensa:

> **No son pantallas aisladas: es una máquina de estados compartida entre roles.** Cajero registra el pedido, Cocina lo prepara, Despacho lo entrega/libera, y Administración supervisa métricas, roles, seguridad e integridad.

## 3. Roles reales y credenciales demo

| Rol | Usuario | Contraseña | Ruta principal | Propósito |
|---|---:|---:|---|---|
| Administrador | `admin` | `Admin123!` | `/Admin/Dashboard/Dashboard` | Configuración, usuarios, auditoría, reportes |
| Encargado | `carlos` | `Encargado321!` | Gestión/operación | Supervisión operativa, productos, inventario, cierres |
| Gerente | `luciana` | `Gerente890!` | `/Admin/Dashboard/Gerente` | Métricas gerenciales y reportes |
| Cajero | `sofia` | `Cajero567!` | `/Operaciones/Pedidos/Index` | POS, pedidos, caja/pagos |
| Cocinero | `pedro` | `Cocina456!` | `/Cocina/KDS` | Preparación en KDS |
| Despacho | `ana` | `Despacho901!` | `/Operaciones/Despacho/Index` | Entrega de pedidos listos |
| Mesero | `maria` | `Mesero789!` | `/Operaciones/Mesero/Index` | Atención en mesa y handoff |

## 4. Relación con la imagen “Pizza Express”

La imagen representa un pitch comercial simplificado de 4 roles: Cajero, Cocina, Despacho y Administrador. La Mesa del Duque implementa esos roles y además separa responsabilidades reales: Encargado, Gerente y Mesero.

| Rol del pitch | Rol real | Estado |
|---|---|---|
| Cajero | Cajero | Implementado |
| Cocina | Cocinero | Implementado |
| Despacho | Despacho | Implementado |
| Administrador | Administrador/Gerente/Encargado | Implementado con granularidad real |

Frase útil:

> “La imagen muestra la narrativa de venta; el sistema implementa una separación más profesional de responsabilidades.”

## 5. Modelos de negocio requeridos

| Modelo | Estado actual | Cómo defenderlo | Riesgo |
|---|---|---|---|
| Comida rápida | Funcional/parcial | POS + pedido rápido + cocina + despacho | No aparece con ese nombre explícito |
| Restaurante mesa | Fuerte | Mesas, mapa, mesero, pedido Comer aquí, despacho/liberación | Requiere demo limpia |
| Para llevar | Explícito | `TipoServicio.ParaLlevar`, sin mesa, entrega en despacho | Debe separarse de Delivery |
| Delivery/despacho | Despacho sí; Delivery no explícito | Despacho operativo existe; Delivery a domicilio requiere historia adicional | Alto si se afirma “delivery completo” |

### Verdad técnica sobre Delivery

Actualmente existe:

- `ComerAqui` y `ParaLlevar`.
- Pantalla de Despacho.
- Rol dedicado Despacho.
- Liberación de mesa al despachar.

Actualmente NO existe como delivery completo:

- `TipoServicio.Delivery`.
- Dirección de entrega.
- Teléfono/nombre del cliente para domicilio.
- Repartidor/courier.
- Costo de envío.
- Estados específicos de delivery.

Frase honesta si Delivery aún no se implementa:

> “El sistema soporta despacho operativo y pedidos para llevar. Delivery a domicilio queda identificado como extensión planificada o como mejora de la versión 3.x si se implementa antes de la defensa.”

## 6. Flujo operativo principal

```text
Cajero crea pedido
    ↓
Cocina recibe comanda en KDS
    ↓
Cocina marca Listo
    ↓
Despacho recibe pedido listo
    ↓
Despacho entrega pedido y libera mesa si aplica
    ↓
Administrador/Gerente revisa dashboard, reportes, auditoría y roles
```

## 7. Slices y mejoras recientes

Se han trabajado reparaciones y hardening sobre:

- Fundación de esquema y datos.
- Ciclo día/turno/caja.
- Consistencia KDS.
- Headers de seguridad.
- Integración de reportes.
- Hardening de pagos en mesero.
- Escapado cliente POS/Mesero.
- Rol dedicado Despacho.
- Assets críticos/offline.
- Mensajes de error seguros.
- Transferencia de mesas/handoff.
- Navegación Mesero.
- Reconciliación de cierre/caja.
- Mapa de salón conectado al catálogo.
- Feedback de guardado de productos.
- Tiempo correcto en Despacho desde hora listo.
- Pulido shell Admin/Gerente/Sidebar.

## 8. Fortalezas defendibles

- Arquitectura por capas: Dominio, Aplicación, Infraestructura, Web.
- Persistencia EF Core/PostgreSQL.
- Roles segmentados y RBAC.
- SignalR para flujos en tiempo real.
- xUnit y pruebas de regresión.
- Productos, recetas, alérgenos, inventario, mesas y pedidos.
- Dashboard operativo y gerencial.
- Auditoría y seguridad básica.
- Cierre/caja mejorado.
- Documentación de arquitectura, calidad y releases.

## 9. Gaps críticos antes de defensa

| Severidad | Gap | Por qué importa |
|---|---|---|
| P0 | Stock no es transaccional/concurrente | La rúbrica exige bloquear ventas si stock llega a cero |
| P0 | Delivery no es first-class | La matriz pide Delivery/despacho explícito |
| P0 | SQL final está desactualizado | La entrega exige script `.sql` limpio |
| P1 | Documentación final no está empaquetada | Lo que no se documenta, el evaluador no lo ve |
| P1 | Release metadata está atrasada | Para `3.x` hay que reconciliar versión/historial |
| P1 | Historias/Jira no reflejan todo lo agregado | La rúbrica audita gestión de calidad/agilidad |

## 10. Qué NO debe inventar el generador de documentos

No afirmar como implementado si no existe:

- integración con plataformas externas de delivery;
- backup manual a Supabase Storage;
- Jira con burndown real sin screenshots/export;
- release `v3.0.0` si aún no se creó;
- delivery con dirección/teléfono si no se implementa;
- stock concurrente seguro si no se corrige;
- PDF/video ya publicado si no existe enlace.

Usar “Pendiente de adjuntar por el equipo” donde falte evidencia.

## 11. Archivos relevantes en el RAR

| Tema | Archivos/carpetas |
|---|---|
| Dominio | `src/LaMesaDelDuque.Dominio/` |
| Servicios | `src/LaMesaDelDuque.Aplicacion/Servicios/` |
| EF/Postgres | `src/LaMesaDelDuque.Infraestructura/` |
| UI Razor | `src/LaMesaDelDuque.Web/Pages/` |
| JS POS/KDS | `src/LaMesaDelDuque.Web/wwwroot/js/` |
| Seeds runtime | `src/LaMesaDelDuque.Web/Program.cs` |
| SQL | `scripts/` |
| Pruebas | `tests/LaMesaDelDuque.Pruebas/` |
| Documentación | `docs/` |
| Releases | `docs/releases/`, `CHANGELOG.md`, `.release-please-manifest.json` |

## 12. Criterios de evaluación

| Criterio | Peso | Evidencia esperada |
|---|---:|---|
| Adaptabilidad | 10% | Matriz de modelos, roles, tipos de servicio |
| Roles en tiempo real | 10% | Demo Cajero → Cocina → Despacho → Admin |
| Stock e integridad | 10% | Bloqueo por stock cero, transacciones, no negativos |
| Usabilidad, errores y roles | 10% | UI clara, errores seguros, RBAC, manuales |
| Jira/agilidad/evidencia | 10% | Historias, criterios, PRs, releases, screenshots/export Jira |
