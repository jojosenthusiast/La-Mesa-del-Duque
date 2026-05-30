# Runbook de defensa — simulación en vivo de 30 minutos

Este runbook está diseñado para la simulación y defensa del sistema La Mesa del Duque. Debe practicarse antes de la presentación real y ajustarse con capturas reales del entorno final.

## Objetivo de la defensa

Demostrar que el sistema gestiona un flujo gastronómico completo con roles separados, inventario controlado, operación en tiempo real, usabilidad adecuada y evidencia de calidad.

## Cuentas demo

| Rol | Usuario | Contraseña | Ruta inicial esperada |
|---|---|---|---|
| Cajero | `sofia` | `Cajero567!` | POS / Pedidos |
| Cocina | `pedro` | `Cocina456!` | KDS / Cocina |
| Despacho | `ana` | `Despacho901!` | Despacho |
| Administrador | `admin` | `Admin123!` | Dashboard administrativo |

> Antes de la defensa, confirmar que estas credenciales siguen vigentes en el ambiente final.

## Preparación antes de iniciar

- [ ] App abierta en 4 pestañas o 4 navegadores separados.
- [ ] Cada pestaña ya está logueada con un rol distinto.
- [ ] Zoom del navegador ajustado para que se vea bien en proyector.
- [ ] Datos seed cargados: productos, ingredientes, recetas, mesas y usuarios.
- [ ] Al menos un producto con stock suficiente.
- [ ] Al menos un ingrediente preparado para demo de stock bajo/agotado.
- [ ] Jira abierto en una pestaña auxiliar.
- [ ] PDF/documento final abierto o disponible.
- [ ] Plan B listo si falla internet o base de datos.

## Estructura de 30 minutos

### Minuto 0–3 — Apertura y propósito

**Quién habla:** líder del equipo.

**Mensaje clave:**

> “La Mesa del Duque es un sistema de gestión gastronómica diseñado para operar con roles reales: caja, cocina, despacho y administración. No buscamos acumular funciones decorativas; priorizamos fitness for purpose: que el sistema resuelva el flujo operativo de un restaurante con claridad, integridad y trazabilidad.”

**Mostrar:** pantalla inicial o dashboard.

**Puntos a mencionar:**

- Un sistema, roles separados.
- Flujo en tiempo real.
- Inventario por recetas.
- Adaptabilidad a comida rápida, mesa, para llevar y delivery/despacho.

### Minuto 3–8 — Cajero / POS

**Rol:** Cajero (`sofia`).

**Acciones:**

1. Crear pedido `Comer aquí` con mesa disponible.
2. Agregar productos al pedido.
3. Mostrar total y ticket previo/post pago si aplica.
4. Procesar pago en efectivo o tarjeta.
5. Confirmar que la orden pasa a cocina.

**Si Delivery ya está implementado:**

6. Crear pedido `Delivery` breve con nombre, teléfono y dirección.
7. Mostrar diferencia respecto a `Para llevar`.

**Mensaje clave:**

> “Caja no solo cobra: valida disponibilidad, evita vender productos imposibles de preparar y envía la comanda al flujo operativo correcto.”

**Evidencia de rúbrica:**

- Usabilidad de caja.
- Control de stock antes de venta.
- Diferenciación de modalidad.

### Minuto 8–13 — Cocina / KDS

**Rol:** Cocina (`pedro`).

**Acciones:**

1. Mostrar que el pedido llegó sin recargar la página.
2. Leer número de pedido, productos, cantidades, modalidad y notas.
3. Marcar pedido como `Listo`.
4. Mostrar que desaparece de cocina o cambia de estado según UI.

**Mensaje clave:**

> “Cocina trabaja con una pantalla operacional, no con reportes administrativos. La prioridad aquí es claridad, legibilidad y actualización en tiempo real.”

**Evidencia de rúbrica:**

- Simulación de roles en tiempo real.
- WebSocket/SignalR.
- Usabilidad a distancia.

### Minuto 13–18 — Despacho

**Rol:** Despacho (`ana`).

**Acciones:**

1. Mostrar pedido listo recibido desde cocina.
2. Confirmar información de entrega o retiro.
3. Marcar como `Despachado`/entregado.
4. Si el pedido tenía mesa, verificar liberación o cambio de estado.

**Mensaje clave:**

> “Despacho cierra el ciclo operativo: evita que cocina y caja pierdan visibilidad sobre qué ya fue entregado y qué sigue pendiente.”

**Evidencia de rúbrica:**

