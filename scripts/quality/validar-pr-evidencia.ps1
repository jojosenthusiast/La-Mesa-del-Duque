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

if ($errores.Count -gt 0) {
    foreach ($errorItem in $errores) {
        if ($Modo -eq 'strict') { Write-Error $errorItem } else { Write-Warning $errorItem }
    }
    if ($Modo -eq 'strict') {
        exit 1
    } else {
        Write-Warning 'Validación de evidencia de PR: se encontraron advertencias pero el modo progresivo no bloquea el CI.'
        exit 0
    }
}
Write-Output 'Validación de evidencia de PR completada sin errores.'
exit 0
