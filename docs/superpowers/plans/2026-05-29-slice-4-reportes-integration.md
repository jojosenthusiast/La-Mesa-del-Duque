# Slice 4 Reportes Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn the existing Reportes service into a tracked, role-protected Razor Pages workflow so Admin/Encargado/Gerente links no longer point to a missing/untracked page.

**Architecture:** Keep report generation in `IReportesServicio`; the page model only validates the date range and returns generated file downloads. Build a lean Razor page without inline JavaScript so it fits the new security-header baseline and avoids adding more CSP debt.

**Tech Stack:** .NET 8, Razor Pages, xUnit, existing ClosedXML/QuestPDF report service.

---

## File Structure

- Create `src/LaMesaDelDuque.Web/Pages/Admin/Reportes/Index.cshtml.cs`
  - `[Authorize(Roles = "Administrador,Encargado,Gerente")]`
  - GET displays the report launcher with default current-month date range.
  - handlers:
    - `OnGetVentasPdfAsync`
    - `OnGetVentasExcelAsync`
    - `OnGetKardexExcelAsync`
    - `OnGetMermasPdfAsync`
  - invalid ranges return the page with a clear error instead of generating files.
- Create `src/LaMesaDelDuque.Web/Pages/Admin/Reportes/Index.cshtml`
  - four report cards/forms.
  - each form has its own `desde`/`hasta` date fields and submit button.
  - no inline script.
- Create `tests/LaMesaDelDuque.Pruebas/Web/ReportesPageTests.cs`
  - page model authorization roles.
  - default date range.
  - each export handler calls the correct service method and returns expected content type/file name.
  - invalid range does not call service and returns `PageResult`.

## Tasks

### Task 1: Add RED tests for the tracked report workflow
- [ ] Create `ReportesPageTests`.
- [ ] Reference `LaMesaDelDuque.Web.Pages.Admin.Reportes.IndexModel`, which should fail to compile before the page exists in the remediation worktree.
- [ ] Run:
  `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter ReportesPageTests --logger "console;verbosity=normal"`
- [ ] Expected RED: compile failure because the Reportes page model is missing.

### Task 2: Implement Reportes page model
- [ ] Create `Index.cshtml.cs` under `Pages/Admin/Reportes`.
- [ ] Add role authorization for `Administrador,Encargado,Gerente`.
- [ ] Add date range normalization:
  - `desde.Date`
  - `hasta.Date.AddDays(1).AddTicks(-1)`
- [ ] Add invalid-range guard:
  - set `ToastError`
  - return `Page()`
  - do not call `IReportesServicio`.
- [ ] Re-run focused tests.

### Task 3: Implement Reportes Razor page
- [ ] Create `Index.cshtml`.
- [ ] Add four forms/cards for Ventas PDF, Ventas Excel, Kardex Excel, and Mermas PDF.
- [ ] Keep controls labeled and semantic.
- [ ] Avoid inline JavaScript.
- [ ] Run build to ensure Razor compilation succeeds.

### Task 4: Full verification and commit
- [ ] Run `dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal`.
- [ ] Run `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --no-build --logger "console;verbosity=minimal"`.
- [ ] Run `git diff --check`.
- [ ] Inspect `git diff --stat`.
- [ ] Commit with `fix(slice4): integrate reportes workflow`.
