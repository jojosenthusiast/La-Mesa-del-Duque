# Criterios de Aceptación — La Mesa del Duque

## 1. Propósito

Este documento detalla los criterios de aceptación para cada historia de usuario del sistema **La Mesa del Duque**. Los criterios de aceptación definen las condiciones específicas, verificables y medibles que determinan si una historia de usuario está correctamente implementada. Siguen el formato Gherkin (Given/When/Then) cuando aplica.

## 2. Criterios por historia de usuario

### CA-001: Registrar pedido en punto de venta (POS) — HU-001

**CA-001-01: Pedido con productos válidos**
- **Dado** que el usuario está autenticado con rol "Mesero" o "Cajero"
- **Cuando** selecciona una mesa libre, agrega al menos un producto del menú con cantidad ≥ 1, y confirma el pedido
- **Entonces** el pedido se registra en el sistema con estado "Pendiente", se asocia a la mesa seleccionada, y se notifica vía SignalR a los clientes conectados.

**CA-001-02: Pedido sin productos**
- **Dado** que el usuario está en la pantalla de POS
- **Cuando** intenta confirmar un pedido sin haber seleccionado productos
- **Entonces** el sistema muestra un mensaje de validación: "Debe agregar al menos un producto al pedido" y no se registra el pedido.

**CA-001-03: Pedido con mesa ocupada**
- **Dado** que el usuario selecciona una mesa que ya tiene un pedido activo
- **Cuando** confirma el nuevo pedido
- **Entonces** el sistema permite registrar el pedido adicional en la misma mesa (una mesa puede tener múltiples pedidos activos).

**CA-001-04: Usuario no autenticado**
- **Dado** que un usuario no ha iniciado sesión
- **Cuando** intenta acceder a la página de POS
- **Entonces** el sistema redirige al inicio de sesión.

---

### CA-002: Modificar pedido no pagado — HU-002

**CA-002-01: Agregar producto a pedido existente**
- **Dado** que existe un pedido en estado "Pendiente" o "En preparación" no pagado
- **Cuando** el usuario agrega un nuevo producto con cantidad ≥ 1
- **Entonces** el pedido se actualiza, el total se recalcula, y se notifica el cambio vía SignalR.

**CA-002-02: Pedido ya pagado**
- **Dado** que existe un pedido en estado "Pagado"
- **Cuando** el usuario intenta modificarlo
- **Entonces** el sistema muestra el mensaje: "No se puede modificar un pedido que ya fue pagado" y rechaza la operación.

**CA-002-03: Eliminar producto de pedido**
- **Dado** que un pedido pendiente tiene al menos 2 productos
- **Cuando** el usuario elimina uno de los productos
- **Entonces** el pedido se actualiza y el total se recalcula.

**CA-002-04: Eliminar último producto**
- **Dado** que el pedido tiene exactamente 1 producto
- **Cuando** el usuario intenta eliminar ese producto
- **Entonces** el sistema advierte: "El pedido debe tener al menos un producto. Si desea cancelar, elimine el pedido completo."

---

### CA-003: Eliminar pedido pendiente no pagado — HU-003

**CA-003-01: Pedido pendiente eliminado**
- **Dado** que existe un pedido en estado "Pendiente" no pagado
- **Cuando** un usuario con rol "Mesero" o "Administrador" confirma la eliminación
- **Entonces** el pedido se elimina del sistema (borrado lógico o físico según diseño) y la mesa asociada queda libre si no tiene otros pedidos.

**CA-003-02: Pedido pagado**
- **Dado** que existe un pedido en estado "Pagado"
- **Cuando** el usuario intenta eliminarlo
- **Entonces** el sistema rechaza la operación con el mensaje: "No se puede eliminar un pedido pagado."

**CA-003-03: Pedido en preparación**
- **Dado** que existe un pedido en estado "En preparación"
- **Cuando** el usuario intenta eliminarlo
- **Entonces** el sistema rechaza la operación con el mensaje: "No se puede eliminar un pedido que ya está en preparación."

---

### CA-011: Gestionar productos — HU-011

**CA-011-01: Crear producto**
- **Dado** que el usuario es "Administrador"
- **Cuando** completa el formulario con nombre, descripción, precio > 0 y categoría
- **Entonces** el producto se registra en el sistema y aparece en la lista de productos.

**CA-011-02: Validación de precio**
- **Dado** que el usuario ingresa un precio ≤ 0 o no numérico
- **Cuando** intenta guardar el producto
- **Entonces** el sistema muestra: "El precio debe ser un valor mayor que cero."

**CA-011-03: Editar producto**
- **Dado** que existe un producto registrado
- **Cuando** el administrador modifica su nombre, precio o categoría
- **Entonces** los cambios se reflejan inmediatamente. Si el producto está en pedidos activos, el precio no afecta pedidos ya registrados.

**CA-011-04: Eliminar producto con pedidos activos**
- **Dado** que un producto está incluido en al menos un pedido activo
- **Cuando** el administrador intenta eliminarlo
- **Entonces** el sistema muestra: "No se puede eliminar el producto porque está en pedidos activos" y rechaza la operación.

**CA-011-05: Eliminar producto sin dependencias**
- **Dado** que un producto no está en ningún pedido activo
- **Cuando** el administrador confirma la eliminación
- **Entonces** el producto se elimina del catálogo.

---

### CA-014: Recetas de productos — HU-014

**CA-014-01: Asociar receta**
- **Dado** que existe un producto sin receta asociada
- **Cuando** el usuario "Administrador" o "Chef" ingresa ingredientes con cantidades e instrucciones
- **Entonces** la receta queda vinculada al producto y es visible en el detalle del producto.

