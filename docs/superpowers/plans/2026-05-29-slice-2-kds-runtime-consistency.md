# Slice 2 KDS Runtime Consistency Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prove and harden the KDS browser contract so kitchen orders returned by the backend are rendered safely and counted by the UI.

**Architecture:** Keep the backend JSON contract in `KDSModel` and add a Node-backed runtime smoke test for `cocina-kds.js` without adding npm dependencies. Fix only the KDS JavaScript rendering boundary: normalize/escape server order data before rendering, keep the existing three-column layout, and preserve SignalR/polling behavior.

**Tech Stack:** .NET 8, xUnit, Razor Pages, vanilla JavaScript, local Node runtime when available.

---

## File Structure

- Create `tests/LaMesaDelDuque.Pruebas/Calidad/KdsJavaScriptRuntimeTests.cs`
  - Runs a committed Node script when Node is installed; returns early like `JavaScriptSyntaxTests` when Node is unavailable.
- Create `tests/LaMesaDelDuque.Pruebas/Calidad/kds-runtime-smoke.js`
  - Provides a tiny fake DOM, mocked `fetch`, and deterministic KDS order payload.
  - Executes `src/LaMesaDelDuque.Web/wwwroot/js/cocina-kds.js`.
  - Dispatches `DOMContentLoaded`.
  - Asserts one backend order becomes one `.lmd-kds-card`, updates `#lmd-kds-contador` to `1 ordenes`, and escapes unsafe order text.
- Modify `src/LaMesaDelDuque.Web/wwwroot/js/cocina-kds.js`
  - Add a small HTML escaping helper.
  - Escape all order-controlled text before inserting via `innerHTML`.
  - Put `horaRecibido` on the rendered card dataset so escalation scans real cards, not a selector that never matches.
- Optional if required by focused tests: modify `tests/LaMesaDelDuque.Pruebas/Web/KDSPageTests.cs`
  - Add/adjust backend JSON contract tests only if the runtime test exposes a server-shape mismatch.

## Tasks

### Task 1: Add RED runtime smoke coverage for KDS initial load
- [ ] Add `KdsJavaScriptRuntimeTests` with command:
  `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter KdsJavaScriptRuntimeTests --logger "console;verbosity=normal"`
- [ ] Add `kds-runtime-smoke.js` that fails against current `cocina-kds.js` because unsafe backend text is rendered as HTML instead of escaped text.
- [ ] Expected RED: Node exits non-zero with an assertion explaining that unsafe KDS payload text was not escaped.

### Task 2: Fix KDS rendering boundary
- [ ] Add `escapeHtml(value)` in `cocina-kds.js`.
- [ ] Use escaped values for `productoNombre`, `notas`, `alergenos`, `ingredientesQuitados`, `ingredientesExtra`, `mesaNumero`, and any other order-controlled text interpolated into `innerHTML`.
- [ ] Add `card.dataset.horaRecibido = orden.horaRecibido || ''` so `actualizarEscalacion()` can find cards with received-time data.
- [ ] Re-run the focused KDS runtime smoke test and verify GREEN.

### Task 3: Keep the existing KDS contract intact
- [ ] Run existing KDS page-model tests:
  `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter KDSPageTests --logger "console;verbosity=normal"`
- [ ] Run JavaScript syntax checks:
  `node --check src\LaMesaDelDuque.Web\wwwroot\js\cocina-kds.js`
- [ ] If a server-shape mismatch appears, add a focused `KDSPageTests` assertion before changing production code.

### Task 4: Full verification and commit
- [ ] Run `dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal`.
- [ ] Run `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --no-build --logger "console;verbosity=minimal"`.
- [ ] Run `git diff --check`.
- [ ] Inspect `git diff --stat` and keep the slice reviewable.
- [ ] Commit with `fix(slice2): harden kds runtime rendering`.
