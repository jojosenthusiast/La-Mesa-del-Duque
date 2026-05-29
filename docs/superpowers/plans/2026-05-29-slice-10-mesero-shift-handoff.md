# Slice 10 Mesero Table Ownership / Shift Handoff Baseline Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a minimal, auditable table-owner baseline so Mesero users only see their own active tables and can transfer an active table/order to another Mesero during shift handoff.

**Architecture:** Ownership belongs to the active `Pedido`, not to `Mesa`, because the business unit being transferred is the current table service/order and the same physical table must be reusable across shifts. `Pedido.MeseroAsignadoId` is nullable for legacy and non-table orders; `ShiftHandoffServicio` queries active table orders through repositories and writes a manual `Auditoria` row when ownership changes. The web layer adds a small handoff page and a Mesero landing link without rewriting the large POS JavaScript surface.

**Tech Stack:** ASP.NET Core Razor Pages, EF Core 8, Npgsql/SQLite test provider, xUnit, existing Domain/Application/Infrastructure layering.

---

## File Structure

- Modify `src/LaMesaDelDuque.Dominio/Entidades/Pedido.cs` — add `MeseroAsignadoId` and domain method `AsignarMesero`.
- Modify `src/LaMesaDelDuque.Dominio/Repositorios/IPedidoRepositorio.cs` — add active-owner query contracts.
- Modify `src/LaMesaDelDuque.Infraestructura/Repositorios/PedidoRepositorio.cs` — implement active-owner and active-table tracked queries.
- Modify `src/LaMesaDelDuque.Infraestructura/Persistencia/Configuraciones/PedidoConfiguracion.cs` — map nullable FK/index to `Usuarios`.
- Modify `src/LaMesaDelDuque.Aplicacion/Dtos/PedidoDto.cs` and `src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs` — assign table orders to authenticated Mesero users and expose ownership in DTOs.
- Replace `src/LaMesaDelDuque.Aplicacion/Servicios/ShiftHandoffServicio.cs` — remove stub behavior and persist/audit transfers.
- Create `src/LaMesaDelDuque.Web/Pages/Operaciones/Mesero/Handoff.cshtml.cs` and `Handoff.cshtml` — minimal authenticated handoff UI.
- Modify `src/LaMesaDelDuque.Web/Pages/Operaciones/Mesero/Index.cshtml` and `src/LaMesaDelDuque.Web/Pages/Index.cshtml.cs` — add navigation entry points.
- Add tests in `tests/LaMesaDelDuque.Pruebas/Dominio/PedidoMeseroOwnershipTests.cs`, `tests/LaMesaDelDuque.Pruebas/Aplicacion/ShiftHandoffServicioTests.cs`, `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs`, and `tests/LaMesaDelDuque.Pruebas/Web/MeseroHandoffPageTests.cs`.

---

### Task 1: RED tests for domain ownership and handoff behavior

**Files:**
- Create: `tests/LaMesaDelDuque.Pruebas/Dominio/PedidoMeseroOwnershipTests.cs`
- Create: `tests/LaMesaDelDuque.Pruebas/Aplicacion/ShiftHandoffServicioTests.cs`
- Modify: `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs`
- Modify: `tests/LaMesaDelDuque.Pruebas/Aplicacion/TestHttpContextAccessor.cs`

- [ ] **Step 1: Write failing domain tests**

Create tests asserting that a table order can be assigned to a Mesero and `Guid.Empty` is rejected.

- [ ] **Step 2: Write failing application service tests**

Create tests asserting `ObtenerMesasActivasAsync(usuarioId)` filters by `MeseroAsignadoId` and `TransferirMesaAsync(mesaId, nuevoMeseroId, usuarioResponsableId)` changes owner and creates an `Auditoria` row.

- [ ] **Step 3: Write failing creation ownership test**

Add a `PedidosServicioTests` case with an authenticated `Mesero` role claim, create a table order, reload the persisted order, and assert `MeseroAsignadoId == usuario.Id`.

- [ ] **Step 4: Run RED tests**

Run:

```powershell
dotnet test tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj --filter "PedidoMeseroOwnershipTests|ShiftHandoffServicioTests|CrearPedido_ComerAqui_ComoMesero_DebeAsignarMeseroActual" --no-restore
```

Expected: FAIL/compile errors because `MeseroAsignadoId`, repository methods, and the new transfer signature do not exist yet.

---

### Task 2: Implement domain, repository, and application ownership

**Files:**
- Modify: `src/LaMesaDelDuque.Dominio/Entidades/Pedido.cs`
- Modify: `src/LaMesaDelDuque.Dominio/Repositorios/IPedidoRepositorio.cs`
- Modify: `src/LaMesaDelDuque.Infraestructura/Repositorios/PedidoRepositorio.cs`
- Modify: `src/LaMesaDelDuque.Infraestructura/Persistencia/Configuraciones/PedidoConfiguracion.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/Dtos/PedidoDto.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs`
- Modify: `src/LaMesaDelDuque.Aplicacion/Servicios/ShiftHandoffServicio.cs`