- Interacción entre roles.
- Control del estado de pedido.
- Flujo de entrega.

### Minuto 18–23 — Administración / Gerencia

**Rol:** Administrador (`admin`) o Gerente si existe demo.

**Acciones:**

1. Mostrar dashboard/resumen operativo.
2. Mostrar productos, ingredientes y recetas.
3. Mostrar stock bajo o alerta de inventario.
4. Mostrar cierre de día/caja si está listo.
5. Mostrar usuarios/roles solo si suma a la defensa.

**Mensaje clave:**

> “Administración no está para operar cocina; está para tomar decisiones, controlar catálogo, inventario, roles, ventas y cierre.”

**Evidencia de rúbrica:**

- Roles segmentados.
- Gestión de productos/stock.
- Cierre/reportes.
- Usabilidad y permisos.

### Minuto 23–27 — Calidad, adaptabilidad y Jira

**Mostrar:** PDF/matriz/Jira.

**Acciones:**

1. Abrir matriz de configuración de negocio.
2. Explicar los 4 modelos:
   - Comida rápida: pedido sin mesa y flujo rápido.
   - Restaurante mesa: mesa, ocupación, liberación.
   - Para llevar: retiro por cliente sin dirección.
   - Delivery/despacho: entrega con datos de contacto/dirección si implementado.
3. Mostrar Jira con historias, estados y evidencia.
4. Mencionar releases `0.x`, `1.x`, `2.x`, `3.x`.

**Mensaje clave:**

> “La calidad no está solo en que compile. Está en que los requisitos sean trazables, los roles estén separados y las decisiones estén documentadas.”

### Minuto 27–30 — Cierre y preguntas

**Mensaje final:**

> “El sistema fue diseñado alrededor del flujo real del restaurante: vender solo lo disponible, comunicar cocina y despacho en tiempo real, separar responsabilidades por rol y entregar evidencia de calidad. Lo importante no es tener pantallas de más, sino que cada pantalla cumpla su propósito operativo.”

**Prepararse para preguntas sobre:**

- Qué pasa si no hay stock.
- Qué pasa si varios cajeros venden al mismo tiempo.
- Diferencia entre despacho y delivery.
- Por qué admin no opera cocina.
- Cómo se evidencia la calidad en Jira.

## Plan B durante la defensa

| Problema | Respuesta operativa |
|---|---|
| Falla internet | Usar capturas/video corto como evidencia y explicar arquitectura esperada. |
| Falla Supabase/BD | Mostrar PDF, SQL final y capturas del flujo probado. |
| SignalR no actualiza | Recargar una vez; si persiste, mostrar captura/video y explicar el flujo implementado. |
| Login de rol falla | Usar admin solo para mostrar evidencia, pero explicar que es plan B, no flujo normal. |
| Producto sin stock bloquea demo principal | Tener producto alternativo con stock suficiente. |
| El evaluador pide Delivery | Mostrar modalidad explícita si ya está implementada; si no, explicar honestamente como mejora planificada y no prometer que existe. |

## Frases clave para defender decisiones

- “Fitness for purpose: cada rol ve lo que necesita para trabajar.”
- “El inventario se descuenta por receta, no manualmente por intuición.”
- “Caja no debe vender lo que cocina no puede preparar.”
- “Administrador decide y audita; no compite con cocina ni despacho.”
- “La trazabilidad está en Jira, auditoría, historial de estados y documentación.”
- “La defensa muestra un flujo real, no pantallas aisladas.”

## Qué NO hacer en vivo

- No crear demasiados productos desde cero si consume tiempo.
- No improvisar datos de delivery si aún no están implementados.
- No prometer que existe concurrencia transaccional si no fue verificada.
- No entrar a módulos técnicos que no suman a la rúbrica.
- No usar admin para operar todo el flujo si existen roles separados.
- No ocultar errores: si aparece un error, explicar recuperación y evidencia.

## Checklist final de ensayo

- [ ] Pedido mesa creado y despachado completo.
- [ ] Pedido para llevar probado.
- [ ] Pedido delivery probado si existe.
- [ ] Stock insuficiente probado o captura lista.
- [ ] KDS probado con actualización en tiempo real.
- [ ] Despacho probado.
- [ ] Dashboard/admin probado.
- [ ] Cierre o reporte probado.
- [ ] PDF final abre correctamente.
- [ ] Jira está ordenado.
- [ ] SQL final está enlazado o anexado.
- [ ] Video corto o manual rápido listo.
