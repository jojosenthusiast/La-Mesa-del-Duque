# Suite de Regresión — La Mesa del Duque

## 1. Propósito

Este documento define la suite de pruebas de regresión del sistema **La Mesa del Duque**. Su objetivo es garantizar que los cambios introducidos en cada sprint no rompen funcionalidad existente. La suite se ejecuta en cada Pull Request y antes de cada release.

## 2. Estructura de la suite

La suite de regresión está organizada por historia de usuario y se almacena en `tests/regresion/`. Cada caso de prueba tiene un identificador único `TC-REG-NNN`.

### Jerarquía de ejecución

```
Nivel 1 — Smoke tests (5 min)
    └── Verifican que el sistema arranca y las funciones críticas responden.

Nivel 2 — Regresión por HU (15-20 min)
    └── Casos de prueba automatizados que cubren cada HU del sprint actual.

Nivel 3 — Regresión completa (30-45 min)
    └── Suite completa de pruebas unitarias + integración + casos manuales documentados.
```

## 3. Catálogo de casos de regresión

### 3.1 Smoke Tests (Nivel 1)

| ID         | Descripción                                      | Tipo      | HU        |
|------------|--------------------------------------------------|-----------|-----------|
| TC-REG-S01 | La aplicación arranca sin errores.               | Automática | Global   |
| TC-REG-S02 | La página de inicio de sesión carga correctamente.| Automática | HU-025   |
| TC-REG-S03 | La conexión a la base de datos responde.         | Automática | Global   |
| TC-REG-S04 | El endpoint de SignalR (PedidoHub) está disponible.| Automática | HU-001  |

### 3.2 HU-001 — Registrar pedido (POS)

| ID         | Descripción                                                | Tipo      |
|------------|------------------------------------------------------------|-----------|
| TC-REG-001-01 | Registrar un pedido con productos válidos — se crea correctamente. | Automática |
| TC-REG-001-02 | Registrar un pedido sin productos — se muestra error de validación. | Automática |
| TC-REG-001-03 | Registrar un pedido con una mesa ocupada — se asigna correctamente. | Automática |
| TC-REG-001-04 | SignalR notifica a otros clientes sobre el nuevo pedido.     | Manual    |
| TC-REG-001-05 | El pedido se muestra en la lista de pedidos activos.         | Automática |

### 3.3 HU-002 — Modificar pedido no pagado

| ID         | Descripción                                                | Tipo      |
|------------|------------------------------------------------------------|-----------|
| TC-REG-002-01 | Agregar un producto a un pedido existente no pagado — se actualiza. | Automática |
| TC-REG-002-02 | Quitar un producto de un pedido existente — se actualiza.  | Automática |
| TC-REG-002-03 | Intentar modificar un pedido ya pagado — se rechaza con error. | Automática |
| TC-REG-002-04 | Cambiar la mesa asociada a un pedido — se actualiza.       | Automática |
| TC-REG-002-05 | SignalR notifica cambios en el pedido a otros clientes.    | Manual    |

### 3.4 HU-003 — Eliminar pedido pendiente no pagado

| ID         | Descripción                                                | Tipo      |
|------------|------------------------------------------------------------|-----------|
| TC-REG-003-01 | Eliminar un pedido en estado pendiente y no pagado — se elimina. | Automática |
| TC-REG-003-02 | Intentar eliminar un pedido pagado — se rechaza con error. | Automática |
| TC-REG-003-03 | Intentar eliminar un pedido en preparación — se rechaza con error. | Automática |
| TC-REG-003-04 | La eliminación se refleja en la lista de pedidos activos.  | Automática |

### 3.5 HU-011 — Gestionar productos

| ID         | Descripción                                                | Tipo      |
|------------|------------------------------------------------------------|-----------|
| TC-REG-011-01 | Crear un producto con datos válidos — se registra en BD.   | Automática |
| TC-REG-011-02 | Crear un producto sin nombre — error de validación.        | Automática |
| TC-REG-011-03 | Editar el precio de un producto — se actualiza.            | Automática |
| TC-REG-011-04 | Eliminar un producto que no está en pedidos activos — se elimina. | Automática |
| TC-REG-011-05 | Eliminar un producto que está en un pedido activo — se rechaza. | Automática |
| TC-REG-011-06 | Listar productos con filtro por categoría.                 | Automática |

