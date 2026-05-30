# Slice 1 Lifecycle + TurnoCaja Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Harden the restaurant operational lifecycle so payment, dispatch, table state, and cashier shift history agree with the audit evidence.

**Architecture:** Keep the fix inside existing domain/application/page boundaries. Payment records money; dispatch releases tables. TurnoCaja history remains a page concern for authorization, while persistence correctness belongs in repository/service tests.

**Tech Stack:** .NET 8, Razor Pages, EF Core SQLite tests, xUnit.

---

## File Structure

- Modify `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs` for service-level table state after payment.
- Modify `tests/LaMesaDelDuque.Pruebas/Aplicacion/DespachoMesaTests.cs` for dispatch release and grace behavior.
- Create `tests/LaMesaDelDuque.Pruebas/Aplicacion/TurnoCajaServicioTests.cs` for shift close/movement/report persistence.
- Create `tests/LaMesaDelDuque.Pruebas/Web/TurnoCajaPageTests.cs` for Cajero authorization on Historial.
- Modify `src/LaMesaDelDuque.Aplicacion/Servicios/PedidosServicio.cs` to stop freeing tables during payment.
- Modify `src/LaMesaDelDuque.Aplicacion/Servicios/DespachoServicio.cs` to release tables only when no other active order remains and start grace on release.
- Modify `src/LaMesaDelDuque.Infraestructura/Repositorios/TurnoCajaRepositorio.cs` so mutable/reported shift loads are tracked and include movements.
- Modify `src/LaMesaDelDuque.Web/Pages/Operaciones/TurnoCaja/Historial.cshtml.cs` to allow Cajero.

## Tasks

### Task 1: Payment does not release dine-in table
- [ ] Change the existing payment table-state test to expect `Ocupada` after `PagarPedidoAsync`.
- [ ] Run the focused test and verify RED.
- [ ] Remove table release/grace from `PagarPedidoAsync`.
- [ ] Run the focused test and verify GREEN.

### Task 2: Dispatch owns table release and grace
- [ ] Add a dispatch test for release + `GraciaHasta` when no other active order remains.
- [ ] Add a dispatch test proving a second active order keeps the table occupied.
- [ ] Run focused tests and verify RED.
- [ ] Update `DespachoServicio` to check other active orders and start grace after release.
- [ ] Run focused tests and verify GREEN.

### Task 3: Cajero can use TurnoCaja history/Reporte Z path
- [ ] Add reflection test for `HistorialModel` roles including `Cajero`.
- [ ] Run focused test and verify RED.
- [ ] Add `Cajero` to `HistorialModel` authorize roles.
- [ ] Run focused test and verify GREEN.

### Task 4: TurnoCaja mutations and Reporte Z persist data
- [ ] Add tests for closing a turno and reloading the closed state.
- [ ] Add tests for registering a movement and seeing it in Reporte Z.
- [ ] Run focused tests and verify RED.
- [ ] Update repository loading/tracking/includes so mutations persist and Reporte Z contains movements.
- [ ] Run focused tests and verify GREEN.

### Task 5: Full verification and commit
- [ ] Run `dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal`.
- [ ] Run `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --no-build --logger "console;verbosity=minimal"`.
- [ ] Run `git diff --check` and inspect status/stat.
- [ ] Commit with `fix(slice1): harden lifecycle and cashier shift flow`.
