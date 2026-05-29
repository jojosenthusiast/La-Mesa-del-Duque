# Slice 8 Critical Vendor Assets / Offline Reliability Baseline Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the critical POS/KDS/Dashboard runtime independent from public CDNs by shipping required vendor assets locally and tightening the security header baseline accordingly.

**Architecture:** Treat this as an asset reliability and CSP slice, not a visual redesign. Keep existing UI behavior and vendor APIs intact, copy only the files currently needed at runtime, update Razor/JS references from external hosts to `/lib/...`, and add source-level regression tests that catch future CDN regressions. Google Fonts are removed rather than vendored in this slice because font fallback is acceptable for restaurant operations; SignalR, Chart.js, Bootstrap Icons, and Lucide SVG icons are operationally relevant and must be local.

**Tech Stack:** ASP.NET Core Razor Pages, static files under `wwwroot/lib`, xUnit source contract tests, PowerShell asset vendoring, .NET 8.

---

## File Structure

- Create: `tests/LaMesaDelDuque.Pruebas/Web/CriticalVendorAssetsTests.cs` — regression tests for no external critical asset hosts, local asset existence, and CSP host tightening.
- Create: `src/LaMesaDelDuque.Web/wwwroot/lib/microsoft/signalr/dist/browser/signalr.min.js` — local SignalR browser client.
- Create: `src/LaMesaDelDuque.Web/wwwroot/lib/chart.js/chart.umd.min.js` — local Chart.js UMD bundle.
- Create: `src/LaMesaDelDuque.Web/wwwroot/lib/bootstrap-icons/font/bootstrap-icons.css` and font files — local Bootstrap Icons CSS/font.
- Create: `src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/*.svg` — local Lucide icons currently referenced by POS/KDS/Mesero/Tableside.
- Modify: `src/LaMesaDelDuque.Web/Pages/Shared/_Layout.cshtml` — remove Google Fonts/preconnect and point Bootstrap Icons to local CSS.
- Modify: `src/LaMesaDelDuque.Web/Pages/Admin/Dashboard/Dashboard.cshtml` — use local Chart.js and SignalR.
- Modify: `src/LaMesaDelDuque.Web/Pages/Cocina/KDS.cshtml` — remove Google Fonts and use local SignalR.
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Mesero/Index.cshtml` — use local SignalR.
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Pedidos/Index.cshtml` — use local SignalR.
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Pedidos/Tableside.cshtml` — remove Google Fonts, use local SignalR, and local Lucide icon path.
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Salon/Mapa.cshtml` — use local SignalR.
- Modify: `src/LaMesaDelDuque.Web/wwwroot/js/cocina-kds.js`, `mesero.js`, `pos.js`, `tableside.js` — point Lucide icon references to local SVG files.
- Modify: `src/LaMesaDelDuque.Web/Seguridad/SecurityHeadersMiddleware.cs` — remove CDN/font hosts from CSP now that runtime assets are local.

---

### Task 1: RED tests for vendor asset reliability

**Files:**
- Create: `tests/LaMesaDelDuque.Pruebas/Web/CriticalVendorAssetsTests.cs`

- [ ] **Step 1: Write the failing test**

Create a test class that:

```csharp
private static readonly string[] ForbiddenRuntimeHosts =
[
    "cdn.jsdelivr.net",
    "cdnjs.cloudflare.com",
    "fonts.googleapis.com",
    "fonts.gstatic.com",
    "unpkg.com"
];
```

Checks all runtime source files under `Pages/**/*.cshtml`, `wwwroot/js/*.js`, and `Seguridad/**/*.cs` for those hosts, while intentionally excluding `wwwroot/lib` because vendored third-party license comments may include URLs.

Also assert these files exist:

```text
src/LaMesaDelDuque.Web/wwwroot/lib/microsoft/signalr/dist/browser/signalr.min.js
src/LaMesaDelDuque.Web/wwwroot/lib/chart.js/chart.umd.min.js
src/LaMesaDelDuque.Web/wwwroot/lib/bootstrap-icons/font/bootstrap-icons.css
src/LaMesaDelDuque.Web/wwwroot/lib/bootstrap-icons/font/fonts/bootstrap-icons.woff2
src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/check.svg
src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/arrow-left.svg
src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/package.svg
src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/alert-triangle.svg
```

