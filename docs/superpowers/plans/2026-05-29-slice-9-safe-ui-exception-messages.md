# Slice 9 Safe UI Exception Messages Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stop selected high-risk Razor Page handlers from exposing unexpected exception details to users while preserving intentional domain validation messages.

**Architecture:** Keep this as a focused PageModel hardening slice. Do not introduce a global error framework yet; instead, add local generic-error helpers and logger-backed unexpected-exception handling on the selected surfaces found by the post-Slice-8 scan. Preserve `ReglaDominioException` and validation-style `ArgumentException` messages because those are user-actionable business errors.

**Tech Stack:** ASP.NET Core Razor Pages, C#/.NET 8, xUnit source/behavior regression tests, Microsoft.Extensions.Logging.

---

## File Map

- Modify: `src/LaMesaDelDuque.Web/Pages/Admin/Dashboard/Dashboard.cshtml.cs`
  - Log unexpected dashboard page-load failures and show a generic toast.
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Inventario/Index.cshtml.cs`
  - Inject `ILogger<IndexModel>`.
  - Preserve business validation messages for `ReglaDominioException` / `ArgumentException`.
  - Log all other unexpected form-handler exceptions and show a generic toast.
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Salon/Mapa.cshtml.cs`
  - Inject `ILogger<MapaModel>`.
  - Add generic 500 JSON responses for unexpected exceptions in mutating JSON handlers.
  - Keep domain/argument messages for expected business validation.
- Modify: `tests/LaMesaDelDuque.Pruebas/Web/MapaSalonPageTests.cs`
  - Update constructor calls for the explicit logger dependency.
- Create: `tests/LaMesaDelDuque.Pruebas/Web/SafeUiExceptionMessagesTests.cs`
  - Add behavior tests for Dashboard, Inventario, and Mapa unexpected exception handling.
  - Add source guard tests that selected pages do not regress to raw `ToastError = ex.Message` patterns.

## Task 1: Regression tests first

- [ ] **Step 1: Create `tests/LaMesaDelDuque.Pruebas/Web/SafeUiExceptionMessagesTests.cs`**

Add tests with these behaviors:

```csharp
[Fact]
public async Task Dashboard_OnGetAsync_ErrorInesperado_NoExponeDetalleYLoguea()
{
    var logger = new RecordingLogger<DashboardModel>();
    var page = new DashboardModel(new ThrowingMetricaServicio(new InvalidOperationException("password=secret; host=internal")), logger);

    await page.OnGetAsync();

    Assert.NotNull(page.ToastError);
    Assert.DoesNotContain("password=secret", page.ToastError, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dashboard", page.ToastError, StringComparison.OrdinalIgnoreCase);
    Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception is InvalidOperationException);
}
```

Also include:
- Inventory domain errors keep the domain message.
- Inventory no-open-day merma errors keep the operational message without logging as an unexpected error.
- Inventory unexpected errors do not expose detail and are logged.
- Mapa unexpected JSON errors return status 500, do not expose detail, and are logged.
- Source guard forbids `ToastError = ex.Message` and the dashboard interpolated exception toast in the selected files.

- [ ] **Step 2: Run the focused test file and verify RED**

Run:

```powershell
dotnet test tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj --filter FullyQualifiedName~SafeUiExceptionMessagesTests --no-restore
```

Expected: fail before production changes because selected PageModels still expose raw exception messages and/or lack logger-backed generic handling.

## Task 2: Dashboard safe page-load error

- [ ] **Step 1: Modify Dashboard page-load catch**

Change `CargarDatosAsync()` so unexpected exceptions are logged and the toast is generic:

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error inesperado al cargar dashboard administrativo.");
    ToastError = "No se pudo cargar el dashboard. Intenta nuevamente.";
}
```

- [ ] **Step 2: Run the focused Dashboard test**

Run:

```powershell
dotnet test tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj --filter "FullyQualifiedName~SafeUiExceptionMessagesTests&FullyQualifiedName~Dashboard" --no-restore
```

Expected: Dashboard safe-message test passes.

## Task 3: Inventario safe form-handler errors

- [ ] **Step 1: Add logger dependency and helper methods**

Inject `ILogger<IndexModel>` and add helpers:

```csharp
private const string MensajeErrorInesperado = "Ocurrio un error interno. Intenta nuevamente.";

