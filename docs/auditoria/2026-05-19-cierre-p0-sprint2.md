# Cierre de auditoría P0/P1 — Sprint 1 y Sprint 2

> Fecha: 2026-05-19  
> Base verificada: `feat/sprint2`  
> Runtime de desarrollo: SQLite  
> Suite técnica: `dotnet build`, `dotnet test`

---

## 1. Propósito

Este documento reconcilia la auditoría previa registrada en `observations.md` del vault de calidad con el estado actual del código en `feat/sprint2`.

La auditoría original rechazó el claim de cierre de Sprint 1/Sprint 2 por 7 bugs funcionales y de trazabilidad. Este documento deja evidencia de qué bugs fueron corregidos, cómo se corrigieron y con qué verificación se considera cerrada la observación.

---

## 2. Verificación ejecutada

| Verificación | Resultado |
|--------------|-----------|
| `dotnet build` | ✅ 0 errores |
| `dotnet test` | ✅ 271/271 passing |
| `grep alert(` en `pos.js` | ✅ 0 coincidencias |
| `grep confirm(` en `pos.js` | ✅ 0 coincidencias |
| `grep alert(` en `tableside.js` | ✅ 0 coincidencias |
| `grep window.confirm(` en `site.js` | ✅ 0 coincidencias |

---

## 3. Estado de bugs auditados

### BUG-001 — POS runtime contract mismatch breaks tables/products

**Estado anterior**
- Mesas renderizaban `undefined undefinedp`
- Productos renderizaban `undefined $NaN undefinedmin`
- Runtime emitía PascalCase y el JS esperaba camelCase

**Corrección aplicada**
- Serialización JSON corregida a camelCase en:
  - `Pages/Operaciones/Pedidos/Index.cshtml`
  - `Pages/Operaciones/Pedidos/Tableside.cshtml`

**Evidencia técnica**
- Uso explícito de `JsonNamingPolicy.CamelCase`
- La base SQLite local fue recreada para aplicar el esquema actual y descartar runtime viejo

**Estado**: ✅ **CERRADO**

---

### BUG-002 — Cocinero sees Pedidos but cannot access it

**Estado anterior**
- El cocinero veía `PEDIDOS` en la navegación pero la ruta podía llevarlo a un flujo no permitido

**Corrección aplicada**
- La navegación en `_Layout.cshtml` ahora muestra `PEDIDOS` solo a:
  - `Administrador`
  - `Encargado`
  - `Mesero`
- Se agregó pestaña `COCINA` visible para:
  - `Cocinero`
  - `Encargado`
  - `Administrador`
- El POS (`Operaciones/Pedidos/Index`) quedó restringido con:
  - `[Authorize(Roles = "Administrador,Encargado,Mesero")]`

**Estado**: ✅ **CERRADO**

---

### BUG-003 — Mesero can operate management controls on Mesas

**Estado anterior**
- El mesero podía ver acciones de administración (`Nueva mesa`, cambios de estado, `Desactivar`)

**Corrección aplicada**
- En `Pages/Operaciones/Mesas/Index.cshtml`:
  - el mesero conserva la vista operacional del salón
  - se ocultan botones de creación, cambio de estado y desactivación para `Mesero`
- En `Pages/Operaciones/Mesas/Index.cshtml.cs`:
  - `OnPostGuardarAsync` → `Forbid()` para mesero
  - `OnPostCambiarEstadoAsync` → `Forbid()` para mesero
  - `OnPostDesactivarAsync` → `Forbid()` para mesero

**Estado**: ✅ **CERRADO**

---

### BUG-004 — Product management role does not match Sprint 1 story

**Estado anterior**
- La implementación permitía `Administrador, Encargado`
- La documentación histórica mencionaba solo `Administrador`

**Resolución tomada**
- Se decidió **mantener Encargado** por coherencia operativa real
- Se actualizó la documentación canónica para reflejarlo:
  - `docs/requisitos/historias-usuario.md`
  - `docs/requisitos/criterios-aceptacion.md`

**Estado**: ✅ **CERRADO por alineación documental**

---

### BUG-005 — “No blocking popups” claim is false in code

**Estado anterior**
- `pos.js` tenía múltiples `alert()` / `confirm()`
- `tableside.js` tenía `alert()`
- `site.js` tenía `window.confirm()`

**Corrección aplicada**
- `site.js` ahora expone:
  - `window.lmdToast(message, type)`
  - `window.lmdConfirm(message)` con modal promise-based
- `pos.js` migró errores/éxitos a toasts y confirmaciones a modal no bloqueante
- `tableside.js` migró errores a toasts
- `site.js` ya no usa `window.confirm()` en formularios destructivos

**Verificación**
- `grep` de producción JS → 0 coincidencias de `alert(` y `confirm(`

**Estado**: ✅ **CERRADO**

---

### BUG-006 — Fresh full test/build cannot run while server is up

**Estado anterior**
- `dotnet test` fallaba si `LaMesaDelDuque.Web.exe` estaba usando las DLLs del mismo output folder

**Resolución operativa**
- Se documentó y verificó el flujo correcto de verificación:
  1. detener `dotnet` en ejecución
  2. ejecutar `dotnet build`
  3. ejecutar `dotnet test`
- El problema no era funcional del producto sino operacional del ciclo local de build/test compartiendo output

**Estado**: ✅ **MITIGADO / CERRADO operativamente**

---

### BUG-007 — Non-JSON cash payment handler likely rejects valid payment

**Estado anterior**
- `OnPostPagarEfectivoAsync` consultaba `Vm.PedidosActivos` antes de poblarla
- Un pago válido podía rebotar con “El pedido ya no está activo”

**Corrección aplicada**
- `OnPostPagarEfectivoAsync` ahora consulta pedidos activos desde `_pedidosServicio.ListarPedidosActivosAsync()`
- Ya no depende del estado no inicializado de `Vm`

**Estado**: ✅ **CERRADO**

---

## 4. Hallazgos adicionales cerrados durante la reconciliación

| Hallazgo | Estado |
|----------|--------|
| CSRF faltante en formularios de Productos, Mesas, Usuarios y Logout | ✅ Corregido |
| Lockout mal implementado (5 por minuto) | ✅ Corregido a 5 consecutivos / 15 minutos |
| Estado de HU Sprint 1 aún figuraba “En desarrollo” en docs | ✅ Actualizado a “Implementado” |
| Falta de releases curados intermedios para Sprint 2 | ✅ 8 releases intermedios + `v2.0.0` creados |

---

## 5. Veredicto final

Con la evidencia actual:

- el runtime crítico del POS quedó corregido,
- la navegación y roles quedaron alineados,
- las acciones administrativas ya no quedan expuestas a roles operativos,
- el flujo de pago en efectivo ya no depende de `Vm` no cargado,
- la suite de pruebas está en **271/271 passing**,
- y la documentación canónica ya quedó reconciliada con Sprint 1 y Sprint 2.

### Estado de aceptación

**Sprint 1 y Sprint 2 pueden considerarse cerrados para iniciar Sprint 3.**

---

## 6. Siguiente paso recomendado

Leer `docs/plan-sprint3.md` e iniciar Sprint 3 con:

1. mapa visual de mesas,
2. dashboard/reportes,
3. QR guest ordering.
