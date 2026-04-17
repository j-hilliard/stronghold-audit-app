$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$pidFile = Join-Path $repoRoot 'qa-runtime\agent-watch\watcher.pid'

if (-not (Test-Path $pidFile)) {
    Write-Host "No watcher PID file found."
    return
}

$raw = Get-Content -Raw $pidFile
$pidData = $null
try {
    $pidData = $raw | ConvertFrom-Json
} catch {
    Remove-Item -LiteralPath $pidFile -Force
    Write-Host "Removed invalid watcher PID file."
    return
}

if ($null -eq $pidData -or -not $pidData.Pid) {
    Remove-Item -LiteralPath $pidFile -Force
    Write-Host "Removed empty watcher PID file."
    return
}

$process = Get-Process -Id $pidData.Pid -ErrorAction SilentlyContinue
if ($null -eq $process) {
    Remove-Item -LiteralPath $pidFile -Force
    Write-Host "Watcher process not running. Cleared PID file."
    return
}

Stop-Process -Id $pidData.Pid -Force
Start-Sleep -Milliseconds 300

if (Test-Path $pidFile) {
    Remove-Item -LiteralPath $pidFile -Force
}

Write-Host "Codex watcher stopped (PID $($pidData.Pid))."
