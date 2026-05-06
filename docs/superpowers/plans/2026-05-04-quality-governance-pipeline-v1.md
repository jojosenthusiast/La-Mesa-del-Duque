# Quality Governance Pipeline v1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convertir el sistema de calidad existente de “documentación y checklist manual” a una tubería progresiva de calidad como código, con trazabilidad, cobertura, regresión, evidencia y métricas verificables.

**Architecture:** La solución se implementará con scripts PowerShell pequeños y enfocados bajo `scripts/quality/`, ejecutables localmente y desde GitHub Actions. La primera versión trabajará en modo progresivo: reporta advertencias y publica evidencia; después de estabilizar los datos, los mismos scripts podrán activarse como compuertas bloqueantes.

**Tech Stack:** ASP.NET Core/.NET 8, xUnit, Coverlet/XPlat Code Coverage, GitHub Actions, PowerShell 7 en CI, Markdown como fuente de trazabilidad y evidencia, artifacts de GitHub Actions.

---

## Principios de alcance

- Esta fase NO busca certificación completa ISO/IEC 25002 ni ISO 27001.
- ISO/IEC 25002 se usa como guía para medir calidad del producto.
- ISO 27001 se usa como guía ligera para riesgos, evidencia y controles mínimos.
- El primer PR debe producir visibilidad y evidencia confiable antes de bloquear merges.
- PR #9 debe permanecer separado; este plan debe ejecutarse en una rama nueva.

## Estructura de archivos propuesta

### Crear

- `scripts/quality/validar-trazabilidad.ps1` — valida que la matriz de trazabilidad use IDs HU/CA verificables y que las rutas referenciadas existan.
- `scripts/quality/validar-pr-evidencia.ps1` — valida estructura mínima del cuerpo de PR: HU, evidencia de pruebas, impacto/riesgo y checklist de calidad.
- `scripts/quality/generar-paquete-evidencia.ps1` — copia reportes de pruebas, cobertura, métricas y metadatos del commit/PR a una carpeta de evidencia.
- `scripts/quality/generar-metricas-calidad.ps1` — genera `artifacts/quality/metricas-calidad.json` y `artifacts/quality/metricas-calidad.md`.
- `tests/quality/fixtures/trazabilidad-valida.md` — fixture mínima válida para pruebas de trazabilidad.
- `tests/quality/fixtures/trazabilidad-ruta-invalida.md` — fixture con ruta inexistente para probar error.
- `tests/quality/fixtures/pr-body-valido.md` — fixture de cuerpo PR válido.
- `tests/quality/fixtures/pr-body-invalido.md` — fixture de cuerpo PR incompleto.
- `tests/quality/quality-scripts.Tests.ps1` — pruebas de los scripts de calidad.

### Modificar

- `.github/workflows/ci.yml` — agregar jobs de calidad progresiva: validación de trazabilidad, regresión, métricas y evidencia.
- `.github/pull_request_template.md` — agregar campos machine-readable para HU, riesgo, evidencia y modo de gate.
- `docs/calidad/matriz-trazabilidad.md` — normalizar filas para que puedan validarse por script.
- `docs/metricas/indicadores-calidad.md` — documentar qué indicadores serán generados automáticamente.
- `docs/calidad/suite-regresion.md` — alinear la taxonomía con `[Trait("Category", "Regression")]`.
- `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs` — marcar pruebas críticas de pedidos como regresión.
- `tests/LaMesaDelDuque.Pruebas/Persistencia/RepositorioIntegrationTests.cs` — marcar pruebas críticas de persistencia como regresión.

---

## Task 1: Crear pruebas base para scripts de calidad

**Files:**
- Create: `tests/quality/fixtures/trazabilidad-valida.md`
- Create: `tests/quality/fixtures/trazabilidad-ruta-invalida.md`
- Create: `tests/quality/fixtures/pr-body-valido.md`
- Create: `tests/quality/fixtures/pr-body-invalido.md`
- Create: `tests/quality/quality-scripts.Tests.ps1`

- [ ] **Step 1: Crear fixture de trazabilidad válida**

Crear `tests/quality/fixtures/trazabilidad-valida.md`:

```markdown
| HU | Criterio | Implementación | Pruebas | Estado |
|----|----------|----------------|---------|--------|
| HU-001 | CA-001 | `src/LaMesaDelDuque.Dominio/Entidades/Pedido.cs` | `tests/LaMesaDelDuque.Pruebas/Entidades/PedidoTests.cs` | Verificado |
```

