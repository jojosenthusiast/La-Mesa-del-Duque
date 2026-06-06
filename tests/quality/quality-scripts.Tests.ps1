Describe 'Quality Governance scripts' {
    $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')

    It 'rechaza trazabilidad con rutas inexistentes' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-ruta-invalida.md'
        & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo warning
        $LASTEXITCODE | Should Be 1
    }

    It 'acepta trazabilidad con rutas existentes' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-valida.md'
        & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo warning
        $LASTEXITCODE | Should Be 0
    }

    It 'reporta error controlado cuando RepoRoot no existe' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $rutaInexistente = Join-Path $env:TEMP "repo-root-falso-$([guid]::NewGuid())"
        & $script -RepoRoot $rutaInexistente -Modo warning
        $LASTEXITCODE | Should Be 1
    }

    # === New: Traceability per-section CA validation ===

    It 'falla en modo strict cuando una seccion no tiene CA-###' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-sin-ca.md'
        & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo strict
        $LASTEXITCODE | Should Be 1
    }

    It 'advierte en modo warning cuando una seccion no tiene CA-### pero no bloquea' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-sin-ca.md'
        $output = & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo warning 3>&1
        $LASTEXITCODE | Should Be 0
        ($output -join "`n") | Should Match 'CA-###'
    }

    # === New: Planned row detection ===

    It 'falla en modo strict cuando hay filas planificadas' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-planificada.md'
        & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo strict
        $LASTEXITCODE | Should Be 1
    }

    It 'advierte en modo warning sobre filas planificadas pero no bloquea' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-planificada.md'
        $output = & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo warning 3>&1
        $LASTEXITCODE | Should Be 0
        ($output -join "`n") | Should Match 'planificada'
    }

    # === New: Backtick-less path detection ===

    It 'falla en modo strict cuando hay rutas sin backticks' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-rutas-sin-backtick.md'
        & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo strict
        $LASTEXITCODE | Should Be 1
    }

    It 'advierte en modo warning sobre rutas sin backticks pero no bloquea' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-trazabilidad.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/trazabilidad-rutas-sin-backtick.md'
        $output = & $script -MatrizPath $fixture -RepoRoot $repoRoot -Modo warning 3>&1
        $LASTEXITCODE | Should Be 0
        ($output -join "`n") | Should Match 'sin backticks'
    }

    # === Updated: PR evidence validator — strict mode blocks, warning mode does not ===

    It 'rechaza un cuerpo de PR sin HU ni riesgo en modo estricto' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-pr-evidencia.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/pr-body-invalido.md'
        & $script -PrBodyPath $fixture -Modo strict
        $LASTEXITCODE | Should Be 1
    }

    It 'advierte sobre un cuerpo de PR invalido en modo progresivo pero no bloquea' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-pr-evidencia.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/pr-body-invalido.md'
        $output = & $script -PrBodyPath $fixture -Modo warning 3>&1
        $LASTEXITCODE | Should Be 0
        ($output -join "`n") | Should Match 'modo progresivo no bloquea'
    }

    It 'acepta un cuerpo de PR con evidencia de calidad completa en modo estricto' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-pr-evidencia.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/pr-body-valido.md'
        & $script -PrBodyPath $fixture -Modo strict
        $LASTEXITCODE | Should Be 0
    }

    It 'acepta un cuerpo de PR valido tambien en modo warning' {
        $global:LASTEXITCODE = $null
        $script = Join-Path $repoRoot 'scripts/quality/validar-pr-evidencia.ps1'
        $fixture = Join-Path $repoRoot 'tests/quality/fixtures/pr-body-valido.md'
        & $script -PrBodyPath $fixture -Modo warning
        $LASTEXITCODE | Should Be 0
    }
}

