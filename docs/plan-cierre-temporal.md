# Plan de cierre — Lo temporal y lo que falta

> Rama: `feat/sprint3-inv-ingredientes-kds`  
> Tests: 295/295 | Build: 0 errores  
> Fecha: Mayo 2026

---

## 1. Lo TEMPORAL que necesita fix

| # | Qué | Por qué es temporal | Fix necesario |
|---|-----|-------------------|---------------|
| 1 | **CierreServicio simplificado** | No calcula totales reales del sistema. Solo guarda lo que el usuario escribe. | Restaurar consultas a pagos/pedidos reales del día, con manejo de errores cuando las tablas no tengan datos. |
| 2 | **Pedidos van a cocina al pagar** | `PagarPedidoAsync` llama `GenerarOrdenesAsync`. El flujo correcto es: crear pedido → marcar en preparación → cocina. | Mover `GenerarOrdenesAsync` a `CrearPedidoAsync` o al handler de "Listo" en el POS. Ya está en `CrearPedidoAsync` también — verificar que no se duplique. |
| 3 | **Migrations hand-crafted** | Varias migraciones fueron escritas a mano (`MesaOcupadaDesde`, `MermaTipoLote`, `AgregarAlergenos`, etc.) en vez de generadas por EF. | Regenerar una migración consolidada con `dotnet ef migrations add` que capture todo el estado actual. |
| 4 | **CargarDatosAsync con try/catch vacío** | El POS traga errores silenciosamente. Si falla la carga de mesas/productos, la UI queda vacía sin aviso. | Mostrar toast de error en la UI cuando falle. |
| 5 | **POS sin validación server-side completa** | El `OnPostCrearJsonAsync` del POS crea pedidos pero no valida stock, disponibilidad real de mesa, ni precios actualizados. | Agregar validación completa en el handler. |
| 6 | **finalizarPago no valida monto** | El keypad del efectivo acepta cualquier monto y llama `PagarJson`. No hay validación de que el pedido esté activo. | Validar que el pedido existe y está en estado correcto antes de pagar. |
| 7 | **alert()/confirm() residuales** | `pos.js` usa `lmdToast`/`lmdConfirm` pero `window.confirm` podría aparecer en otros JS. | Revisión global de todos los JS. |
| 8 | **BD SQLite se borró manualmente** | Para aplicar nuevas columnas se borró la BD. En producción esto no es viable. | Ejecutar migraciones con `dotnet ef database update`. |

---

## 2. Lo que FALTA implementar

| # | Feature | Prioridad | Esfuerzo |
|---|---------|-----------|----------|
| 1 | **CRUD real en Inventario** — formularios de crear/editar ingredientes y proveedores que persistan | 🔴 Crítico | 4h |
| 2 | **Página de Despacho** — donde el runner ve pedidos listos y libera mesas al entregar | 🔴 Crítico | 6h |
| 3 | **Seed data de alérgenos por producto** — `ProductoAlergeno` no tiene datos semilla. Sin esto, el modal de modificadores no muestra alérgenos reales. | 🟡 Alto | 2h |
| 4 | **Seed data de ingredientes/recetas** — productos sin recetas no descuentan stock. | 🟡 Alto | 3h |
| 5 | **Dashboard funcional** — los KPIs existen en Slice 2 pero no están en esta rama. | 🟡 Alto | 4h |
| 6 | **Mapa visual de mesas** — Slice 1 completo en otra rama. Mergear a esta. | 🟡 Alto | 2h |
| 7 | **Email/PDF real para tickets** — hoy solo genera HTML. | 🟢 Medio | 4h |
| 8 | **WhatsApp API real** — hoy es simulación. | 🟢 Medio | 6h |
| 9 | **Métodos de pago locales reales** — hoy Pix/Nequi/MP son simuladores. | 🟢 Medio | 8h |
| 10 | **Rate limiting y QR tokens** — documentados como MVP+ en Slice 3. | 🟢 Bajo | 4h |

---

## 3. Plan de acción post-chat

```
Fase A — Estabilizar (1-2 días)
├── A1: Regenerar migración consolidada con dotnet ef migrations add
├── A2: Restaurar CierreServicio con consultas reales (con fallback)
├── A3: Fix CargarDatosAsync para mostrar errores en UI
├── A4: Seed data ProductoAlergeno + ingredientes + recetas
└── A5: CRUD funcional en Inventario

Fase B — Completar flujos core (3-5 días)
├── B1: Página de Despacho (runner ve pedidos listos, libera mesas)
├── B2: Validación server-side completa en handlers del POS
├── B3: Mergear Mapa visual (Slice 1) y Dashboard (Slice 2)
└── B4: Revisión global de alert()/confirm() residuales

Fase C — Madurar (1-2 semanas)
├── C1: Email/PDF real para tickets
├── C2: WhatsApp API real (cambiar simulador)
├── C3: Pagos locales reales (Pix, Nequi, MercadoPago)
├── C4: Rate limiting + QR tokens
└── C5: Tests de integración para flujos core (POS → Cocina → Despacho → Cierre)
```

---

## 4. Lo que SÍ funciona

- ✅ POS 4 pantallas (selección → productos → pago → documentos)
- ✅ Categorías, productos, carrito, keypad efectivo
- ✅ Modal de modificadores con alérgenos por producto
- ✅ Crear pedido → SignalR → cocina (vía `GenerarOrdenesAsync`)
- ✅ KDS con estaciones, timers, colores, agrupación por mesa, undo, escalación
- ✅ Cierre de día (abrir/cerrar con efectivo/tarjeta)
- ✅ Home con navegación por roles
- ✅ Inventario (lectura ingredientes, proveedores, mermas del día)
- ✅ Stock deduction al pagar
- ✅ Dark mode (en Slice 5, rama separada)
- ✅ QR guest ordering (en Slice 3, rama separada)
- ✅ Favoritos/Quick Reorder (en Slice 4, rama separada)
- ✅ 295 tests pasando, 0 errores de build