- [ ] **Step 2: Crear fixture de trazabilidad inválida**

Crear `tests/quality/fixtures/trazabilidad-ruta-invalida.md`:

```markdown
| HU | Criterio | Implementación | Pruebas | Estado |
|----|----------|----------------|---------|--------|
| HU-999 | CA-999 | `src/Ruta/Inexistente.cs` | `tests/Ruta/InexistenteTests.cs` | Parcial |
```

- [ ] **Step 3: Crear fixtures de PR body**

Crear `tests/quality/fixtures/pr-body-valido.md`:

```markdown
## Evidencia de calidad

HU: HU-001
Riesgo: Bajo - cambio cubierto por pruebas unitarias y regresión.
Pruebas: dotnet test "LaMesaDelDuque.slnx"
Trazabilidad: docs/calidad/matriz-trazabilidad.md
```

Crear `tests/quality/fixtures/pr-body-invalido.md`:

```markdown
## Evidencia de calidad

Pruebas: pendiente
```

- [ ] **Step 4: Escribir pruebas Pester mínimas antes de implementar scripts**

Crear `tests/quality/quality-scripts.Tests.ps1`:

```powershell
Describe 'Quality Governance scripts' {
    $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')

    It 'rechaza trazabilidad con rutas inexistentes' {
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-ruta-invalida.md'
        & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo warning
        $LASTEXITCODE | Should -Be 1
    }

    It 'acepta trazabilidad con rutas existentes' {
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-valida.md'
        & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo warning
        $LASTEXITCODE | Should -Be 0
    }

    It 'rechaza un cuerpo de PR sin HU ni riesgo' {
        $script = Join-Path $repoRoot 'scripts/quality/validar-pr-evidencia.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/pr-body-invalido.md'
        & $script -PrBodyPath $fixture -Modo warning
        $LASTEXITCODE | Should -Be 1
    }
}
```

- [ ] **Step 5: Ejecutar pruebas y confirmar falla RED**

Run:

```powershell
pwsh -NoProfile -Command "Install-Module Pester -Force -Scope CurrentUser; Invoke-Pester tests/quality/quality-scripts.Tests.ps1"
```

Expected:

```text
Failed: scripts/quality/validar-trazabilidad.ps1 no existe
Failed: scripts/quality/validar-pr-evidencia.ps1 no existe
```

- [ ] **Step 6: Commit sugerido de ejecución**

```bash
git add tests/quality
git commit -m "test(calidad): agregar pruebas base de gobernanza"
```

---

## Task 2: Implementar validador de trazabilidad

**Files:**
- Create: `scripts/quality/validar-trazabilidad.ps1`
- Modify: `docs/calidad/matriz-trazabilidad.md`

- [ ] **Step 1: Implementar script mínimo**

Crear `scripts/quality/validar-trazabilidad.ps1`:

