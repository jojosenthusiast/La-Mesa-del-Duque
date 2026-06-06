# Plan Sprint 3 — La Mesa del Duque

> Rama activa: `feat/sprint2` (PR #50 hacia `main`, CI verde)
> Inicio sugerido: mergear PR #50 para que Release Please genere v2.0.0
> Vault: `C:\Users\frenzied\Desktop\SoftwareGestionCalidad`
> Dev repo: `C:\Users\frenzied\Desktop\La-Mesa-del-Duque-bootstrap`

---

## 1. Estado al cierre de Sprint 2

| Métrica | Valor |
|---------|-------|
| Tests | 271/271 (0 fallos) |
| HUs Sprint 1 | 8/8 completo |
| HUs Sprint 2 | 9/9 completo |
| Total HUs | 17 |
| CAs documentados | 59 |
| Release notes | 18 versiones (v0.2.0 → v2.0.0) |
| PRs Sprint 2 | #42 → #50 |
| Documento competitivo | `docs/diseno/investigacion-competitiva.md` |

---

## 2. Prioridades Sprint 3 (basadas en investigación de 30+ sistemas)

### 🔴 CRÍTICO — Lo que el research dice que falta

| # | Feature | % industria | Competidor referencia | Esfuerzo |
|---|---------|-------------|----------------------|----------|
| 1 | **Mapa visual de mesas** (drag & drop) | 80%+ | TouchBistro, Square, Floreant | Alto (~50h) |
| 2 | **Integración WhatsApp Business** | 85% LATAM | Consumer (Brasil), Goomer | Alto (~60h) |
| 3 | **Métodos de pago locales** (Pix, Nequi, MercadoPago) | 100% LATAM | Consumer, Alegra | Alto (~60h) |
| 4 | **QR guest ordering** (cliente escanea y pide) | 40% | Toast, Square, SambaPOS | Medio (~35h) |
| 5 | **Dashboard / Reportes** (KPIs, tendencias) | 100% | Upserve, Lightspeed, Lavu | Medio (~40h) |
| 6 | **Integración delivery** (PedidosYa, Rappi, UberEats) | 47% | Zelty, Square, Consumer | Alto (~50h) |

### 🟠 ALTO — Pulido y experiencia

| # | Feature | Competidor referencia |
|---|---------|----------------------|
| 7 | **Dark mode** (automático al atardecer) | Square |
| 8 | **Favoritos / Repetir último pedido** (one-tap) | Toast |
| 9 | **Facturación electrónica** (DTE El Salvador, CFDI México, DIAN Colombia) | Alegra, Glop, Tango Restô |
| 10 | **Lealtad / CRM** (puntos, visitas, segmentación) | TouchBistro, Loyverse |

### 🟡 MEDIO — Madurez del sistema

| # | Feature |
|---|---------|
| 11 | **Mover generación de OrdenCocina** de `CrearPedidoAsync` a `MarcarEnPreparacionAsync` (flujo semántico) |
| 12 | **Landing page pública** (qué ve el cliente antes de login) |
| 13 | **Provisión de Supabase** (base de datos productiva) |
| 14 | **Actualizar docs Sprint 1** (HU-001 a HU-025 de "En desarrollo" a "Implementado") |

---

## 3. Documentos canónicos a leer al empezar

| Orden | Archivo | Contenido |
|-------|---------|-----------|
| 1 | `docs/diseno/investigacion-competitiva.md` | 30+ sistemas analizados, UX patterns, gaps |
| 2 | `docs/requisitos/historias-usuario.md` | 17 HUs (Sprint 1 + 2) |
| 3 | `docs/requisitos/criterios-aceptacion.md` | 59 CAs |
| 4 | `docs/arquitectura/arquitectura-sistema.md` | Capas, patrones, tecnologías |
| 5 | `docs/arquitectura/modelo-dominio.md` | Entidades, reglas de negocio |
| 6 | `docs/diseno/wireframes-base.md` | Wireframes del mapa visual de mesas |
| 7 | `docs/releases/v2.0.0-sprint2-completo.md` | Qué se entregó en Sprint 2 |

---

## 4. Arquitectura actual (punto de partida)

```
.NET 8.0 Razor Pages, 4 capas: Dominio → Aplicación → Infraestructura → Web
SQLite dev / Supabase prod
SignalR (PedidosHub con grupos por pedido y cocina)
Bootstrap 5.3 + vanilla JS (pos.js, cocina-kds.js, tableside.js, sw.js)
271 tests xUnit
```

### Páginas Razor existentes

| Ruta | Rol |
|------|-----|
| `/` (Home) | Autenticado |
| `/Operaciones/Pedidos` (POS) | Admin, Encargado, Mesero |
| `/Operaciones/Pedidos/Tableside` | Mesero, Encargado, Admin |
| `/Cocina/KDS` | Cocinero, Encargado, Admin |
| `/Operaciones/Mesas` | Admin, Encargado, Mesero |
| `/Operaciones/Productos` | Admin, Encargado |
| `/Admin/Usuarios` | Admin |

---

## 5. PRs y branches

| Branch | PR | Estado |
|--------|-----|--------|
| `feat/sprint2` | #50 → `main` | CI verde, pendiente merge |
| `feat/sprint2-kds` | #42 | Mergeado |
| `feat/sprint2-pos-ux` | #43 | Mergeado |
| `feat/sprint2-pago-real` | #44 | Mergeado |
| `feat/sprint2-photo-menu` | #45 | Cerrado (código ya en tracker) |
| `feat/sprint2-split-items` | #46 | Mergeado |
| `feat/sprint2-kds-multi-cook` | #47 | Mergeado |
| `feat/sprint2-modificadores` | #48 | Mergeado |
| `feat/sprint2-offline` | #49 | Mergeado |

---

## 6. Acciones primer día Sprint 3

1. Merge PR #50 (`feat/sprint2` → `main`) — Release Please genera v2.0.0
2. Verificar que Release Please creó release y changelog correctamente
3. Crear rama `feat/sprint3` desde `main`
4. Leer `docs/diseno/investigacion-competitiva.md` para decidir primera feature
5. Priorizar: **mapa visual de mesas** (más impacto en mesero, #1 brecha vs industria)

---

**Fecha**: Mayo 2026
**Versión actual**: v2.0.0 (pendiente merge PR #50)
