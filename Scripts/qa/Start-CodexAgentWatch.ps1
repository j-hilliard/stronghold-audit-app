param(
    [int]$PollSeconds = 1,
    [int]$DebounceSeconds = 3,
    [switch]$RunAllSuites,
    [bool]$Strict = $true,
    [switch]$Foreground
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$workerScript = Join-Path $repoRoot 'Scripts\qa\Run-CodexAgentWatch.ps1'
$stateDir = Join-Path $repoRoot 'qa-runtime\agent-watch'
$pidFile = Join-Path $stateDir 'watcher.pid'
$startupOut = Join-Path $stateDir 'startup.out.log'
$startupErr = Join-Path $stateDir 'startup.err.log'

New-Item -ItemType Directory -Path $stateDir -Force | Out-Null

if (Test-Path $pidFile) {
    $raw = Get-Content -Raw $pidFile
    $existing = $null
    try { $existing = $raw | ConvertFrom-Json } catch { $existing = $null }

    if ($null -ne $existing -and $existing.Pid) {
        $running = Get-Process -Id $existing.Pid -ErrorAction SilentlyContinue
        if ($null -ne $running) {
            Write-Host "Codex watcher already running (PID $($existing.Pid))."
            return
        }

        Remove-Item -LiteralPath $pidFile -Force
    }
}

if ($Foreground) {
    if ($RunAllSuites) {
        & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -RunAllSuites -Strict:$Strict
    } else {
        & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -Strict:$Strict
    }
    return
}

$strictLiteral = if ($Strict) { '1' } else { '0' }
$command = "& '$workerScript' -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -Strict $strictLiteral"
if ($RunAllSuites) { $command += ' -RunAllSuites' }

$argList = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', $command)

if (Test-Path $startupOut) { Remove-Item -LiteralPath $startupOut -Force }
if (Test-Path $startupErr) { Remove-Item -LiteralPath $startupErr -Force }
$process = Start-Process -FilePath 'powershell' -ArgumentList $argList -WindowStyle Hidden -RedirectStandardOutput $startupOut -RedirectStandardError $startupErr -PassThru

Write-Host "Codex watcher started."
Write-Host "PID: $($process.Id)"
Write-Host "Stop with: .\Scripts\qa\Stop-CodexAgentWatch.ps1"
Write-Host "Startup logs: $startupOut / $startupErr"