```powershell
param(
    [string]$MatrizPath = 'docs/calidad/matriz-trazabilidad.md',
    [string]$RepoRoot = '.',
    [ValidateSet('warning','strict')]
    [string]$Modo = 'warning'
)

$errores = New-Object System.Collections.Generic.List[string]
$root = Resolve-Path $RepoRoot

if (-not (Test-Path -LiteralPath $MatrizPath)) {
    $errores.Add("Matriz de trazabilidad no encontrada: $MatrizPath")
} else {
    $contenido = Get-Content -LiteralPath $MatrizPath -Raw
    $rutas = [regex]::Matches($contenido, '`([^`]+\.(cs|cshtml|md|json|yml|yaml))`')
    foreach ($ruta in $rutas) {
        $relativa = $ruta.Groups[1].Value
        if ($relativa.StartsWith('http')) { continue }
        $absoluta = Join-Path $root $relativa
        if (-not (Test-Path -LiteralPath $absoluta)) {
            $errores.Add("Ruta referenciada no existe: $relativa")
        }
    }

    if ($contenido -notmatch 'HU-\d{3}') {
        $errores.Add('La matriz no contiene identificadores HU-###.')
    }
}

foreach ($errorItem in $errores) {
    if ($Modo -eq 'strict') { Write-Error $errorItem } else { Write-Warning $errorItem }
}

if ($errores.Count -gt 0) { exit 1 }
Write-Host 'Validación de trazabilidad completada sin errores.'
exit 0
```

- [ ] **Step 2: Normalizar matriz de trazabilidad**

Actualizar `docs/calidad/matriz-trazabilidad.md` para que las rutas dentro de backticks apunten a archivos reales. Ejemplo de formato aceptado:

```markdown
| HU | Criterio | Implementación | Pruebas | Estado |
|----|----------|----------------|---------|--------|
| HU-001 | CA-001 | `src/LaMesaDelDuque.Dominio/Entidades/Pedido.cs` | `tests/LaMesaDelDuque.Pruebas/Entidades/PedidoTests.cs` | Verificado |
```

- [ ] **Step 3: Ejecutar pruebas del script**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester tests/quality/quality-scripts.Tests.ps1"
```

Expected:

```text
Tests Passed: 2
Tests Failed: 1
```

La prueba de PR seguirá fallando hasta Task 3.

- [ ] **Step 4: Ejecutar validación real del repo**

Run:

```powershell
pwsh -NoProfile -File scripts/quality/validar-trazabilidad.ps1 -MatrizPath docs/calidad/matriz-trazabilidad.md -RepoRoot . -Modo warning
```

Expected:

```text
Validación de trazabilidad completada sin errores.
```

- [ ] **Step 5: Commit sugerido de ejecución**

```bash
git add scripts/quality/validar-trazabilidad.ps1 docs/calidad/matriz-trazabilidad.md tests/quality
git commit -m "feat(calidad): validar trazabilidad documental"
```

---

## Task 3: Implementar validador de evidencia de PR

**Files:**
- Create: `scripts/quality/validar-pr-evidencia.ps1`
- Modify: `.github/pull_request_template.md`

- [ ] **Step 1: Agregar sección machine-readable al template**

Modificar `.github/pull_request_template.md` agregando después de `## Referencias`:

```markdown
## Evidencia de calidad

HU: 
Riesgo: 
Pruebas: 
Trazabilidad: docs/calidad/matriz-trazabilidad.md
```

- [ ] **Step 2: Implementar script**

Crear `scripts/quality/validar-pr-evidencia.ps1`:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$PrBodyPath,
    [ValidateSet('warning','strict')]
    [string]$Modo = 'warning'
)

$errores = New-Object System.Collections.Generic.List[string]

if (-not (Test-Path -LiteralPath $PrBodyPath)) {
    $errores.Add("No existe el archivo de cuerpo de PR: $PrBodyPath")
} else {
    $body = Get-Content -LiteralPath $PrBodyPath -Raw
    if ($body -notmatch 'HU:\s*HU-\d{3}') { $errores.Add('Falta HU con formato HU-###.') }
    if ($body -notmatch 'Riesgo:\s*\S+') { $errores.Add('Falta Riesgo con descripción no vacía.') }
    if ($body -notmatch 'Pruebas:\s*\S+') { $errores.Add('Falta evidencia de Pruebas.') }
    if ($body -notmatch 'Trazabilidad:\s*docs/calidad/matriz-trazabilidad\.md') { $errores.Add('Falta referencia a matriz de trazabilidad.') }
}

foreach ($errorItem in $errores) {
    if ($Modo -eq 'strict') { Write-Error $errorItem } else { Write-Warning $errorItem }
}

if ($errores.Count -gt 0) { exit 1 }
Write-Host 'Validación de evidencia de PR completada sin errores.'
exit 0
```

- [ ] **Step 3: Ejecutar Pester**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester tests/quality/quality-scripts.Tests.ps1"
```

Expected:

```text
Tests Passed: 3
Tests Failed: 0
```

- [ ] **Step 4: Commit sugerido de ejecución**

```bash
git add scripts/quality/validar-pr-evidencia.ps1 .github/pull_request_template.md tests/quality
git commit -m "feat(calidad): validar evidencia minima de pull requests"
```

---

## Task 4: Agregar taxonomía de regresión

**Files:**
- Modify: `docs/calidad/suite-regresion.md`
- Modify: `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs`
- Modify: `tests/LaMesaDelDuque.Pruebas/Persistencia/RepositorioIntegrationTests.cs`
- Modify: `.github/workflows/ci.yml`

- [ ] **Step 1: Documentar taxonomía**

Actualizar `docs/calidad/suite-regresion.md` con esta regla:

```markdown
## Taxonomía ejecutable

Las pruebas de regresión automatizadas deben marcarse con:

```csharp
[Trait("Category", "Regression")]
```

La suite se ejecuta con:

```powershell
dotnet test "LaMesaDelDuque.slnx" --filter "Category=Regression"
```
```

- [ ] **Step 2: Marcar pruebas críticas de pedidos**

En `tests/LaMesaDelDuque.Pruebas/Aplicacion/PedidosServicioTests.cs`, agregar `[Trait("Category", "Regression")]` a pruebas de:

