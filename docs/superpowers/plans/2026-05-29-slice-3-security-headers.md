# Slice 3 Security Headers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish a verified baseline of browser security headers for every web response without breaking the current CDN-heavy Razor Pages UI.

**Architecture:** Add a focused middleware in the Web project that writes defensive headers before the rest of the request pipeline runs. Keep the first CSP intentionally compatible with current external fonts/scripts/icons, while locking down dangerous defaults. Unit-test the middleware directly with `DefaultHttpContext` so this slice is fast and deterministic.

**Tech Stack:** .NET 8, ASP.NET Core middleware, xUnit.

---

## File Structure

- Create `src/LaMesaDelDuque.Web/Seguridad/SecurityHeadersMiddleware.cs`
  - One middleware class responsible only for baseline security headers.
  - One extension method `UseLaMesaSecurityHeaders()` so `Program.cs` stays readable.
- Modify `src/LaMesaDelDuque.Web/Program.cs`
  - Register the middleware immediately after exception/HSTS setup and before HTTPS/static/routing.
- Create `tests/LaMesaDelDuque.Pruebas/Web/SecurityHeadersMiddlewareTests.cs`
  - Directly execute the middleware with `DefaultHttpContext`.
  - Assert headers are present and do not overwrite a more specific downstream/header-preexisting value.

## Baseline Headers

- `X-Content-Type-Options: nosniff`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy: camera=(), microphone=(), geolocation=(), payment=()`
- `Content-Security-Policy`
  - `default-src 'self'`
  - allows current inline styles/scripts and CDN assets only where the current UI still depends on them
  - keeps `connect-src 'self'` for SignalR/fetch
  - blocks framing with `frame-ancestors 'self'`
  - blocks plugins with `object-src 'none'`

## Tasks

### Task 1: Add RED middleware tests
- [ ] Create `SecurityHeadersMiddlewareTests`.
- [ ] Test a normal response gets all four headers.
- [ ] Test existing headers are not overwritten.
- [ ] Run:
  `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --filter SecurityHeadersMiddlewareTests --logger "console;verbosity=normal"`
- [ ] Expected RED: compile/test failure because middleware does not exist yet.

### Task 2: Implement the middleware
- [ ] Create `SecurityHeadersMiddleware`.
- [ ] Add headers before the downstream middleware runs so static files and Razor responses both inherit the baseline.
- [ ] Use `TryAdd` semantics: do not overwrite an explicit header already present.
- [ ] Add `UseLaMesaSecurityHeaders()` extension.
- [ ] Re-run focused tests and verify GREEN.

### Task 3: Wire into the real app
- [ ] Add `using LaMesaDelDuque.Web.Seguridad;` to `Program.cs`.
- [ ] Call `app.UseLaMesaSecurityHeaders();` after exception/HSTS and before `UseHttpsRedirection()`.
- [ ] Add a source-level regression in the test file if necessary to prove `Program.cs` wires the middleware.

### Task 4: Full verification and commit
- [ ] Run `dotnet build LaMesaDelDuque.slnx --no-restore --verbosity minimal`.
- [ ] Run `dotnet test tests\LaMesaDelDuque.Pruebas\LaMesaDelDuque.Pruebas.csproj --no-build --logger "console;verbosity=minimal"`.
- [ ] Run `git diff --check`.
- [ ] Inspect `git diff --stat`.
- [ ] Commit with `fix(slice3): add security headers baseline`.