Describe 'Umbral de cobertura progresivo — CI y documentacion' {
    $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')

    It 'CI workflow contiene paso Instalar ReportGenerator' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'Instalar ReportGenerator'
    }

    It 'CI workflow contiene paso Generar resumen de cobertura' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'Generar resumen de cobertura'
    }

    It 'CI workflow contiene paso Validar umbral de cobertura' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'Validar umbral de cobertura'
    }

    It 'CI workflow usa pwsh para validacion de cobertura' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'shell:\s*pwsh'
    }

    It 'CI workflow referencia Summary.json para extraer cobertura' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'Summary\.json'
    }

    It 'Indicadores de calidad incluye IND-09: Cobertura automatizada' {
        $indicadores = Get-Content (Join-Path $repoRoot 'docs/metricas/indicadores-calidad.md') -Raw
        $indicadores | Should Match 'IND-09: Cobertura automatizada'
    }

    It 'Indicadores de calidad referencia Summary.json como fuente de IND-09' {
        $indicadores = Get-Content (Join-Path $repoRoot 'docs/metricas/indicadores-calidad.md') -Raw
        $indicadores | Should Match "TestResults/CoverageReport/Summary\.json"
    }

    It 'Indicadores de calidad documenta objetivo de 80% en modo progresivo' {
        $indicadores = Get-Content (Join-Path $repoRoot 'docs/metricas/indicadores-calidad.md') -Raw
        $indicadores | Should Match '80%'
    }
}