**CA-014-02: Producto sin receta**
- **Dado** que un producto no tiene receta
- **Cuando** se visualiza el detalle del producto
- **Entonces** el sistema muestra: "Este producto no tiene receta asociada."

---

### CA-016: Gestión de mesas — HU-016

**CA-016-01: Crear mesa**
- **Dado** que el usuario es "Administrador"
- **Cuando** ingresa un número o identificador de mesa
- **Entonces** la mesa se registra con estado "Libre".

**CA-016-02: Cambiar estado de mesa**
- **Dado** que existe una mesa en estado "Libre"
- **Cuando** el administrador la cambia a "Ocupada", "Reservada" o "En mantenimiento"
- **Entonces** el estado se actualiza. Si la mesa está "Ocupada" y tiene pedidos activos, se advierte antes de cambiar a "Libre".

**CA-016-03: Eliminar mesa con pedidos activos**
- **Dado** que la mesa tiene al menos un pedido activo
- **Cuando** se intenta eliminar
- **Entonces** el sistema rechaza la operación: "No se puede eliminar una mesa con pedidos activos."

---

### CA-021: Gestión de usuarios y roles — HU-021

**CA-021-01: Crear usuario**
- **Dado** que el usuario es "Administrador"
- **Cuando** completa el formulario con nombre de usuario, contraseña ≥ 8 caracteres y rol
- **Entonces** el usuario se registra con la contraseña encriptada (BCrypt) y el rol asignado.

**CA-021-02: Cambiar rol**
- **Dado** que un usuario tiene el rol "Mesero"
- **Cuando** el administrador le asigna el rol "Cajero"
- **Entonces** los permisos del usuario cambian inmediatamente. Si el usuario tiene sesión activa, se le fuerza a cerrar sesión.

**CA-021-03: Desactivar usuario**
- **Dado** que un usuario existe en el sistema
- **Cuando** el administrador lo desactiva (borrado lógico)
- **Entonces** el usuario no puede iniciar sesión. Sus registros históricos (pedidos) se conservan.

---

### CA-025: Inicio de sesión, RBAC y CSRF — HU-025

**CA-025-01: Inicio de sesión exitoso**
- **Dado** que un usuario registrado ingresa credenciales correctas
- **Cuando** envía el formulario
- **Entonces** el sistema crea una sesión y redirige al dashboard correspondiente a su rol.

**CA-025-02: Credenciales incorrectas**
- **Dado** que un usuario ingresa credenciales incorrectas
- **Cuando** envía el formulario
- **Entonces** el sistema muestra un mensaje genérico: "Credenciales inválidas" (sin especificar si falló el usuario o la contraseña).

**CA-025-03: Bloqueo por intentos fallidos**
- **Dado** que un usuario falla el inicio de sesión 5 veces consecutivas
- **Cuando** intenta una sexta vez
- **Entonces** la cuenta se bloquea temporalmente (15 minutos) y se muestra: "Cuenta bloqueada por múltiples intentos fallidos. Intente de nuevo más tarde."

**CA-025-04: Acceso por rol — Cajero**
- **Dado** que un usuario con rol "Cajero" inicia sesión
- **Cuando** navega por el sistema
- **Entonces** ve las opciones de POS y pedidos, pero NO ve las páginas de gestión de usuarios, ni de configuración.

**CA-025-05: Acceso por rol — Cocinero**
- **Dado** que un usuario con rol "Cocinero" inicia sesión
- **Cuando** navega por el sistema
- **Entonces** ve los pedidos activos y sus recetas, pero NO puede modificar productos, mesas ni usuarios.

**CA-025-06: Protección CSRF en formulario POST**
- **Dado** que un usuario está autenticado
- **Cuando** se envía un formulario POST sin el token CSRF (o con un token inválido)
- **Entonces** el servidor rechaza la solicitud con HTTP 400.

**CA-025-07: Cierre de sesión**
- **Dado** que un usuario tiene sesión activa
- **Cuando** hace clic en "Cerrar sesión"
- **Entonces** la cookie de sesión se invalida y es redirigido a la página de inicio de sesión.

**CA-025-08: Sesión expirada**
- **Dado** que la sesión de un usuario ha expirado por inactividad
- **Cuando** intenta acceder a cualquier página que requiera autenticación
- **Entonces** es redirigido al inicio de sesión.

---

## 3. Resumen de cobertura

| HU    | Cantidad de criterios |
|-------|-----------------------|
| HU-001| 4                     |
| HU-002| 4                     |
| HU-003| 3                     |
| HU-011| 5                     |
| HU-014| 2                     |
| HU-016| 3                     |
| HU-021| 3                     |
| HU-025| 8                     |
| **Total** | **32**          |

---

### CA-000: Arquitectura base del dominio

Validación inicial del modelo de dominio con entidades, enumeraciones y reglas de negocio. Cubierto en `docs/arquitectura/modelo-dominio.md`.

**CA-000-01: Invariantes de entidad**
- **Dado** que se instancia cualquier entidad del dominio
- **Cuando** se proporcionan datos inválidos (nulos, vacíos, negativos)
- **Entonces** se lanza `ReglaDominioException` con un mensaje descriptivo.

**CA-000-02: Estados iniciales**
- **Dado** que se crea una entidad sin especificar estado
- **Cuando** se consulta su estado
- **Entonces** la entidad tiene el estado por defecto definido (Mesa→Disponible, Pedido→Abierto, entidades→Activo).

---

**Versión**: 1.0 | **Fecha**: Abril 2026 | **Actualizado**: Mayo 2026 — dominio base | **Actualizar con cada HU nueva**
