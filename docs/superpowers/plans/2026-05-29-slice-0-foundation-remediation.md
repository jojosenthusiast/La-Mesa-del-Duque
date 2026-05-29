# Slice 0 Foundation Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore the build and remove the most dangerous audit blockers before broader workflow work.

**Architecture:** This slice is intentionally narrow: fix dependency/test-contract drift, add static regression checks, repair the KDS JavaScript parser failure, and remove secret-bearing debug logging. It does not redesign the payment/mesa lifecycle, build missing modules, or overhaul UX yet.

**Tech Stack:** .NET 8, xUnit, Razor Pages, JavaScript checked by local Node, EF Core, PowerShell verification commands.

---

## Task 1: Restore package and test-contract build baseline

**Files:**
- Modify: `src/LaMesaDelDuque.Aplicacion/LaMesaDelDuque.Aplicacion.csproj`
- Modify: `tests/LaMesaDelDuque.Pruebas/Web/PedidosPageTests.cs`

- [ ] Add the missing `ClosedXML` and `QuestPDF` package references to the application project because `ReportesServicio.cs` already references those namespaces.
- [ ] Update `FakePedidosServicio` to match `IPedidosServicio` signatures:
  - `PagarPedidoAsync(Guid pedidoId, MetodoPago metodoPago = MetodoPago.Efectivo, string? referenciaPos = null, CancellationToken cancelacion = default)`
  - `PagarCuentaAsync(Guid cuentaId, MetodoPago metodoPago, decimal propinaMonto = 0, string? referenciaPos = null, CancellationToken cancelacion = default)`
- [ ] Run `dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal` and keep moving only once the prior dependency/signature errors are gone.

## Task 2: Add static regression checks for Slice 0 risks

**Files:**
- Create: `tests/LaMesaDelDuque.Pruebas/Calidad/JavaScriptSyntaxTests.cs`
- Create: `tests/LaMesaDelDuque.Pruebas/Calidad/SecurityConfigurationTests.cs`

- [ ] Add a test that runs `node --check` against every `src/LaMesaDelDuque.Web/wwwroot/js/*.js` file when Node is available; if Node is not installed, skip by returning early so local .NET-only environments are not blocked.
- [ ] Add a test that scans `src/LaMesaDelDuque.Web/appsettings*.json*` and asserts no real connection string password pattern exists.
- [ ] Add a test that scans `src/LaMesaDelDuque.Infraestructura/InyeccionInfraestructura.cs` and fails if it contains `Console.WriteLine` or logs the raw `connectionString` variable.
- [ ] Run the new tests and confirm they fail before production fixes where possible. Expected initial failures: KDS JS syntax and connection logging.

## Task 3: Repair KDS JavaScript syntax

**Files:**
- Modify: `src/LaMesaDelDuque.Web/wwwroot/js/cocina-kds.js`

- [ ] Remove the stray fragment after `agregarSeparadorMesa(...)` that produces `Unexpected token '}'`.
- [ ] Run `node --check src/LaMesaDelDuque.Web/wwwroot/js/cocina-kds.js` and confirm exit code `0`.
- [ ] Run `dotnet test tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj --no-build --filter JavaScriptSyntaxTests` after build succeeds.

## Task 4: Remove secret-bearing debug logging

**Files:**
- Modify: `src/LaMesaDelDuque.Infraestructura/InyeccionInfraestructura.cs`

- [ ] Replace `Console.WriteLine` debug output with no raw connection-string logging.
- [ ] Keep provider selection behavior unchanged: Development with blank connection uses SQLite; nonblank connection uses Npgsql; fallback uses SQLite.
- [ ] Run `dotnet test tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj --filter SecurityConfigurationTests` and confirm the static checks pass.

## Task 5: Final Slice 0 verification

**Files:**
- No additional source files.

- [ ] Run `dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal`.
- [ ] Run `dotnet test LaMesaDelDuque.slnx --no-build --verbosity minimal`.
- [ ] Run `node --check src/LaMesaDelDuque.Web/wwwroot/js/cocina-kds.js`.
- [ ] Run `git status --short` and summarize all changed files.
