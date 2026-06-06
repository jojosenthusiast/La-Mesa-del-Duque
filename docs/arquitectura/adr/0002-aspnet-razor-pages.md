# ADR 0002: ASP.NET Core Razor Pages

## Estado

**Aceptado**

## Contexto

El sistema **La Mesa del Duque** necesita una interfaz web para que los empleados del restaurante gestionen pedidos, productos, mesas y usuarios. Se requiere que la interfaz sea:

- Rápida de desarrollar (el plazo del proyecto académico es limitado).
- Mantenible por un equipo que está aprendiendo .NET.
- Segura, con protección CSRF integrada.
- Capaz de manejar comunicación en tiempo real (pedidos) y operaciones CRUD tradicionales.

Se evaluaron tres alternativas para la capa de presentación:

1. **ASP.NET Core MVC**: Modelo-Vista-Controlador tradicional con vistas Razor.
2. **ASP.NET Core Razor Pages**: Modelo de página con code-behind, introducido en ASP.NET Core 2.0.
3. **API REST + SPA (React/Vue)**: Backend como API y frontend separado con framework JavaScript.

## Decisión

Se adopta **ASP.NET Core Razor Pages** como modelo de presentación principal.

Razor Pages ofrece un modelo de desarrollo basado en páginas donde cada página tiene su propio modelo (`PageModel`) que maneja la lógica de presentación de forma cohesiva. Esto resulta más intuitivo que MVC para aplicaciones orientadas a páginas (como un sistema de gestión), y mantiene la seguridad integrada de ASP.NET Core (protección CSRF automática, validación del lado del servidor con Data Annotations, autorización con `[Authorize]`).

## Consecuencias

### Positivas

- **Productividad**: Cada página es autónoma (archivo `.cshtml` + `.cshtml.cs`). No requiere configuración de rutas manual en la mayoría de los casos.
- **Protección CSRF integrada**: Los formularios POST incluyen automáticamente tokens anti-falsificación.
- **Cohesión**: La lógica relacionada con una página vive en un solo lugar (PageModel). Ideal para operaciones CRUD.
- **Curva de aprendizaje suave**: Más simple que MVC para desarrolladores nuevos. No requiere entender el patrón Controlador-Vista.
- **Compatibilidad total con SignalR**: Los Hubs de SignalR coexisten naturalmente con Razor Pages.
- **Validación**: Data Annotations funcionan tanto en servidor como en cliente (con jQuery Unobtrusive Validation).

### Negativas

- Menor flexibilidad que una SPA para interfaces altamente interactivas (arrastrar y soltar, actualizaciones parciales complejas sin recarga).
- Las páginas muy complejas (múltiples formularios en una sola vista) pueden resultar en PageModels extensos.
- No es la opción más moderna si el objetivo fuera una experiencia de aplicación de una sola página (SPA).

### Mitigaciones

- Para interacciones complejas en tiempo real (POS de pedidos), se combina Razor Pages con SignalR para actualizaciones parciales.
- Los PageModels se mantienen enfocados usando servicios de dominio inyectados en lugar de lógica en el code-behind.
- Se establece un límite de complejidad: si un PageModel supera las 200 líneas, se considera extraer un servicio de aplicación.

## Alternativas consideradas

| Alternativa                | Razón del descarte                                            |
|----------------------------|---------------------------------------------------------------|
| ASP.NET Core MVC           | Mayor complejidad (controladores separados) sin beneficio para este tipo de aplicación. Razor Pages es más adecuado para CRUD. |
| API REST + SPA (React)     | Introduce dos repositorios, dos pipelines de build, y requiere manejar CSRF manualmente en el frontend. Mayor tiempo de desarrollo. |

## Relación con otros ADR

- **ADR-0001**: La capa Web usa Razor Pages. La separación en capas garantiza que el dominio no depende del modelo de presentación.
- **ADR-0003**: La infraestructura de datos usa PostgreSQL/Supabase. Razor Pages se comunica con el dominio, que a su vez usa repositorios (inyectados).

---

**Fecha**: Abril 2026 | **Decisores**: Arquitecto de software, equipo de desarrollo