- [ ] **Step 2: Run focused test to verify RED**

Run:

```powershell
dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter CriticalVendorAssetsTests --logger "console;verbosity=minimal"
```

Expected: FAIL because current Razor/JS/CSP files still reference CDN hosts and local SignalR/Chart/Bootstrap Icons/Lucide files are missing.

---

### Task 2: Vendor the critical assets locally

**Files:**
- Create files under `src/LaMesaDelDuque.Web/wwwroot/lib/...`

- [ ] **Step 1: Download/copy pinned assets into `wwwroot/lib`**

Use exact current runtime families:

```text
@microsoft/signalr browser client 8.x
chart.js 4.4.1 UMD
bootstrap-icons 1.11.3 font CSS + woff2/woff
lucide-static SVG icons currently referenced by the app
```

Copy only runtime files and license files where available. Do not commit `node_modules`.

- [ ] **Step 2: Verify local asset paths exist**

Run:

```powershell
Test-Path src\LaMesaDelDuque.Web\wwwroot\lib\microsoft\signalr\dist\browser\signalr.min.js
Test-Path src\LaMesaDelDuque.Web\wwwroot\lib\chart.js\chart.umd.min.js
Test-Path src\LaMesaDelDuque.Web\wwwroot\lib\bootstrap-icons\font\bootstrap-icons.css
Test-Path src\LaMesaDelDuque.Web\wwwroot\lib\lucide-static\icons\check.svg
```

Expected: all `True`.

---

### Task 3: Replace external references with local references

**Files:**
- Modify the Razor/JS/CSP files listed in File Structure.

- [ ] **Step 1: Replace SignalR script tags**

Use:

```html
<script src="~/lib/microsoft/signalr/dist/browser/signalr.min.js"></script>
```

for Dashboard, KDS, Mesero, POS, Tableside, Salon Mapa, and keep Despacho aligned with the same path.

- [ ] **Step 2: Replace Chart.js script tag**

Use:

```html
<script src="~/lib/chart.js/chart.umd.min.js"></script>
```

- [ ] **Step 3: Replace Bootstrap Icons CSS**

Use:

```html
<link rel="stylesheet" href="~/lib/bootstrap-icons/font/bootstrap-icons.css" />
```

- [ ] **Step 4: Remove Google Fonts runtime dependency**

Delete Google Fonts `preconnect` and `href` tags from `_Layout.cshtml`, `KDS.cshtml`, and `Tableside.cshtml`. Existing CSS font stacks may keep `Cinzel`/`Montserrat` as preferred font names; browser fallback is acceptable.

- [ ] **Step 5: Replace Lucide icon references**

Change:

```text
https://cdn.jsdelivr.net/npm/lucide-static@latest/icons/
```

to:

```text
/lib/lucide-static/icons/
```

in Razor and JS files.

- [ ] **Step 6: Tighten CSP**

Change `SecurityHeadersMiddleware` so `script-src`, `style-src`, and `font-src` no longer list `cdnjs.cloudflare.com`, `cdn.jsdelivr.net`, `fonts.googleapis.com`, or `fonts.gstatic.com`.

- [ ] **Step 7: Run focused test to verify GREEN**

Run:

```powershell
dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter CriticalVendorAssetsTests --logger "console;verbosity=minimal"
```

Expected: PASS.

---

### Task 4: Verification and commit

**Files:**
- All changed files from Tasks 1-3.

- [ ] **Step 1: Build**

Run:

```powershell
dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal
```

Expected: 0 errors.

- [ ] **Step 2: Full test suite**

Run:

```powershell
dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --no-build --logger "console;verbosity=minimal"
```

Expected: all tests pass.

- [ ] **Step 3: Whitespace check**

Run:

```powershell
git diff --check
```

Expected: no output and exit code 0.

- [ ] **Step 4: Review and commit work unit**

Run:

```powershell
git diff --stat
git status --short
git add docs/superpowers/plans/2026-05-29-slice-8-critical-vendor-assets.md tests/LaMesaDelDuque.Pruebas/Web/CriticalVendorAssetsTests.cs src/LaMesaDelDuque.Web
git commit -m "fix(slice8): localize critical vendor assets"
```

Expected: one conventional work-unit commit containing tests, local assets, CSP, and runtime reference updates.
