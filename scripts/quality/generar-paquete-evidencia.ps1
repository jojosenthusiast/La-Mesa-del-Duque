param(
    [string]$OutputDir = 'artifacts/evidence',
    [string]$QualityDir = 'artifacts/quality',
    [string]$TestResultsDir = 'TestResults'
)

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

if (Test-Path -LiteralPath $QualityDir) {
    $items = Get-ChildItem -Path $QualityDir -ErrorAction SilentlyContinue
    if ($items) {
        Copy-Item -Path (Join-Path $QualityDir '*') -Destination $OutputDir -Recurse -Force
    }
}

if (Test-Path -LiteralPath $TestResultsDir) {
    Copy-Item -Path $TestResultsDir -Destination (Join-Path $OutputDir 'TestResults') -Recurse -Force
}

# Git evidence with robust exit-code validation.
# PowerShell try/catch does NOT catch native-command failures (non-zero exit code).
# We explicitly inspect $LASTEXITCODE after each git invocation.
if (Get-Command git -ErrorAction SilentlyContinue) {
    # --- commit hash ---
    $commitLines = @(git rev-parse HEAD 2>&1)
    if ($LASTEXITCODE -eq 0 -and $commitLines) {
        $commitLines | Set-Content -LiteralPath (Join-Path $OutputDir 'commit.txt') -Encoding UTF8
    } else {
        $exitCode = if ($LASTEXITCODE) { $LASTEXITCODE } else { 1 }
        $marker = "[git fallo con exit code $exitCode]"
        Write-Warning "No se pudo obtener el hash del commit (exit $exitCode)."
        $marker | Set-Content -LiteralPath (Join-Path $OutputDir 'commit.txt') -Encoding UTF8
    }

    # --- working-tree status ---
    $statusLines = @(git status --short 2>&1)
    if ($LASTEXITCODE -ne 0) {
        $marker = "[git status fallo con exit code $LASTEXITCODE]"
        Write-Warning "No se pudo obtener el estado de git (exit $LASTEXITCODE)."
        $marker | Set-Content -LiteralPath (Join-Path $OutputDir 'git-status.txt') -Encoding UTF8
    } elseif ($statusLines) {
        $statusLines | Set-Content -LiteralPath (Join-Path $OutputDir 'git-status.txt') -Encoding UTF8
    } else {
        # Clean working tree — valid, but add marker for audit clarity.
        '[sin cambios]' | Set-Content -LiteralPath (Join-Path $OutputDir 'git-status.txt') -Encoding UTF8
    }
} else {
    Write-Warning "Git no encontrado. La evidencia de commit y estado usara marcadores de fallback."
    '[git no disponible]' | Set-Content -LiteralPath (Join-Path $OutputDir 'commit.txt') -Encoding UTF8
    '[git no disponible]' | Set-Content -LiteralPath (Join-Path $OutputDir 'git-status.txt') -Encoding UTF8
}

Write-Output "Paquete de evidencia generado en $OutputDir"
exit 0
