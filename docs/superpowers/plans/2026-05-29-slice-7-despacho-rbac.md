# Slice 7 Dedicated Despacho Role / RBAC Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a dedicated `Despacho` role so dispatch responsibilities stop piggybacking on `Cajero`/`Mesero` and the system routes/menus/auth policies reflect the real operational boundary.

**Architecture:** Keep the change as an RBAC slice: seed/repair development access in `Program.cs`, route Despacho logins to the dispatch page, restrict the dispatch PageModel to `Administrador`, `Encargado`, and `Despacho`, and update home/layout navigation to surface dispatch only to authorized roles. Do not introduce a permission engine or schema migration; roles are already the project boundary.

**Tech Stack:** ASP.NET Core Razor Pages, cookie auth roles, xUnit source/reflection/PageModel tests, .NET 8.

---

## File Structure

- Create: `tests/LaMesaDelDuque.Pruebas/Web/DespachoRbacTests.cs` — source/reflection/PageModel regression coverage for the new role boundary.
- Modify: `src/LaMesaDelDuque.Web/Program.cs` — seed fresh and existing dev databases with `Despacho` role + demo user and repair credentials.
- Modify: `src/LaMesaDelDuque.Web/Pages/Auth/Login.cshtml.cs` — redirect `Despacho` users to `/Operaciones/Despacho/Index`.
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Despacho/Index.cshtml.cs` — remove `Cajero`/`Mesero` from page authorization and add `Despacho`.
- Modify: `src/LaMesaDelDuque.Web/Pages/Index.cshtml.cs` — make the home module visible to `Administrador`, `Encargado`, and `Despacho` only.
- Modify: `src/LaMesaDelDuque.Web/Pages/Shared/_Layout.cshtml` — label dedicated Despacho operator sessions correctly in the ops strip.
- Modify: `src/LaMesaDelDuque.Web/Pages/Shared/_Sidebar.cshtml` — expose a Despacho link to management users using the sidebar.

---

### Task 1: RED tests for the Despacho RBAC contract

**Files:**
- Create: `tests/LaMesaDelDuque.Pruebas/Web/DespachoRbacTests.cs`

- [ ] **Step 1: Write failing tests**

Create tests that verify:

```csharp
[Fact]
public void DespachoPage_DebePermitirSoloGestionYDespacho()
{
    var attribute = typeof(LaMesaDelDuque.Web.Pages.Operaciones.Despacho.IndexModel)
        .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
        .OfType<AuthorizeAttribute>()
        .Single();

    var roles = attribute.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

    Assert.Contains("Administrador", roles);
    Assert.Contains("Encargado", roles);
    Assert.Contains("Despacho", roles);
    Assert.DoesNotContain("Cajero", roles);
    Assert.DoesNotContain("Mesero", roles);
}
```

Also include PageModel behavior for home modules, and source checks for `Program.cs`, `Login.cshtml.cs`, `_Layout.cshtml`, and `_Sidebar.cshtml`.

- [ ] **Step 2: Run focused test to verify RED**

Run:

```powershell
dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter DespachoRbacTests --logger "console;verbosity=minimal"
```

Expected: FAIL because current code has no dedicated seeded `Despacho` role/user, Despacho page allows `Cajero`/`Mesero`, home exposes Despacho to `Cajero`/`Mesero`, and login has no Despacho route.

---

### Task 2: Implement minimal RBAC hardening

**Files:**
- Modify: `src/LaMesaDelDuque.Web/Program.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/Auth/Login.cshtml.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/Operaciones/Despacho/Index.cshtml.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/Index.cshtml.cs`
- Modify: `src/LaMesaDelDuque.Web/Pages/Shared/_Layout.cshtml`
- Modify: `src/LaMesaDelDuque.Web/Pages/Shared/_Sidebar.cshtml`

- [ ] **Step 1: Seed dedicated role and demo user**

In `Program.cs`, add `Despacho` to fresh seed and add an idempotent dev repair for existing databases:

```csharp
var despachoRol = new Rol("Despacho", "Entrega pedidos listos y libera mesas");
```

Add user credentials:

```csharp
var despachoHash = BCrypt.Net.BCrypt.HashPassword("Despacho901!", 12);
new Usuario("ana", "ana@mesadelduque.com", despachoHash, "Ana Despacho", despachoRol)
```

After the main seed block, ensure an existing dev DB gets missing `Despacho` role/user.

- [ ] **Step 2: Route Despacho users after login**

Add to `Login.cshtml.cs`:

```csharp
"Despacho" => "/Operaciones/Despacho/Index",
```

- [ ] **Step 3: Restrict dispatch page authorization**

Change `Pages/Operaciones/Despacho/Index.cshtml.cs` to:

```csharp
[Authorize(Roles = "Administrador,Encargado,Despacho")]
```

- [ ] **Step 4: Update navigation visibility**

In `Index.cshtml.cs`, add `esDespacho` and change the Despacho module rule to `esAdmin || esEncargado || esDespacho`.

In `_Layout.cshtml`, add an `esDespacho` role boolean and set the ops strip label to `Despacho` for that role.

In `_Sidebar.cshtml`, add a sidebar link to `/Operaciones/Despacho/Index` for management users so Admin/Encargado can still reach dispatch from the shell.

- [ ] **Step 5: Run focused test to verify GREEN**

Run:

```powershell
dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter DespachoRbacTests --logger "console;verbosity=minimal"
```

Expected: PASS.

---

### Task 3: Verification and commit

**Files:**
- All changed files from Tasks 1-2.

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
git add docs/superpowers/plans/2026-05-29-slice-7-despacho-rbac.md tests/LaMesaDelDuque.Pruebas/Web/DespachoRbacTests.cs src/LaMesaDelDuque.Web/Program.cs src/LaMesaDelDuque.Web/Pages/Auth/Login.cshtml.cs src/LaMesaDelDuque.Web/Pages/Operaciones/Despacho/Index.cshtml.cs src/LaMesaDelDuque.Web/Pages/Index.cshtml.cs src/LaMesaDelDuque.Web/Pages/Shared/_Layout.cshtml src/LaMesaDelDuque.Web/Pages/Shared/_Sidebar.cshtml
git commit -m "fix(slice7): add dedicated despacho role"
```

Expected: one conventional work-unit commit containing tests, code, and plan.
