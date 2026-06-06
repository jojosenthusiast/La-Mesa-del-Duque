# Auditoría integral — La Mesa del Duque
> Fecha: Mayo 2026 | Rama: `feat/sprint3-inv-ingredientes-kds` | Tests: 367/367

---

## 1. Bugs CRÍTICOS corregidos durante la auditoría

| # | Bug | Severidad | Fix |
|---|-----|-----------|-----|
| 1 | **11 servicios no registrados en DI** — `InyeccionAplicacion.cs` perdió todos los registros de Fase A/B en el merge del dashboard. Inventario, Cierre, Despacho, Mermas, Alergenos, etc. rotos en runtime. | 🔴 CRÍTICO | Restaurados los 11 registros |
| 2 | **4 repos no registrados en DI** — `ProveedorRepositorio`, `MermaRepositorio`, `CierreDiaRepositorio`, `AlergenoRepositorio` sin registro. | 🔴 CRÍTICO | Agregados a `InyeccionInfraestructura.cs` |
| 3 | **Despacho sin link en sidebar** — la página existe pero no se puede navegar a ella desde el menú. | 🔴 CRÍTICO | Agregado al `_Layout.cshtml` |
| 4 | **Dashboard y Mapa sin link en home** — accesibles solo por sidebar, no desde la página principal. | 🟡 ALTO | Agregados a `ModuleLinks` en `Index.cshtml.cs` |

---

## 2. Estado de pantallas

| Pantalla | Ruta | Auth | Sidebar | Home | Estado |
|----------|------|------|---------|------|--------|
| Home | `/Index` | Auth | — | — | ✅ Completo |
| Pedidos POS | `/Operaciones/Pedidos/Index` | Admin,Encargado,Mesero | ✅ | ✅ | ✅ Completo (SPA 4 pantallas) |
| Tableside | `/Operaciones/Pedidos/Tableside` | Mesero,Encargado,Admin | ❌ | ❌ | ⚠️ Sin link (app standalone) |
| Cocina KDS | `/Cocina/KDS` | Cocinero,Encargado,Admin | ✅ | ✅ | ✅ Completo |
| Mesas | `/Operaciones/Mesas/Index` | Admin,Encargado,Mesero | ✅ | ✅ | ✅ Completo |
| Mapa Salón | `/Operaciones/Salon/Mapa` | Admin,Encargado,Mesero | ✅ | ✅ (nuevo) | ✅ Completo |
| Productos | `/Operaciones/Productos/Index` | Admin,Encargado | ✅ | ✅ | ✅ Completo |
| Inventario | `/Operaciones/Inventario/Index` | Admin,Encargado | ✅ | ✅ | ✅ CRUD completo |
| Cierre | `/Operaciones/Cierre/Index` | Admin,Encargado | ✅ | ✅ | ✅ Con consultas reales |
| Despacho | `/Operaciones/Despacho/Index` | Admin,Encargado,Mesero | ✅ (nuevo) | ✅ | ✅ Nuevo |
| Dashboard | `/Admin/Dashboard/Dashboard` | Admin,Encargado | ✅ | ✅ (nuevo) | ✅ KPIs + Chart.js |
| Usuarios | `/Admin/Usuarios/Index` | Admin | ✅ | ✅ | ✅ Completo |
| Login | `/Auth/Login` | Público | — | — | ✅ Funcional |

---

## 3. Servicios — estado

| Servicio | Interfaz | DI | Usado por |
|----------|----------|-----|-----------|
| CatalogoProductos | ✅ | ✅ | POS, Tableside, Productos |
| RecetasProductos | ✅ | ✅ | POS (modificadores) |
| Mesas | ✅ | ✅ | POS, Mesas, Mapa, Despacho |
| Pedidos | ✅ | ✅ | POS, Tableside, Despacho, Dashboard |
| Usuarios | ✅ | ✅ | Login, Admin/Usuarios |
| Cocina | ✅ | ✅ | KDS, POS (auto-generar) |
| ZonasSalon | ✅ | ✅ | Mapa Salón |
| Metrica | ✅ | ✅ | Dashboard |
| AlertaStock | ✅ | ✅ (fix) | POS |
| Ticket | ✅ | ✅ (fix) | POS (documentos) |
| Alergeno | ✅ | ✅ (fix) | POS (modificadores) |
| TableTimer | ✅ | ✅ (fix) | Mesas |
| Upsell | ✅ | ✅ (fix) | — |
| ShiftHandoff | ✅ | ✅ (fix) | Cierre |
| Inventario | ✅ | ✅ (fix) | Inventario |
| Margen | ✅ | ✅ (fix) | — |
| Merma | ✅ | ✅ (fix) | Cierre, Inventario |
| Cierre | ✅ | ✅ (fix) | Cierre |
| Despacho | ✅ | ✅ (fix) | Despacho |

---

## 4. Lo que FUNCIONA (verificado)

