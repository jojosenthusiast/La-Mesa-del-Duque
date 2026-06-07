# Slice 5 Mesero Payment Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Mesero/tableside payment path preserve payment method and reference traceability instead of silently recording non-cash payments as cash.

**Architecture:** Keep the domain payment invariant in `Pago`/`PedidosServicio`, and make the Mesero Razor Page handler translate UI payment codes into `MetodoPago` before calling the service. The JavaScript payment overlay remains vanilla JS, but non-cash methods must collect a visible reference before posting to `PagarJson`.

**Tech Stack:** .NET 8, Razor Pages, xUnit, vanilla JavaScript, existing Node syntax smoke tests.

---

## File Structure

- Create `tests/LaMesaDelDuque.Pruebas/Web/MeseroPageTests.cs`
  - Tests the Mesero payment handler directly.
  - Uses fakes for `IPedidosServicio`, catalog, tables, and SignalR hub.
- Create `tests/LaMesaDelDuque.Pruebas/Web/MeseroJavaScriptPaymentTests.cs`
  - Source-level contract test for the Mesero JS payment reference flow.
- Modify `src/LaMesaDelDuque.Web/Pages/Operaciones/Mesero/Index.cshtml.cs`
  - Map `tarjeta` to `MetodoPago.Tarjeta`.
  - Map `qr` and `transferencia` to `MetodoPago.Transferencia`.
  - Reject card/transfer payments without reference before calling the service.
  - Pass the normalized reference to `PagarPedidoAsync`.
- Modify `src/LaMesaDelDuque.Web/wwwroot/js/mesero.js`
  - Route card/QR buttons to a reference overlay.
  - Post `referencia` to `PagarJson`.

## Tasks

### Task 1: Add RED backend tests for Mesero payment mapping

- [ ] Create `MeseroPageTests` with tests for:
  - card payment passes `MetodoPago.Tarjeta` and trimmed reference to `IPedidosServicio.PagarPedidoAsync`.
  - QR payment passes `MetodoPago.Transferencia` and trimmed reference.
  - card payment without reference returns `BadRequestObjectResult` and does not call the service.
  - cash payment still validates insufficient received amount.
- [ ] Run:
  `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter MeseroPageTests --logger "console;verbosity=normal"`
- [ ] Expected RED: at least card/QR mapping fails because the current handler ignores `metodoPago` and `referencia`.

### Task 2: Add RED source contract test for Mesero JS reference capture

- [ ] Create `MeseroJavaScriptPaymentTests` asserting `mesero.js` contains:
  - `abrirReferenciaPago('tarjeta'`
  - `abrirReferenciaPago('qr'`
  - `lmd-mesero-payment-ref`
  - `referencia: referencia`
- [ ] Run:
  `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter MeseroJavaScriptPaymentTests --logger "console;verbosity=normal"`
- [ ] Expected RED: current `mesero.js` pays card/QR directly without a reference overlay.

### Task 3: Implement Mesero PageModel payment hardening

- [ ] In `OnPostPagarJsonAsync`, normalize method text using a small switch expression.
- [ ] Trim `referencia` once into `referenciaNormalizada`.
- [ ] If method is `Tarjeta` or `Transferencia` and reference is blank, return `BadRequest(ErrorSeguro(new ArgumentException("La referencia del pago es obligatoria para tarjeta o transferencia.")))`.
- [ ] Preserve existing cash insufficient-amount validation.
- [ ] Call `_pedidosServicio.PagarPedidoAsync(pedidoId, metodoEnum, referenciaNormalizada)`.
- [ ] Re-run focused `MeseroPageTests` and expect green.

### Task 4: Implement Mesero JS reference overlay

- [ ] Replace card/QR direct button calls in `abrirPago()` with `mesero.abrirReferenciaPago('tarjeta', total)` and `mesero.abrirReferenciaPago('qr', total)`.
- [ ] Add `abrirReferenciaPago(metodo, total)` to render a small overlay with `#lmd-mesero-payment-ref`.
- [ ] Add `confirmarReferenciaPago(metodo, total)` that trims the reference, rejects blank input with toast, and calls `pagarDirecto(metodo, total, referencia)`.
- [ ] Change `pagarDirecto(metodo, monto)` to `pagarDirecto(metodo, monto, referencia)` and include `referencia` in the posted JSON when present.
- [ ] Re-run `MeseroJavaScriptPaymentTests` and `JavaScriptSyntaxTests`.

### Task 5: Full verification and commit

- [ ] Run `dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal`.
- [ ] Run `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --no-build --logger "console;verbosity=minimal"`.
- [ ] Run `git diff --check`.
- [ ] Inspect `git diff --stat`.
- [ ] Commit with `fix(slice5): harden mesero payment traceability`.