private void RegistrarErrorInesperado(Exception ex, string accion)
{
    _logger.LogError(ex, "Error inesperado al {Accion} en inventario.", accion);
    ToastError = MensajeErrorInesperado;
}

private void RegistrarErrorDeNegocio(Exception ex)
{
    ToastError = ex.Message;
}
```

- [ ] **Step 2: Replace each broad `catch (Exception ex)` in form handlers**

For each handler, use this shape:

```csharp
catch (ReglaDominioException ex) { RegistrarErrorDeNegocio(ex); }
catch (ArgumentException ex) { RegistrarErrorDeNegocio(ex); }
catch (Exception ex) { RegistrarErrorInesperado(ex, "crear ingrediente"); }
```

For `registrar merma`, also preserve the explicit operational no-open-day message with a filtered `InvalidOperationException` catch before the generic catch. Use action labels specific to each operation:
- `crear ingrediente`
- `editar ingrediente`
- `cambiar estado del ingrediente`
- `crear proveedor`
- `editar proveedor`
- `cambiar estado del proveedor`
- `registrar merma`

- [ ] **Step 3: Run focused Inventory tests**

Run:

```powershell
dotnet test tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj --filter "FullyQualifiedName~SafeUiExceptionMessagesTests&FullyQualifiedName~Inventario" --no-restore
```

Expected: Inventory domain-message and unexpected-error tests pass.

## Task 4: Mapa safe JSON unexpected errors

- [ ] **Step 1: Add logger dependency and generic JSON helper**

Inject `ILogger<MapaModel>` and add:

```csharp
private JsonResult ErrorInesperadoJson(Exception ex, string accion)
{
    _logger.LogError(ex, "Error inesperado al {Accion} en mapa de salon.", accion);
    return new JsonResult(new { exito = false, error = "Ocurrio un error interno." }) { StatusCode = 500 };
}
```

- [ ] **Step 2: Add generic catch blocks after existing domain catches**

For `OnPostActualizarPosicionAsync` and `OnPostCambiarEstadoAsync`, keep `ReglaDominioException` / `ArgumentException` catches as-is, then add:

```csharp
catch (Exception ex)
{
    return ErrorInesperadoJson(ex, "actualizar posicion de mesa");
}
```

and:

```csharp
catch (Exception ex)
{
    return ErrorInesperadoJson(ex, "cambiar estado de mesa");
}
```

- [ ] **Step 3: Update existing Mapa tests**

Pass `NullLogger<MapaModel>.Instance` in existing `MapaModel` constructors.

- [ ] **Step 4: Run focused Mapa tests**

Run:

```powershell
dotnet test tests/LaMesaDelDuque.Pruebas/LaMesaDelDuque.Pruebas.csproj --filter "FullyQualifiedName~MapaSalonPageTests|FullyQualifiedName~SafeUiExceptionMessagesTests" --no-restore
```

Expected: existing Mapa behavior stays green and new unexpected-error test passes.

## Task 5: Full verification and commit

- [ ] **Step 1: Run build**

```powershell
dotnet build LaMesaDelDuque.slnx --no-restore
```

Expected: exit code 0.

- [ ] **Step 2: Run full tests**

```powershell
dotnet test LaMesaDelDuque.slnx --no-build
```

Expected: all tests pass.

- [ ] **Step 3: Check whitespace and diff**

```powershell
git diff --check
git diff --stat
```

Expected: `git diff --check` has no output/errors; diff only contains Slice 9 plan, tests, and selected PageModels.

- [ ] **Step 4: Commit reviewable work units**

Keep the behavior commit under the review budget by committing the plan separately from code/tests:

```powershell
git add docs/superpowers/plans/2026-05-29-slice-9-safe-ui-exception-messages.md
git commit -m "docs(slice9): add safe ui exception plan"

git add tests/LaMesaDelDuque.Pruebas/Web/SafeUiExceptionMessagesTests.cs tests/LaMesaDelDuque.Pruebas/Web/MapaSalonPageTests.cs src/LaMesaDelDuque.Web/Pages/Admin/Dashboard/Dashboard.cshtml.cs src/LaMesaDelDuque.Web/Pages/Operaciones/Inventario/Index.cshtml.cs src/LaMesaDelDuque.Web/Pages/Operaciones/Salon/Mapa.cshtml.cs
git commit -m "fix(slice9): harden ui exception messages"
```

Expected: two conventional commits: one plan-only docs commit and one behavior commit containing tests with implementation.