- creación de pedido con detalle válido,
- cancelación de pedido pendiente,
- rechazo de modificación de pedido cancelado/cerrado,
- actualización de cantidad de detalle.

Ejemplo:

```csharp
[Fact]
[Trait("Category", "Regression")]
public async Task CrearPedidoAsync_DatosValidos_DebeCrearPedido()
```

- [ ] **Step 3: Marcar pruebas críticas de persistencia**

En `tests/LaMesaDelDuque.Pruebas/Persistencia/RepositorioIntegrationTests.cs`, agregar `[Trait("Category", "Regression")]` a pruebas que validen persistencia de pedidos, detalles y consultas por estado.

- [ ] **Step 4: Ejecutar regresión local**

Run:

```powershell
dotnet test "LaMesaDelDuque.slnx" --filter "Category=Regression"
```

Expected:

```text
Failed: 0
```

- [ ] **Step 5: Agregar job de regresión en CI**

Modificar `.github/workflows/ci.yml` y agregar job después de `compilar-y-probar`:

```yaml
  regresion:
    name: Suite de regresión
    runs-on: ubuntu-latest
    steps:
      - name: Obtener código fuente
        uses: actions/checkout@v4

      - name: Configurar .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restaurar dependencias
        run: dotnet restore

      - name: Ejecutar pruebas de regresión
        run: dotnet test "LaMesaDelDuque.slnx" --filter "Category=Regression" --verbosity normal
```

Actualizar `needs` del job `gobernanza` para incluir `regresion`.

- [ ] **Step 6: Commit sugerido de ejecución**

```bash
git add docs/calidad/suite-regresion.md tests/LaMesaDelDuque.Pruebas .github/workflows/ci.yml
git commit -m "test(calidad): agregar suite de regresion ejecutable"
```

---

## Task 5: Agregar umbral de cobertura progresivo

**Files:**
- Modify: `.github/workflows/ci.yml`
- Modify: `docs/metricas/indicadores-calidad.md`

- [ ] **Step 1: Agregar herramienta ReportGenerator en CI**

En `.github/workflows/ci.yml`, dentro de `compilar-y-probar`, después de ejecutar pruebas, agregar:

```yaml
      - name: Instalar ReportGenerator
        run: dotnet tool install --global dotnet-reportgenerator-globaltool

      - name: Generar resumen de cobertura
        run: reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:"MarkdownSummary;JsonSummary"
```

- [ ] **Step 2: Agregar validación progresiva de cobertura**

Agregar después del resumen:

```yaml
      - name: Validar umbral de cobertura en modo progresivo
        shell: pwsh
        run: |
          $summary = Get-Content "TestResults/CoverageReport/Summary.json" -Raw | ConvertFrom-Json
          $coverage = [double]$summary.summary.linecoverage
          Write-Host "Cobertura de líneas: $coverage%"
          if ($coverage -lt 80) {
            Write-Warning "La cobertura está por debajo del objetivo documental de 80%. Modo progresivo: no bloquea este PR."
          }
```

- [ ] **Step 3: Documentar métrica automatizada**

Actualizar `docs/metricas/indicadores-calidad.md` indicando:

```markdown
## IND-01 Cobertura automatizada

Fuente: `TestResults/CoverageReport/Summary.json` generado por CI.
Objetivo inicial: 80% de cobertura de líneas.
Modo v1: advertencia no bloqueante.
Modo estricto posterior: bloqueo de PR si la cobertura baja de 80%.
```

- [ ] **Step 4: Verificar localmente con pruebas completas**

Run:

```powershell
dotnet test "LaMesaDelDuque.slnx" --collect:"XPlat Code Coverage" --results-directory TestResults
```

Expected:

```text
Failed: 0
```

- [ ] **Step 5: Commit sugerido de ejecución**

```bash
git add .github/workflows/ci.yml docs/metricas/indicadores-calidad.md
git commit -m "ci(calidad): reportar umbral de cobertura progresivo"
```

---

## Task 6: Generar métricas de calidad y paquete de evidencia

**Files:**
- Create: `scripts/quality/generar-metricas-calidad.ps1`
- Create: `scripts/quality/generar-paquete-evidencia.ps1`
- Modify: `.github/workflows/ci.yml`

- [ ] **Step 1: Crear generador de métricas**

Crear `scripts/quality/generar-metricas-calidad.ps1`:

