# Checklist de Revisión de Código — La Mesa del Duque

## 1. Propósito

Este checklist garantiza que cada Pull Request es revisado de forma sistemática y consistente, cubriendo aspectos de funcionalidad, diseño, legibilidad, seguridad y pruebas antes de ser fusionado en `develop` o `main`.

## 2. Instrucciones para el revisor

1. Leer la descripción del PR y verificar que está vinculado a una historia de usuario o tarea.
2. Ejecutar el código localmente si la revisión lo requiere (cambios complejos, UI, SignalR).
3. Marcar cada ítem como **✓ Cumple**, **✗ No cumple**, o **N/A**.
4. Si algún ítem marcado como **✗ No cumple** es bloqueante, solicitar cambios en el PR.
5. Completar la sección de resumen al final.

## 3. Checklist

### 3.1 General

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 1  | El PR tiene un título descriptivo siguiendo convención (`feat:`, `fix:`, `docs:`, etc.). | ☐ |
| 2  | El PR está vinculado a una HU o tarea específica.                     | ☐ |
| 3  | El alcance del PR es acotado (un solo propósito, no mezcla funcionalidades no relacionadas). | ☐ |
| 4  | No se incluyen archivos de configuración local, binarios, `obj/`, `bin/` ni secretos. | ☐ |

### 3.2 Arquitectura y diseño

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 5  | Se respeta la arquitectura en capas: Dominio → Infraestructura → Web. | ☐ |
| 6  | La capa de Dominio no tiene dependencias de Infraestructura ni Web.   | ☐ |
| 7  | Las entidades de dominio no contienen lógica de persistencia o presentación. | ☐ |
| 8  | Las interfaces están en la capa de Dominio y sus implementaciones en Infraestructura. | ☐ |
| 9  | Se aplican principios SOLID: responsabilidad única, inversión de dependencias. | ☐ |
| 10 | Nuevas clases o módulos están en el namespace/proyecto correcto.      | ☐ |

### 3.3 Funcionalidad

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 11 | La funcionalidad implementada coincide con los criterios de aceptación de la HU. | ☐ |
| 12 | Se manejan todos los flujos: principal, alternativo y de error.       | ☐ |
| 13 | Las condiciones de borde están cubiertas (nulos, vacíos, límites).    | ☐ |
| 14 | No hay `TODO`, `FIXME` o código comentado sin justificación.          | ☐ |
| 15 | Los mensajes de error mostrados al usuario son claros y están en español. | ☐ |

### 3.4 Legibilidad y mantenibilidad

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 16 | Nombres de clases, métodos y variables son descriptivos y en español (dominio) / inglés (patrones técnicos). | ☐ |
| 17 | Los métodos son cortos (< 30 líneas) y con una sola responsabilidad.  | ☐ |
| 18 | Se evita la duplicación de código (DRY). Si existe duplicación justificada, está documentada. | ☐ |
| 19 | El código complejo tiene comentarios que explican el *por qué*.        | ☐ |
| 20 | Las constantes y valores mágicos están extraídos en variables con nombre significativo. | ☐ |

### 3.5 Seguridad

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 21 | Las entradas de usuario se validan en el servidor (Data Annotations, FluentValidation). | ☐ |
| 22 | No se usa SQL crudo concatenado; todas las consultas usan EF Core / LINQ parametrizado. | ☐ |
| 23 | Las páginas que requieren autenticación tienen `[Authorize]`.         | ☐ |
| 24 | Las páginas que requieren rol específico tienen `[Authorize(Roles = "...")]`. | ☐ |
| 25 | Los formularios POST incluyen token anti-falsificación (`@Html.AntiForgeryToken()`). | ☐ |
| 26 | No se exponen contraseñas, tokens o secretos en logs, URLs o respuestas. | ☐ |
| 27 | Las contraseñas se almacenan con BCrypt (nunca en texto plano).       | ☐ |

### 3.6 Pruebas

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 28 | Existen pruebas unitarias para la lógica de negocio nueva o modificada. | ☐ |
| 29 | Las pruebas cubren casos de éxito, fallo y límite.                    | ☐ |
| 30 | Las pruebas son independientes entre sí (no dependen de orden de ejecución). | ☐ |
| 31 | Los nombres de los métodos de prueba describen el escenario (`Debe_CrearPedido_CuandoDatosSonValidos`). | ☐ |
| 32 | `dotnet test` se ejecuta sin errores.                                 | ☐ |

### 3.7 Rendimiento

| #  | Ítem                                                                 | Cumple |
|----|-----------------------------------------------------------------------|--------|
| 33 | Las consultas a base de datos no tienen N+1 (se usa `.Include()` o proyecciones cuando es necesario). | ☐ |
| 34 | No se cargan colecciones completas innecesariamente (usar `.Take()`, `.Skip()`, paginación). | ☐ |
| 35 | Las operaciones costosas no se ejecutan en el hilo de UI (SignalR, procesos largos). | ☐ |

## 4. Resumen de revisión

| Campo              | Valor                        |
|--------------------|------------------------------|
| **Revisor**        |                              |
| **Fecha**          |                              |
| **PR**             |                              |
| **HU relacionada** |                              |
| **Veredicto**      | ☐ Aprobado  ☐ Cambios solicitados  ☐ Rechazado |

### Comentarios adicionales

_(Espacio para observaciones, sugerencias o riesgos detectados durante la revisión.)_

---

**Versión**: 1.0 | **Fecha**: Abril 2026