- [ ] **Step 1: Add `Pedido.MeseroAsignadoId` and `AsignarMesero(Guid meseroId)`**

The method must reject `Guid.Empty`, reject non-table orders, and update the nullable owner id.

- [ ] **Step 2: Add active repository queries**

Implement `ObtenerActivosPorMeseroAsync(Guid meseroId, CancellationToken)` and `ObtenerActivoPorMesaParaActualizarAsync(Guid mesaId, CancellationToken)` with active states: `Pendiente`, `EnPreparacion`, `EnCobro`, `Pagado`, `Listo`.

- [ ] **Step 3: Map owner persistence**

Configure nullable `MeseroAsignadoId`, FK to `Usuario`, `DeleteBehavior.SetNull`, and an index.

- [ ] **Step 4: Assign current Mesero on table order creation**

In `PedidosServicio.CrearPedidoAsync`, if `tipoServicio == ComerAqui`, a mesa exists, and the authenticated principal is in role `Mesero`, call `pedido.AsignarMesero(usuarioId)` before adding the order.

- [ ] **Step 5: Replace the handoff stub**

`ObtenerMesasActivasAsync` must validate non-empty user id and return only active orders owned by that user. `TransferirMesaAsync` must validate target Mesero is an active `Mesero`, find the active tracked table order, update owner, add `Auditoria`, save changes, and log the transfer.

- [ ] **Step 6: Run GREEN service tests**

Run the same filtered `dotnet test` command. Expected: PASS.

---

### Task 3: Add minimal Mesero handoff UI and source/page tests

**Files:**
- Create: `src/LaMesaDelDuque.Web/Pages/Operaciones/Mesero/Handoff.cshtml.cs`
- Create: `src/LaMesaDelDuque.Web/Pages/Operaciones/Mesero/Handoff.cshtml`
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Mesero/Index.cshtml`
- Modify: `src/LaMesaDelDuque.Web/Pages/Index.cshtml.cs`
- Create: `tests/LaMesaDelDuque.Pruebas/Web/MeseroHandoffPageTests.cs`

- [ ] **Step 1: Write failing page tests**

Assert the handoff PageModel permits `Administrador`, `Encargado`, and `Mesero`; loads current-user tables plus active Meseros; rejects missing identity; and calls transfer with the selected table, target Mesero, and current actor.

- [ ] **Step 2: Run RED page tests**

Run:

```powershell
dotnet test tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj --filter MeseroHandoffPageTests --no-restore
```

Expected: FAIL/compile errors because the page does not exist.

- [ ] **Step 3: Implement PageModel and Razor page**

Use `IShiftHandoffServicio` and `IUsuariosServicio`; filter users by `RolNombre == "Mesero" && Activo`; avoid exposing raw exception messages for unexpected errors.

- [ ] **Step 4: Add navigation**

Add a small link from `Operaciones/Mesero/Index.cshtml` to `/Operaciones/Mesero/Handoff` and a home module labeled `Transferir mesas` for Admin/Encargado/Mesero.

- [ ] **Step 5: Run GREEN page tests**

Run the same filtered page test command. Expected: PASS.

---

### Task 4: Migration, full verification, and commit

**Files:**
- Create/modify EF migration files under `src/LaMesaDelDuque.Infraestructura/Migrations/` if EF tooling can generate a clean migration.
- Verify all changed source/tests.

- [ ] **Step 1: Generate migration if tooling is available**

Run:

```powershell
dotnet ef migrations add AgregarMeseroAsignadoPedido --project src/LaMesaDelDuque.Infraestructura --startup-project src/LaMesaDelDuque.Web --no-build
```

If EF tooling is unavailable, create a manual migration adding nullable `MeseroAsignadoId`, index, and FK to `Usuarios(Id)` with `SetNull`, then ensure the model snapshot compiles.

- [ ] **Step 2: Build**

Run:

```powershell
dotnet build LaMesaDelDuque.slnx --no-restore
```

Expected: 0 errors.

- [ ] **Step 3: Test**

Run:

```powershell
dotnet test LaMesaDelDuque.slnx --no-build
```

Expected: all tests pass.

- [ ] **Step 4: Review diff and commit work unit**

Run `git diff --stat` and `git status --short`, then commit the full Slice 10 work unit with:

```powershell
git add docs/superpowers/plans/2026-05-29-slice-10-mesero-shift-handoff.md src tests
git commit -m "feat(slice10): add mesero table handoff baseline"
```