```powershell
param(
    [string]$OutputDir = 'artifacts/quality',
    [string]$CoverageSummaryPath = 'TestResults/CoverageReport/Summary.json'
)

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$coverage = $null
if (Test-Path -LiteralPath $CoverageSummaryPath) {
    $summary = Get-Content -LiteralPath $CoverageSummaryPath -Raw | ConvertFrom-Json
    $coverage = [double]$summary.summary.linecoverage
}

$metricas = [ordered]@{
    fechaUtc = (Get-Date).ToUniversalTime().ToString('o')
    coberturaLineas = $coverage
    objetivoCobertura = 80
    modoGate = 'progresivo-warning'
}

$jsonPath = Join-Path $OutputDir 'metricas-calidad.json'
$mdPath = Join-Path $OutputDir 'metricas-calidad.md'

$metricas | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $jsonPath -Encoding UTF8
@"
# Métricas de calidad

| Métrica | Valor |
|---|---:|
| Cobertura de líneas | $coverage |
| Objetivo de cobertura | 80 |
| Modo de gate | progresivo-warning |
"@ | Set-Content -LiteralPath $mdPath -Encoding UTF8

Write-Host "Métricas generadas en $OutputDir"
```

- [ ] **Step 2: Crear generador de paquete de evidencia**

Crear `scripts/quality/generar-paquete-evidencia.ps1`:

```powershell
param(
    [string]$OutputDir = 'artifacts/evidence',
    [string]$QualityDir = 'artifacts/quality',
    [string]$TestResultsDir = 'TestResults'
)

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

if (Test-Path -LiteralPath $QualityDir) {
    Copy-Item -Path (Join-Path $QualityDir '*') -Destination $OutputDir -Recurse -Force
}
if (Test-Path -LiteralPath $TestResultsDir) {
    Copy-Item -Path $TestResultsDir -Destination (Join-Path $OutputDir 'TestResults') -Recurse -Force
}

git rev-parse HEAD | Set-Content -LiteralPath (Join-Path $OutputDir 'commit.txt') -Encoding UTF8
git status --short | Set-Content -LiteralPath (Join-Path $OutputDir 'git-status.txt') -Encoding UTF8

Write-Host "Paquete de evidencia generado en $OutputDir"
```

- [ ] **Step 3: Integrar scripts en CI**

En `.github/workflows/ci.yml`, dentro del job `gobernanza`, después de validar documentos, agregar pasos:

```yaml
      - name: Validar trazabilidad en modo progresivo
        shell: pwsh
        run: pwsh -NoProfile -File scripts/quality/validar-trazabilidad.ps1 -Modo warning

      - name: Generar métricas de calidad
        shell: pwsh
        run: pwsh -NoProfile -File scripts/quality/generar-metricas-calidad.ps1

      - name: Generar paquete de evidencia
        shell: pwsh
        run: pwsh -NoProfile -File scripts/quality/generar-paquete-evidencia.ps1

      - name: Publicar evidencia de calidad
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: evidencia-calidad
          path: artifacts/
```

- [ ] **Step 4: Ejecutar scripts localmente**

Run:

```powershell
pwsh -NoProfile -File scripts/quality/generar-metricas-calidad.ps1
pwsh -NoProfile -File scripts/quality/generar-paquete-evidencia.ps1
```

Expected:

```text
Métricas generadas en artifacts/quality
Paquete de evidencia generado en artifacts/evidence
```

- [ ] **Step 5: Commit sugerido de ejecución**

```bash
git add scripts/quality .github/workflows/ci.yml
git commit -m "feat(calidad): generar metricas y evidencia de calidad"
```

---

## Task 7: Ruta de endurecimiento a modo bloqueante

**Files:**
- Modify: `docs/calidad/plan-calidad.md`
- Modify: `docs/calidad/definicion-de-hecho.md`
- Modify: `.github/workflows/ci.yml`

- [ ] **Step 1: Documentar criterio de transición**

Agregar en `docs/calidad/plan-calidad.md`:

```markdown
## Transición de calidad progresiva a calidad bloqueante

Quality Governance Pipeline v1 inicia en modo progresivo para evitar bloquear el desarrollo por deuda documental heredada. El modo estricto se activa cuando se cumplan estas condiciones:

1. La matriz de trazabilidad no tiene rutas rotas.
2. La suite de regresión contiene pruebas marcadas con `Category=Regression` para los flujos críticos de Sprint 1.
3. El paquete de evidencia se publica correctamente en CI durante al menos dos PR consecutivos.
4. La cobertura reportada tiene línea base conocida y no disminuye respecto al PR anterior.
```

