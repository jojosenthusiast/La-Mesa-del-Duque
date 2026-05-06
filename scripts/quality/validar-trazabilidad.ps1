param(
    [string]$MatrizPath = 'docs/calidad/matriz-trazabilidad.md',
    [string]$RepoRoot = '.',
    [ValidateSet('warning','strict')]
    [string]$Modo = 'warning'
)

# Two-tier error system:
#   $errores — hard errors, always cause exit 1 (broken backtick paths, missing matrix, etc.)
#   $advertencias — soft errors, warn in warning mode, fail in strict mode (missing CA, planned rows, un-backticked paths)
$errores = New-Object System.Collections.Generic.List[string]
$advertencias = New-Object System.Collections.Generic.List[string]

# Resolve RepoRoot with controlled error handling
$rootAbsoluto = $null
try {
    $rootAbsoluto = [string](Resolve-Path -LiteralPath $RepoRoot -ErrorAction Stop)
} catch {
    $msg = "RepoRoot no se pudo resolver: $RepoRoot. Detalle: $($_.Exception.Message)"
    if ($Modo -eq 'strict') { Write-Error $msg } else { Write-Warning $msg }
    exit 1
}

# Build absolute matrix path — resolve relative paths against RepoRoot
if ([System.IO.Path]::IsPathRooted($MatrizPath)) {
    $matrizAbsoluta = $MatrizPath
} else {
    $matrizAbsoluta = Join-Path $rootAbsoluto $MatrizPath
}

if (-not (Test-Path -LiteralPath $matrizAbsoluta)) {
    $errores.Add("Matriz de trazabilidad no encontrada: $MatrizPath")
} else {
    $contenido = Get-Content -LiteralPath $matrizAbsoluta -Raw

    # ---- 1. Backtick path validation (hard error) ----
    $rutas = [regex]::Matches($contenido, '`([^`]+\.(cs|cshtml|md|json|yml|yaml))`')
    foreach ($ruta in $rutas) {
        $relativa = $ruta.Groups[1].Value

        # Skip URLs by detecting protocol scheme (http, https, ftp, file, etc.)
        if ($relativa -match '^[a-zA-Z][a-zA-Z0-9+.-]*://') { continue }

        # Normalize: remove anchors (#...) and query strings (?...) before validation
        $normalizada = $relativa -replace '[#?].*$', ''

        $absoluta = Join-Path $rootAbsoluto $normalizada
        if (-not (Test-Path -LiteralPath $absoluta)) {
            $errores.Add("Ruta referenciada no existe: $relativa")
        }
    }

    # ---- 2. Per-section CA-### validation (soft error) ----
    # Extract HU sections: sections starting with ### HU-NNN (where NNN is 3+ digits)
    $secciones = [regex]::Matches($contenido, '###\s+HU-(\d{3,}).*?(?=###\s+HU-\d{3,}|##\s+\d+\.\s+\S+)', [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $huSectionsFound = $false
    foreach ($seccion in $secciones) {
        $huNum = $seccion.Groups[1].Value
        $huSectionsFound = $true
        $bloque = $seccion.Groups[0].Value

        # Skip HU-000 (architectural base, not a user story with acceptance criteria)
        if ($huNum -eq '000') { continue }

        # Check if section references CA-###
        if ($bloque -notmatch 'CA-\d{3,}') {
            $msg = "Seccion HU-${huNum}: no contiene referencia CA-###."
            $advertencias.Add($msg)
        }
    }

    # ---- 3. Planned row detection (soft error) ----
    $planificados = [regex]::Matches($contenido, '\(planificado\)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($planificados.Count -gt 0) {
        $msg = "Se encontraron $($planificados.Count) fila(s) planificada(s) con marcador '(planificado)'. Las rutas planificadas podrian no existir aun."
        $advertencias.Add($msg)
    }

    # ---- 4. Backtick-less path detection (soft error) ----
    # Detect repo-like paths (src/, tests/, docs/, scripts/, .github/, artifacts/) NOT wrapped in backticks
    $lineas = $contenido -split "`r`n|`n"
    $lineNumber = 0
    foreach ($linea in $lineas) {
        $lineNumber++
        # Remove all backtick-wrapped content to isolate raw text
        $sinBackticks = $linea -replace '`[^`]*`', ''

        # Search for paths that look like repo references outside backticks
        # Pattern: src/... or tests/... or docs/... or scripts/... or .github/... or artifacts/...
        if ($sinBackticks -match '\b(src|tests|docs|scripts|\.github|artifacts|workflows)\/[^\s\)\|,\]]+') {
            $rutaCruda = $matches[0].Trim()

            # If the raw path itself contains (planificado), it falls under planned row policy, not un-backticked
            if ($rutaCruda -notmatch 'planificado') {
                $msg = "Linea $lineNumber : Ruta sin backticks detectada: $rutaCruda"
                $advertencias.Add($msg)
            }
        }
    }

    # ---- 5. Global HU presence check (hard error) ----
    if ($contenido -notmatch 'HU-\d{3}') {
        $errores.Add('La matriz no contiene identificadores HU-###.')
    }
}

# Emit warnings for soft errors
foreach ($advertencia in $advertencias) {
    if ($Modo -eq 'strict') {
        Write-Error $advertencia
    } else {
        Write-Warning $advertencia
    }
}

# Emit errors for hard errors
foreach ($errorItem in $errores) {
    if ($Modo -eq 'strict') { Write-Error $errorItem } else { Write-Warning $errorItem }
}

# Determine exit code
$tieneErroresDuros = $errores.Count -gt 0
$tieneErroresSuaves = $advertencias.Count -gt 0

if ($tieneErroresDuros) { exit 1 }
if ($tieneErroresSuaves -and $Modo -eq 'strict') { exit 1 }

Write-Output 'Validación de trazabilidad completada sin errores.'
exit 0
