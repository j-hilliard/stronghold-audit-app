$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$stateDir = Join-Path $repoRoot 'qa-runtime\agent-watch'
$pidFile = Join-Path $stateDir 'watcher.pid'
$statusFile = Join-Path $stateDir 'status.json'
$heartbeatFile = Join-Path $stateDir 'heartbeat.txt'

$watcherPid = $null
$running = $false

if (Test-Path $pidFile) {
    try {
        $pidData = (Get-Content -Raw $pidFile) | ConvertFrom-Json
        $watcherPid = [int]$pidData.Pid
        $proc = Get-Process -Id $watcherPid -ErrorAction SilentlyContinue
        $running = $null -ne $proc
    } catch {
        $running = $false
    }
}

$statusObj = $null
if (Test-Path $statusFile) {
    try { $statusObj = (Get-Content -Raw $statusFile) | ConvertFrom-Json } catch { $statusObj = $null }
}

$heartbeat = if (Test-Path $heartbeatFile) { (Get-Content -Raw $heartbeatFile).Trim() } else { '' }

[pscustomobject]@{
    Running    = $running
    Pid        = $watcherPid
    State      = if ($statusObj) { $statusObj.state } else { '' }
    Step       = if ($statusObj -and $statusObj.PSObject.Properties['step']) { $statusObj.step } else { '' }
    Message    = if ($statusObj) { $statusObj.message } else { '' }
    Overall    = if ($statusObj -and $statusObj.PSObject.Properties['cycleOverall']) { $statusObj.cycleOverall } else { '' }
    ChangedCount = if ($statusObj -and $statusObj.PSObject.Properties['changedCount']) { $statusObj.changedCount } else { 0 }
    SummaryPath  = if ($statusObj -and $statusObj.PSObject.Properties['summaryPath']) { $statusObj.summaryPath } else { '' }
    Timestamp  = if ($statusObj) { $statusObj.timestamp } else { '' }
    Heartbeat  = $heartbeat
    StatusFile = $statusFile
}
