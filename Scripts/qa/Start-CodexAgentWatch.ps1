param(
    [int]$PollSeconds = 1,
    [int]$DebounceSeconds = 3,
    [switch]$RunAllSuites,
    [bool]$Strict = $true,
    [switch]$NoNotify,
    [object]$Notify = $null,
    [switch]$Foreground,
    [switch]$Background
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$workerScript = Join-Path $repoRoot 'Scripts\qa\Run-CodexAgentWatch.ps1'
$stateDir = Join-Path $repoRoot 'qa-runtime\agent-watch'
$pidFile = Join-Path $stateDir 'watcher.pid'
$startupOut = Join-Path $stateDir 'startup.out.log'
$startupErr = Join-Path $stateDir 'startup.err.log'
$heartbeatFile = Join-Path $stateDir 'heartbeat.txt'
$eventLog = Join-Path $stateDir 'events.log'
$statusFile = Join-Path $stateDir 'status.json'

New-Item -ItemType Directory -Path $stateDir -Force | Out-Null

$explicitForeground = $Foreground.IsPresent
$explicitBackground = $Background.IsPresent
if ($explicitForeground -and $explicitBackground) {
    throw "Use either -Foreground or -Background, not both."
}

$effectiveForeground = $explicitForeground -or (-not $explicitBackground)

$effectiveNoNotify = $NoNotify.IsPresent
if ($PSBoundParameters.ContainsKey('Notify')) {
    if ($null -ne $Notify) {
        $notifyText = $Notify.ToString().Trim().ToLowerInvariant()
        if ($notifyText -in @('true', '1', '$true')) {
            $effectiveNoNotify = $false
        } elseif ($notifyText -in @('false', '0', '$false')) {
            $effectiveNoNotify = $true
        } else {
            throw "Invalid value for -Notify: '$Notify'. Use true/false, 1/0, or `$true/`$false."
        }
    }
}

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

        Remove-Item -LiteralPath $pidFile -Force -ErrorAction SilentlyContinue
    }
}

if ($effectiveForeground) {
    if ($Strict) {
        if ($RunAllSuites) {
            if ($effectiveNoNotify) {
                & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -RunAllSuites -Strict -NoNotify
            } else {
                & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -RunAllSuites -Strict
            }
        } else {
            if ($effectiveNoNotify) {
                & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -Strict -NoNotify
            } else {
                & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -Strict
            }
        }
    } else {
        if ($RunAllSuites) {
            if ($effectiveNoNotify) {
                & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -RunAllSuites -NoNotify
            } else {
                & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -RunAllSuites
            }
        } else {
            if ($effectiveNoNotify) {
                & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds -NoNotify
            } else {
                & $workerScript -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds
            }
        }
    }
    return
}

$argString = "-NoProfile -ExecutionPolicy Bypass -File `"$workerScript`" -PollSeconds $PollSeconds -DebounceSeconds $DebounceSeconds"
if ($Strict) { $argString += ' -Strict' }
if ($RunAllSuites) { $argString += ' -RunAllSuites' }
if ($effectiveNoNotify) { $argString += ' -NoNotify' }

if (Test-Path $startupOut) { Remove-Item -LiteralPath $startupOut -Force }
if (Test-Path $startupErr) { Remove-Item -LiteralPath $startupErr -Force }
if (Test-Path $heartbeatFile) { Remove-Item -LiteralPath $heartbeatFile -Force -ErrorAction SilentlyContinue }
$process = Start-Process -FilePath 'powershell' -ArgumentList $argString -WindowStyle Hidden -RedirectStandardOutput $startupOut -RedirectStandardError $startupErr -PassThru

Start-Sleep -Seconds 2
$running = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
if ($null -eq $running) {
    Write-Host "Codex watcher failed to stay running."
    if (Test-Path $startupOut) {
        Write-Host "---- startup.out ----"
        Get-Content $startupOut -ErrorAction SilentlyContinue
    }
    if (Test-Path $startupErr) {
        Write-Host "---- startup.err ----"
        Get-Content $startupErr -ErrorAction SilentlyContinue
    }
    throw "Watcher startup failed."
}

$heartbeatDeadline = (Get-Date).AddSeconds([Math]::Max(5, $PollSeconds + 3))
$heartbeatSeen = $false
while ((Get-Date) -lt $heartbeatDeadline) {
    if (Test-Path $heartbeatFile) {
        $heartbeatSeen = $true
        break
    }
    Start-Sleep -Milliseconds 250
}

if (-not $heartbeatSeen) {
    Write-Host "Codex watcher started but no heartbeat was recorded."
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    if (Test-Path $startupOut) {
        Write-Host "---- startup.out ----"
        Get-Content $startupOut -ErrorAction SilentlyContinue
    }
    if (Test-Path $startupErr) {
        Write-Host "---- startup.err ----"
        Get-Content $startupErr -ErrorAction SilentlyContinue
    }
    throw "Watcher did not become healthy (no heartbeat)."
}

Write-Host "Codex watcher started."
Write-Host "PID: $($process.Id)"
Write-Host "Stop with: .\Scripts\qa\Stop-CodexAgentWatch.ps1"
Write-Host "Startup logs: $startupOut / $startupErr"
Write-Host "Live status: $statusFile"
Write-Host "Event log: $eventLog"
