param(
    [string]$OutputDir = 'artifacts/quality',
    [string]$CoverageSummaryPath = 'TestResults/CoverageReport/Summary.json'
)

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$coverage = $null
if (Test-Path -LiteralPath $CoverageSummaryPath) {
    try {
        $summary = Get-Content -LiteralPath $CoverageSummaryPath -Raw | ConvertFrom-Json
        if ($summary.PSObject.Properties.Name -contains 'summary') {
            $coverage = [double]$summary.summary.linecoverage
        }
    } catch {
        Write-Warning "No se pudo leer el resumen de cobertura: $($_.Exception.Message)"
    }
}

$metricas = [ordered]@{
    fechaUtc          = (Get-Date).ToUniversalTime().ToString('o')
    coberturaLineas   = $coverage
    objetivoCobertura = 80
    modoGate          = 'progresivo-warning'
}

$jsonPath = Join-Path $OutputDir 'metricas-calidad.json'
$mdPath   = Join-Path $OutputDir 'metricas-calidad.md'

$metricas | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$coberturaTexto = if ($null -eq $coverage) { 'No disponible' } else { "$coverage%" }

@"
# Metricas de calidad

| Metrica | Valor |
|---|---:|
| Cobertura de lineas | $coberturaTexto |
| Objetivo de cobertura | 80% |
| Modo de gate | progresivo-warning |

Generado: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC')
"@ | Set-Content -LiteralPath $mdPath -Encoding UTF8

Write-Output "Métricas generadas en $OutputDir"
exit 0