- [ ] **Step 2: Actualizar definición de hecho**

Agregar en `docs/calidad/definicion-de-hecho.md`:

```markdown
Un cambio está listo para revisión cuando adjunta evidencia de calidad generada por CI o explica explícitamente por qué no aplica. En modo estricto, las validaciones de trazabilidad, regresión y cobertura son bloqueantes.
```

- [ ] **Step 3: Preparar switch técnico en CI**

En `.github/workflows/ci.yml`, declarar variable:

```yaml
env:
  QUALITY_GATE_MODE: warning
```

Usar `QUALITY_GATE_MODE` para llamar scripts:

```yaml
run: pwsh -NoProfile -File scripts/quality/validar-trazabilidad.ps1 -Modo $env:QUALITY_GATE_MODE
```

Para modo estricto posterior, cambiar `warning` a `strict`.

- [ ] **Step 4: Verificación final**

Run:

```powershell
pwsh -NoProfile -File scripts/quality/validar-trazabilidad.ps1 -Modo warning
pwsh -NoProfile -File scripts/quality/generar-metricas-calidad.ps1
pwsh -NoProfile -File scripts/quality/generar-paquete-evidencia.ps1
dotnet test "LaMesaDelDuque.slnx"
```

Expected:

```text
Validación de trazabilidad completada sin errores.
Métricas generadas en artifacts/quality
Paquete de evidencia generado en artifacts/evidence
Failed: 0
```

- [ ] **Step 5: Commit sugerido de ejecución**

```bash
git add docs/calidad/plan-calidad.md docs/calidad/definicion-de-hecho.md .github/workflows/ci.yml
git commit -m "docs(calidad): definir transicion a gates bloqueantes"
```

---

## Verificación completa de la rama

Ejecutar antes de abrir PR:

```powershell
pwsh -NoProfile -Command "Invoke-Pester tests/quality/quality-scripts.Tests.ps1"
pwsh -NoProfile -File scripts/quality/validar-trazabilidad.ps1 -Modo warning
pwsh -NoProfile -File scripts/quality/generar-metricas-calidad.ps1
pwsh -NoProfile -File scripts/quality/generar-paquete-evidencia.ps1
dotnet test "LaMesaDelDuque.slnx"
git status --short
```

Expected:

```text
Tests Failed: 0
Validación de trazabilidad completada sin errores.
Métricas generadas en artifacts/quality
Paquete de evidencia generado en artifacts/evidence
Failed: 0
```

`git status --short` debe mostrar únicamente archivos intencionales del cambio; no debe incluir secretos, `appsettings.Development.json`, `.sdd/`, `.atl/`, `.claude/`, `openspec/`, `AGENTS.md` ni `CLAUDE.md`.

---

## PR sugerido

Rama:

```bash
git checkout -b ci/quality-governance-pipeline-v1
```

Título:

```text
ci(calidad): agregar gobernanza progresiva de calidad
```

Descripción mínima:

```markdown
## Resumen
- Agrega validación progresiva de trazabilidad, evidencia de PR, regresión y métricas.
- Publica paquete de evidencia de calidad para auditoría y revisión.
- Define ruta para convertir las advertencias en gates bloqueantes.

## Evidencia de calidad
HU: HU-001
Riesgo: Medio - modifica la tubería de calidad sin cambiar lógica de negocio.
Pruebas: dotnet test "LaMesaDelDuque.slnx"; Invoke-Pester tests/quality/quality-scripts.Tests.ps1
Trazabilidad: docs/calidad/matriz-trazabilidad.md
```

---

## Self-review del plan

### Cobertura de alcance

- Traceability validation: Task 2 y Task 6.
- Coverage threshold: Task 5.
- Regression taxonomy: Task 4.
- PR evidence validation: Task 3.
- Quality evidence artifact: Task 6.
- Quality metrics baseline: Task 5 y Task 6.

### Placeholder scan

El plan no usa `TBD`, `TODO`, “implementar luego” ni instrucciones sin archivo objetivo. Los valores que deben variar por PR están expresados como campos concretos del template, no como placeholders técnicos del plan.

### Consistencia de tipos y rutas

- Los scripts usan parámetros `-Modo warning|strict` de forma consistente.
- Las rutas bajo `scripts/quality/`, `tests/quality/`, `.github/workflows/ci.yml`, `docs/calidad/` y `docs/metricas/` existen o se crean explícitamente.
- La verificación .NET usa `dotnet test "LaMesaDelDuque.slnx"`, no `dotnet build` local separado.