Describe 'Generacion de metricas y evidencia' {
    $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')

    It 'CI workflow contiene paso Generar metricas de calidad' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'Generar metricas de calidad'
    }

    It 'CI workflow contiene paso Generar paquete de evidencia' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'Generar paquete de evidencia'
    }

    It 'CI workflow contiene paso Validar trazabilidad en modo progresivo' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'Validar trazabilidad en modo progresivo'
    }

    # === New: PR evidence CI integration ===

    It 'CI workflow gobernanza contiene paso Validar evidencia de PR en modo progresivo' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'Validar evidencia de PR en modo progresivo'
    }

    It 'CI workflow paso de evidencia de PR referencia validar-pr-evidencia.ps1' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'validar-pr-evidencia\.ps1'
    }

    It 'CI workflow paso de evidencia de PR usa variable QUALITY_GATE_MODE' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'PrBodyPath.*Modo.*QUALITY_GATE_MODE'
    }

    It 'CI workflow publica artefacto evidencia-calidad con path artifacts/' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'evidencia-calidad'
        $ciYaml | Should Match 'path:\s*artifacts/'
    }

    It 'generar-metricas-calidad.ps1 crea metricas-calidad.json y .md' {
        $OutputDir = Join-Path $env:TEMP "test-metrics-$([guid]::NewGuid())"
        $script = Join-Path $repoRoot 'scripts/quality/generar-metricas-calidad.ps1'
        & $script -OutputDir $OutputDir
        (Join-Path $OutputDir 'metricas-calidad.json') | Should Exist
        (Join-Path $OutputDir 'metricas-calidad.md') | Should Exist
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
    }

    It 'generar-metricas-calidad.ps1 maneja Summary.json ausente sin error' {
        $OutputDir = Join-Path $env:TEMP "test-metrics-$([guid]::NewGuid())"
        $script = Join-Path $repoRoot 'scripts/quality/generar-metricas-calidad.ps1'
        & $script -OutputDir $OutputDir -CoverageSummaryPath 'ruta/inexistente/Summary.json'
        (Join-Path $OutputDir 'metricas-calidad.json') | Should Exist
        $json = Get-Content (Join-Path $OutputDir 'metricas-calidad.json') -Raw | ConvertFrom-Json
        $json.coberturaLineas | Should BeNullOrEmpty
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
    }

    It 'generar-metricas-calidad.ps1 incluye campos obligatorios en JSON' {
        $OutputDir = Join-Path $env:TEMP "test-metrics-$([guid]::NewGuid())"
        $script = Join-Path $repoRoot 'scripts/quality/generar-metricas-calidad.ps1'
        & $script -OutputDir $OutputDir
        $json = Get-Content (Join-Path $OutputDir 'metricas-calidad.json') -Raw | ConvertFrom-Json
        $json.fechaUtc | Should Not BeNullOrEmpty
        $json.objetivoCobertura | Should Be 80
        $json.modoGate | Should Be 'progresivo-warning'
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
    }

    It 'generar-metricas-calidad.ps1 imprime mensaje de exito acentuado' {
        $OutputDir = Join-Path $env:TEMP "test-metrics-$([guid]::NewGuid())"
        $script = Join-Path $repoRoot 'scripts/quality/generar-metricas-calidad.ps1'
        $output = & $script -OutputDir $OutputDir 6>&1
        $output | Should Be "Métricas generadas en $OutputDir"
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
    }

    It 'generar-paquete-evidencia.ps1 crea commit.txt y git-status.txt' {
        $OutputDir = Join-Path $env:TEMP "test-evidence-$([guid]::NewGuid())"
        $script = Join-Path $repoRoot 'scripts/quality/generar-paquete-evidencia.ps1'
        & $script -OutputDir $OutputDir -QualityDir 'ruta/inexistente' -TestResultsDir 'ruta/inexistente'
        (Join-Path $OutputDir 'commit.txt') | Should Exist
        (Join-Path $OutputDir 'git-status.txt') | Should Exist
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
    }

    It 'generar-paquete-evidencia.ps1 produce commit.txt no vacio (hash o fallback)' {
        $OutputDir = Join-Path $env:TEMP "test-evidence-$([guid]::NewGuid())"
        $script = Join-Path $repoRoot 'scripts/quality/generar-paquete-evidencia.ps1'
        & $script -OutputDir $OutputDir -QualityDir 'ruta/inexistente' -TestResultsDir 'ruta/inexistente'
        $content = Get-Content (Join-Path $OutputDir 'commit.txt') -Raw
        $content | Should Not BeNullOrEmpty
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
    }

    It 'generar-paquete-evidencia.ps1 produce git-status.txt no vacio (estado o fallback)' {
        $OutputDir = Join-Path $env:TEMP "test-evidence-$([guid]::NewGuid())"
        $script = Join-Path $repoRoot 'scripts/quality/generar-paquete-evidencia.ps1'
        & $script -OutputDir $OutputDir -QualityDir 'ruta/inexistente' -TestResultsDir 'ruta/inexistente'
        $content = Get-Content (Join-Path $OutputDir 'git-status.txt') -Raw
        $content | Should Not BeNullOrEmpty
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
    }

    It 'generar-paquete-evidencia.ps1 commit.txt contiene hash valido o marcador de fallback' {
        $OutputDir = Join-Path $env:TEMP "test-evidence-$([guid]::NewGuid())"
        $script = Join-Path $repoRoot 'scripts/quality/generar-paquete-evidencia.ps1'
        & $script -OutputDir $OutputDir -QualityDir 'ruta/inexistente' -TestResultsDir 'ruta/inexistente'
        $content = Get-Content (Join-Path $OutputDir 'commit.txt') -Raw
        $trimmed = $content.Trim()
        ($trimmed -match '^\[git (no disponible|fallo con exit code \d+)\]$') -or ($trimmed -match '^[a-f0-9]{7,40}$') | Should Be $true
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
    }

    It 'CI workflow define variable QUALITY_GATE_MODE como warning' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match 'QUALITY_GATE_MODE:\s*warning'
    }

    It 'CI workflow usa env QUALITY_GATE_MODE en paso de trazabilidad' {
        $ciYaml = Get-Content (Join-Path $repoRoot '.github/workflows/ci.yml') -Raw
        $ciYaml | Should Match '\$env:QUALITY_GATE_MODE'
    }

    It 'Plan de calidad documenta transicion a calidad bloqueante' {
        $planCalidad = Get-Content (Join-Path $repoRoot 'docs/calidad/plan-calidad.md') -Raw
        $planCalidad | Should Match 'Transición de calidad progresiva'
    }

    It 'Definicion de Hecho exige evidencia de calidad para revision' {
        $dod = Get-Content (Join-Path $repoRoot 'docs/calidad/definicion-de-hecho.md') -Raw
        $dod | Should Match 'evidencia de calidad generada por CI'
    }

    It 'generar-paquete-evidencia.ps1 copia metricas cuando QualityDir existe' {
        $OutputDir = Join-Path $env:TEMP "test-evidence-$([guid]::NewGuid())"
        $QualityDir = Join-Path $env:TEMP "test-quality-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path (Join-Path $QualityDir 'nested') -Force | Out-Null
        Set-Content -Path (Join-Path $QualityDir 'dummy.json') -Value '{}'
        $script = Join-Path $repoRoot 'scripts/quality/generar-paquete-evidencia.ps1'
        & $script -OutputDir $OutputDir -QualityDir $QualityDir -TestResultsDir 'ruta/inexistente'
        (Join-Path $OutputDir 'dummy.json') | Should Exist
        (Join-Path $OutputDir 'nested') | Should Exist
        (Join-Path $OutputDir 'commit.txt') | Should Exist
        Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
        Remove-Item -Recurse -Force $QualityDir -ErrorAction SilentlyContinue
    }
}
