# Slice 6 POS/Mesero Client-Side Escaping Baseline Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prevent POS and Mesero browser views from injecting backend/catalog/order-controlled text as raw HTML.

**Architecture:** Keep the slice narrow: add small local encoding helpers to `pos.js` and `mesero.js`, then apply them only at string-to-HTML boundaries. Use separate HTML text escaping and inline JavaScript string escaping so display text cannot become markup and onclick arguments remain syntactically safe.

**Tech Stack:** ASP.NET Core Razor Pages, vanilla JavaScript, xUnit, Node runtime smoke checks.

---

### Task 1: Runtime regression test for POS/Mesero escaping

**Files:**
- Create: `tests/LaMesaDelDuque.Pruebas/Calidad/PosMeseroJavaScriptRuntimeTests.cs`
- Create: `tests/LaMesaDelDuque.Pruebas/Calidad/pos-mesero-escaping-smoke.js`

- [ ] **Step 1: Write the failing test**

Create an xUnit test that invokes a Node smoke script against `pos.js` and `mesero.js`. The Node script must load each browser script in a fake DOM, inject dangerous product/category/order data, render POS/Mesero flows, and fail if rendered HTML contains raw `<img`, `<script`, `<svg onload`, or unencoded dangerous strings.

- [ ] **Step 2: Run focused test and verify RED**

Run:

```powershell
dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter PosMeseroJavaScriptRuntimeTests --logger "console;verbosity=minimal"
```

Expected: FAIL because current `pos.js`/`mesero.js` interpolate product/category/order fields directly into `innerHTML`.

### Task 2: Escape POS and Mesero render boundaries

**Files:**
- Modify: `src/LaMesaDelDuque.Web/wwwroot/js/pos.js`
- Modify: `src/LaMesaDelDuque.Web/wwwroot/js/mesero.js`

- [ ] **Step 1: Add local helpers**

Add `escapeHtml(value)` and `escapeJsString(value)` near the existing helpers in both scripts.

- [ ] **Step 2: Apply escaping at HTML text and attribute boundaries**

Use `escapeHtml` for visible/backend-controlled text rendered with `innerHTML`: product names, category names, promos, cart names, split person/item names, allergens, ingredients, table zones, and order detail names. Use `escapeJsString` for string values embedded in inline JavaScript handlers.

- [ ] **Step 3: Run focused test and verify GREEN**

Run the focused runtime test again; expected PASS.

### Task 3: Full verification and commit

**Files:**
- Verify: `LaMesaDelDuque.slnx`
- Verify: `tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj`

- [ ] **Step 1: Run JavaScript syntax tests**

```powershell
dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --no-build --filter JavaScriptSyntaxTests --logger "console;verbosity=minimal"
```

Expected: PASS.

- [ ] **Step 2: Run full build**

```powershell
dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal
```

Expected: 0 warnings and 0 errors.

- [ ] **Step 3: Run full test suite**

```powershell
dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --no-build --logger "console;verbosity=minimal"
```

Expected: all tests PASS.

- [ ] **Step 4: Check whitespace**

```powershell
git diff --check
```

Expected: no output.

- [ ] **Step 5: Commit work unit**

```powershell
git add docs/superpowers/plans/2026-05-29-slice-6-pos-mesero-escaping.md tests/LaMesaDelDuque.Pruebas/Calidad/PosMeseroJavaScriptRuntimeTests.cs tests/LaMesaDelDuque.Pruebas/Calidad/pos-mesero-escaping-smoke.js src/LaMesaDelDuque.Web/wwwroot/js/pos.js src/LaMesaDelDuque.Web/wwwroot/js/mesero.js
git commit -m "fix(slice6): escape pos and mesero render data"
```