### 3.6 HU-014 — Recetas de productos

| ID         | Descripción                                                | Tipo      |
|------------|------------------------------------------------------------|-----------|
| TC-REG-014-01 | Asociar una receta a un producto — se guarda correctamente. | Automática |
| TC-REG-014-02 | Editar los ingredientes de una receta — se actualiza.      | Automática |
| TC-REG-014-03 | Eliminar una receta — se desvincula del producto.          | Automática |
| TC-REG-014-04 | Ver receta desde la página de detalle del producto.        | Manual    |

### 3.7 HU-016 — Gestión de mesas

| ID         | Descripción                                                | Tipo      |
|------------|------------------------------------------------------------|-----------|
| TC-REG-016-01 | Crear una mesa — se registra en BD.                        | Automática |
| TC-REG-016-02 | Cambiar el estado de una mesa (libre → ocupada).           | Automática |
| TC-REG-016-03 | Cambiar el estado de una mesa con pedido activo — se actualiza pedido. | Automática |
| TC-REG-016-04 | Eliminar una mesa sin pedidos activos — se elimina.        | Automática |
| TC-REG-016-05 | Eliminar una mesa con pedido activo — se rechaza.          | Automática |

### 3.8 HU-021 — Gestión de usuarios y roles

| ID         | Descripción                                                | Tipo      |
|------------|------------------------------------------------------------|-----------|
| TC-REG-021-01 | Crear un usuario con datos válidos — se registra.          | Automática |
| TC-REG-021-02 | Asignar un rol a un usuario — se actualiza.                | Automática |
| TC-REG-021-03 | Cambiar el rol de un usuario — se actualiza acceso.        | Automática |
| TC-REG-021-04 | Eliminar un usuario — se desactiva (borrado lógico).       | Automática |
| TC-REG-021-05 | Un usuario sin rol no puede acceder a páginas restringidas.| Automática |

### 3.9 HU-025 — Inicio de sesión, RBAC y CSRF

| ID         | Descripción                                                | Tipo      |
|------------|------------------------------------------------------------|-----------|
| TC-REG-025-01 | Iniciar sesión con credenciales válidas — redirige al dashboard. | Automática |
| TC-REG-025-02 | Iniciar sesión con contraseña incorrecta — error.          | Automática |
| TC-REG-025-03 | Usuario con rol "Cocinero" no accede a gestión de usuarios. | Automática |
| TC-REG-025-04 | Usuario con rol "Administrador" accede a todas las secciones. | Automática |
| TC-REG-025-05 | Formulario POST sin token CSRF — se rechaza (HTTP 400).    | Automática |
| TC-REG-025-06 | Cerrar sesión — redirige al inicio de sesión y la cookie se invalida. | Automática |
| TC-REG-025-07 | Sesión expirada — redirige al inicio de sesión.            | Automática |

## 4. Ejecución de la suite

### Automatizada

La suite automatizada se ejecuta con xUnit:

```bash
dotnet test "LaMesaDelDuque.slnx" --filter "Category=Regression"
```

Los casos manuales se ejecutan siguiendo las instrucciones en `tests/regresion/README.md`.

### Frecuencia

| Evento                     | Nivel ejecutado     |
|----------------------------|---------------------|
| Cada commit en `feature/*` | Nivel 1 (Smoke)     |
| Cada Pull Request          | Nivel 1 + Nivel 2   |
| Antes de merge a `main`    | Nivel 1 + 2 + 3     |
| Release candidata          | Nivel 1 + 2 + 3     |

## 5. Taxonomía ejecutable

Las pruebas de regresión automatizadas deben marcarse con:

```csharp
[Trait("Category", "Regression")]
```

La suite se ejecuta con:

```powershell
dotnet test "LaMesaDelDuque.slnx" --filter "Category=Regression"
```

## 6. Resultados

Los resultados de cada ejecución se documentan en `tests/regresion/resultados/` con el formato `YYYY-MM-DD-ejecucion-N.md`, incluyendo:

- Fecha y hora de ejecución.
- Rama y commit probados.
- Total de casos ejecutados / pasaron / fallaron.
- Lista de fallos con trazabilidad al defecto o HU.
- Acciones correctivas.

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Actualizar en cada sprint**