- ✅ 17 pantallas/páginas funcionales
- ✅ 20 servicios registrados en DI
- ✅ 16 repositorios registrados
- ✅ SignalR con PedidosHub (KDS, POS, Mapa, Dashboard)
- ✅ 4 pantallas POS (selección → productos → pago → documentos)
- ✅ CRUD inventario (ingredientes, proveedores, mermas)
- ✅ KDS multi-estación con timers y escalación
- ✅ Cierre de día con consultas reales a Pagos/Pedidos
- ✅ Despacho con auto-Listo desde KDS y soporte Para Llevar
- ✅ Mapa visual con drag-drop y zonas
- ✅ Dashboard con KPIs Chart.js
- ✅ Modificadores de ingredientes con alérgenos
- ✅ Seed data: 8 alérgenos, 3 proveedores, 15 ingredientes, 3 recetas
- ✅ Migración consolidada única
- ✅ 367 tests — 0 errores
- ✅ Bootstrap Icons en toda la app
- ✅ No `alert()`/`confirm()` residuales

---

## 5. Lo que FALTA — priorizado

### 🔴 Crítico (bloquea operación real)

| # | Qué falta | Esfuerzo |
|---|-----------|----------|
| 1 | **No hay rol "Runner"** — Despacho usa Mesero. Debería existir un rol específico. | 1h |
| 2 | **POS: pago con tarjeta simulado** — `procesarPago('tarjeta')` solo muestra toast y llama `finalizarPago` sin procesar nada real. | 4h |
| 3 | **POS: QR/Crédito/Vale simulados** — mismos métodos, misma simulación. | 4h |

### 🟡 Alto (limita funcionalidad)

| # | Qué falta | Esfuerzo |
|---|-----------|----------|
| 4 | **Tableside sin link de navegación** — es standalone, no aparece en sidebar ni home. Solo accesible por URL directa. | 0.5h |
| 5 | **Dashboard no tiene try/catch** — si falla `IMetricaServicio`, la página crashea sin toast. | 0.5h |
| 6 | **Mapa Salón no tiene toast handling** — errores silenciosos. | 0.5h |
| 7 | **Email/PDF real para tickets** — hoy solo genera HTML, no envía ni descarga PDF real. | 6h |
| 8 | **WhatsApp API real** — hoy es simulación. | 8h |
| 9 | **Pagos locales reales** — Pix, Nequi, MercadoPago simulados. | 10h |

### 🟢 Medio (mejora experiencia)

| # | Qué falta | Esfuerzo |
|---|-----------|----------|
| 10 | **Rate limiting + QR tokens** — documentado como MVP+. | 4h |
| 11 | **Dark mode** — en rama separada (`feat/sprint3-slice5-dark-mode`), no mergeado. | 2h |
| 12 | **QR guest ordering** — en rama separada (`feat/sprint3-slice3-qr-guest-ordering`), no mergeado. | 4h |
| 13 | **Favoritos/Quick Reorder** — en rama separada (`feat/sprint3-slice4-favoritos-quick-reorder`), no mergeado. | 3h |
| 14 | **Foto de productos** — en rama `feat/sprint2-photo-menu`, no mergeado. | 2h |
| 15 | **POS offline support** — Service Worker existe (`sw.js`) pero no cachea datos. | 3h |
| 16 | **Tests de integración end-to-end** — flujo POS → Cocina → Despacho → Cierre no tiene tests. | 6h |

### ⚪ Bajo (nice to have)

| # | Qué falta | Esfuerzo |
|---|-----------|----------|
| 17 | **Internacionalización (i18n)** — todo hardcodeado en español. | 20h |
| 18 | **Logging estructurado** — solo `Console.WriteLine`. | 3h |
| 19 | **Health checks** — no hay endpoint de health. | 1h |
| 20 | **Docker** — no hay Dockerfile. | 2h |

---

## 6. Deuda técnica identificada

| # | Deuda | Impacto |
|---|-------|---------|
| 1 | `PedidosServicio` tiene 400+ líneas — debería splitearse. | Mantenibilidad |
| 2 | `pos.js` tiene 550 líneas inline — sin módulos. | Mantenibilidad |
| 3 | `marca.css` tiene 55 KB monolítico. | Performance |
| 4 | Varias ramas con features sin mergear (dark mode, QR, favoritos, foto). | Inconsistencia |
| 5 | No hay separación de comandos/queries (CQRS). | Arquitectura |
| 6 | Validación duplicada entre cliente (JS) y servidor (C#). | Mantenibilidad |
| 7 | `UnidadDeTrabajo` con 17 parámetros — necesita refactor a factories. | Acoplamiento |

---

## 7. Recomendación de próximos pasos

**Semana 1 — Estabilizar para demo:**
1. Agregar link de Tableside al home
2. Agregar try/catch al Dashboard y Mapa
3. Arreglar simulación de pagos (tarjeta → al menos mostrar número de referencia)

**Semana 2 — Features pendientes:**
4. Mergear Dark mode
5. Mergear QR guest ordering
6. Mergear Favoritos

**Semana 3 — Maduración:**
7. Email/PDF real
8. Tests de integración
9. Docker + health checks
