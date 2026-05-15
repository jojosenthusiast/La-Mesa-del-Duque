# Historias de Usuario — La Mesa del Duque

## 1. Propósito

Este documento describe las historias de usuario (HU) que definen los requisitos funcionales del sistema **La Mesa del Duque**. Cada historia sigue el formato estándar: *Como [rol], quiero [funcionalidad], para [beneficio]*. Las historias se agrupan por sprint y módulo funcional.

## 2. Sprint 1 — Funcionalidades principales

### HU-001: Registrar pedido en punto de venta (POS)

**Como** mesero o cajero,
**quiero** registrar un pedido seleccionando productos del menú y asignándolo a una mesa,
**para** que el área de cocina reciba la orden y el cliente sea atendido.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-001`.

**Prioridad**: Alta (imprescindible)

---

### HU-002: Modificar pedido no pagado

**Como** mesero o cajero,
**quiero** modificar los productos de un pedido que aún no ha sido pagado (agregar, quitar productos o cambiar cantidades),
**para** corregir errores o atender solicitudes del cliente antes de que se cierre la cuenta.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-002`.

**Prioridad**: Alta

---

### HU-003: Eliminar pedido pendiente no pagado

**Como** mesero o administrador,
**quiero** eliminar un pedido pendiente que no ha sido pagado,
**para** cancelar pedidos erróneos o que el cliente decidió no consumir.

**Restricción**: No se puede eliminar un pedido que ya fue pagado, ni uno que esté en preparación.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-003`.

**Prioridad**: Alta

---

### HU-011: Gestionar productos

**Como** administrador,
**quiero** crear, editar, listar y eliminar productos del menú (con nombre, descripción, precio y categoría),
**para** mantener actualizado el catálogo de productos ofrecidos en el restaurante.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-011`.

**Prioridad**: Alta (imprescindible para el POS)

---

### HU-014: Gestionar recetas de productos

**Como** administrador o chef,
**quiero** asociar recetas a los productos (ingredientes, cantidades, instrucciones),
**para** que el área de cocina sepa cómo preparar cada producto y se pueda controlar el inventario de ingredientes.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-014`.

**Prioridad**: Media

---

### HU-016: Gestionar mesas

**Como** administrador,
**quiero** crear, editar, cambiar el estado (libre, ocupada, reservada) y eliminar mesas,
**para** reflejar la distribución real del restaurante y asignar pedidos correctamente.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-016`.

**Prioridad**: Alta

---

### HU-021: Gestionar usuarios y roles

**Como** administrador,
**quiero** crear, editar y desactivar usuarios del sistema, y asignarles roles (Administrador, Cajero, Mesero, Cocinero),
**para** controlar quién puede acceder al sistema y qué acciones puede realizar.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-021`.

**Prioridad**: Alta (infraestructura de seguridad)

---

### HU-025: Inicio de sesión, control de acceso basado en roles (RBAC) y protección CSRF

**Como** usuario del sistema,
**quiero** iniciar sesión con mi nombre de usuario y contraseña, y que el sistema me muestre solo las funciones que corresponden a mi rol,
**para** acceder de forma segura y realizar únicamente las operaciones autorizadas.

**Como** responsable de seguridad,
**quiero** que todos los formularios que modifican datos estén protegidos contra falsificación de solicitudes (CSRF),
**para** evitar que un atacante externo realice acciones no autorizadas en nombre de un usuario autenticado.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-025`.

**Prioridad**: Alta (crítico para seguridad)

---

## 3. Sprint 2 — Experiencia operativa y pagos

### HU-030: Visualizar pedidos en cocina (KDS)

**Como** cocinero,
**quiero** ver los pedidos activos en una pantalla de cocina con colores por urgencia, sonido de alerta y separación por estación,
**para** priorizar mi trabajo y no perder órdenes.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-KDS`.

**Prioridad**: Alta

---

### HU-031: Pantalla multi-cocinero compartida

**Como** cocinero,
**quiero** que la pantalla de cocina se divida en columnas por cocinero con colores asignados,
**para** saber cuál es mi territorio y no confundir mis órdenes con las de otros.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-KDS`.

**Prioridad**: Alta

---

### HU-032: Modificar ingredientes al pedir

**Como** mesero,
**quiero** ver los ingredientes de cada producto y poder quitarlos con motivo (alergia, preferencia, intercambio) al momento de tomar la orden,
**para** que el cliente reciba exactamente lo que quiere.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-MOD`.

**Prioridad**: Alta

---

### HU-033: Experiencia POS optimizada

**Como** mesero,
**quiero** un POS sin popups bloqueantes, con notificaciones inline, atajos de teclado, indicador de pasos y persistencia de estado,
**para** trabajar más rápido bajo presión.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-UX`.

**Prioridad**: Alta

---

### HU-034: Menú visual con fotos

**Como** mesero,
**quiero** ver fotos de los productos en el catálogo del POS,
**para** identificar platos más rápido que leyendo nombres.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-UX`.

**Prioridad**: Media

---

### HU-035: Pago por cuentas individuales

**Como** mesero,
**quiero** dividir la cuenta en sub-cuentas por persona, cada una con su propio método de pago y propina,
**para** que grupos paguen solo lo que consumieron.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-PAGO`.

**Prioridad**: Alta

---

### HU-036: División de cuenta por ítems

**Como** mesero,
**quiero** asignar ítems específicos del pedido a cuentas individuales,
**para** que cada comensal pague exactamente lo que pidió.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-PAGO`.

**Prioridad**: Alta

---

### HU-037: Vista tableside para tablet

**Como** mesero,
**quiero** una vista optimizada para tablet que me permita tomar pedidos desde la mesa y enviarlos directo a cocina,
**para** agilizar el servicio en sala.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-PAGO`.

**Prioridad**: Media

---

### HU-038: Modo offline

**Como** mesero,
**quiero** que el POS siga funcionando sin internet, guardando pedidos localmente y sincronizando al reconectar,
**para** no detener la operación del restaurante.

**Criterios de aceptación**: Ver `docs/requisitos/criterios-aceptacion.md#CA-OFF`.

**Prioridad**: Alta

---

## 4. Estados de las historias

| HU    | Título                          | Sprint | Estado        |
|-------|----------------------------------|--------|---------------|
| HU-001| Registrar pedido (POS)          | 1      | En desarrollo |
| HU-002| Modificar pedido no pagado      | 1      | En desarrollo |
| HU-003| Eliminar pedido pendiente        | 1      | En desarrollo |
| HU-011| Gestionar productos              | 1      | En desarrollo |
| HU-014| Recetas de productos              | 1      | En desarrollo |
| HU-016| Gestión de mesas                  | 1      | En desarrollo |
| HU-021| Gestión de usuarios y roles      | 1      | En desarrollo |
| HU-025| Login, RBAC, CSRF                 | 1      | En desarrollo |
| HU-030| KDS — Visualizar pedidos cocina   | 2      | Implementado   |
| HU-031| KDS — Pantalla multi-cocinero     | 2      | Implementado   |
| HU-032| Modificadores de ingredientes     | 2      | Implementado   |
| HU-033| Experiencia POS optimizada        | 2      | Implementado   |
| HU-034| Menú visual con fotos             | 2      | Implementado   |
| HU-035| Pago por cuentas individuales     | 2      | Implementado   |
| HU-036| División de cuenta por ítems      | 2      | Implementado   |
| HU-037| Vista tableside para tablet       | 2      | Implementado   |
| HU-038| Modo offline                      | 2      | Implementado   |

---

**Versión**: 2.0 | **Fecha**: Mayo 2026 | **Actualizar en cada sprint**
