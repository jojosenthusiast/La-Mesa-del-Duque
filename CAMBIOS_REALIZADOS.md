# Cambios realizados — Mesero / Salón / Pedidos

## Mapa del salón
- Se habilitó el cambio de estado de mesa para el rol Mesero.
- El panel permite cambiar entre Disponible, Ocupada, Reservada y Mantenimiento.
- Los cambios aplican el color del estado inmediatamente en el mapa.
- Se agregó el botón superior **Salir al login**.
- Los cambios de estado y posición se notifican por SignalR para sincronizar otras vistas.

## POS Mesero
- Se corrigió la franja superior blanca: ahora tiene fondo oscuro y botones visibles.
- Se agregó botón superior **Salir al login**.
- Se corrigió la visualización de productos del catálogo en el modal de agregar productos.
- Se agregó el botón **Cerrar** junto al total de la mesa.
- El botón **Cerrar** pide confirmación, marca el pedido como **En cobro**, envía la cuenta a caja y cambia la mesa a **Mantenimiento**.
- El total del pedido se actualiza con las cantidades agregadas, eliminadas o modificadas.
- Se sincronizan cambios de pedido y mesa por SignalR.

## Pedidos / Caja
- Se agregó acceso desde Operaciones a **Mesas ocupadas**.
- La vista de pedidos puede abrirse filtrada para ver solo mesas con pedido activo.
- Se agregó botón de **Notificaciones** para ver mesas con pedidos en estado **Listo**.
- Los pedidos en estado **Listo** se mantienen visibles en las listas activas para poder revisarlos/cobrarlos.

## Envío a cocina
- El flujo existente ya genera órdenes de cocina al crear pedidos o agregar ítems.
- Se reforzó la sincronización visual con eventos SignalR cuando se crea o modifica un pedido.

## Verificación posible en este entorno
- Validación de sintaxis JavaScript realizada con `node --check` en:
  - `wwwroot/js/pos.js`
  - `wwwroot/js/mesero.js`
  - `wwwroot/js/mapa-salon.js`
- No se pudo ejecutar `dotnet build` porque el entorno no tiene instalado el SDK de .NET.
