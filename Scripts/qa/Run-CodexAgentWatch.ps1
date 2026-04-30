param(
    [int]$PollSeconds = 1,
    [int]$DebounceSeconds = 3,
    [switch]$RunAllSuites,
    [switch]$Strict,
    [switch]$NoNotify
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$stateDir = Join-Path $repoRoot 'qa-runtime\agent-watch'
$logDir = Join-Path $stateDir 'logs'
$pidFile = Join-Path $stateDir 'watcher.pid'
$statusFile = Join-Path $stateDir 'status.json'
$eventLog = Join-Path $stateDir 'events.log'
$heartbeatFile = Join-Path $stateDir 'heartbeat.txt'
$runLog = Join-Path $logDir "watcher-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

New-Item -ItemType Directory -Path $logDir -Force | Out-Null

[pscustomobject]@{
    Pid       = $PID
    StartedAt = (Get-Date).ToString('s')
    Script    = $MyInvocation.MyCommand.Path
} | ConvertTo-Json | Set-Content -Path $pidFile -Encoding UTF8

function Write-RunLog {
    param([string]$Message)
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
    $line | Tee-Object -FilePath $runLog -Append
}

function Write-Status {
    param(
        [string]$State,
        [string]$Message,
        [string[]]$ChangedFiles = @(),
        [string]$CycleOverall = '',
        [string]$SummaryPath = ''
    )

    $payload = [pscustomobject]@{
        pid          = $PID
        state        = $State
        message      = $Message
        timestamp    = (Get-Date).ToString('s')
        changedCount = $ChangedFiles.Count
        changedFiles = $ChangedFiles
        cycleOverall = $CycleOverall
        summaryPath  = $SummaryPath
    }

    $payload | ConvertTo-Json -Depth 6 | Set-Content -Path $statusFile -Encoding UTF8
    "[{0}] {1} - {2}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $State, $Message | Add-Content -Path $eventLog -Encoding UTF8
}

function Send-ImmediateNotification {
    param(
        [string]$Title,
        [string]$Message
    )

    if ($NoNotify) { return }

    try {
        & msg.exe $env:USERNAME "[Codex Watcher] $Title - $Message" | Out-Null
    } catch {
        # Best-effort only; do not fail watcher on notification issues.
    }
}

function Get-RelativePath {
    param([string]$AbsolutePath)

    $full = [System.IO.Path]::GetFullPath($AbsolutePath)
    if ($full.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($repoRoot.Length).TrimStart('\').Replace('\', '/')
    }
    return $AbsolutePath.Replace('\', '/')
}

function Is-IgnoredPath {
    param([string]$RelativePath)

    $ignorePatterns = @(
        '^\.git/',
        '^qa-runtime/agent-watch/',
        '^qa-runtime/reports/',
        '^qa-artifacts/',
        '^webapp/node_modules/',
        '^webapp/playwright-report/',
        '^webapp/test-results/',
        '^webapp/\.playwright/',
        '^webapp/dist/',
        '^Api/bin/',
        '^Api/obj/',
        '^Data/bin/',
        '^Data/obj/',
        '^Shared/bin/',
        '^Shared/obj/'
    )

    foreach ($pattern in $ignorePatterns) {
        if ($RelativePath -match $pattern) { return $true }
    }

    return $false
}

function Get-Snapshot {
    $watchRoots = @(
        'Api',
        'Data',
        'Shared',
        'webapp/src',
        'webapp/tests',
        'Scripts/qa',
        '.agents',
        '.codex/agents'
    )
    $watchFiles = @(
        'webapp/package.json',
        'webapp/package-lock.json',
        'webapp/playwright.config.ts',
        'webapp/vite.config.ts',
        'webapp/vite.config.mjs',
        'webapp/tsconfig.json'
    )
    $snapshot = @{}

    foreach ($root in $watchRoots) {
        $absoluteRoot = Join-Path $repoRoot $root
        if (-not (Test-Path $absoluteRoot)) { continue }

        $items = Get-ChildItem -Path $absoluteRoot -File -Recurse -ErrorAction SilentlyContinue
        foreach ($item in $items) {
            $relative = Get-RelativePath $item.FullName
            if (Is-IgnoredPath -RelativePath $relative) { continue }
            $snapshot[$relative] = $item.LastWriteTimeUtc.Ticks
        }
    }

    foreach ($file in $watchFiles) {
        $absoluteFile = Join-Path $repoRoot $file
        if (-not (Test-Path $absoluteFile)) { continue }
        $item = Get-Item -LiteralPath $absoluteFile -ErrorAction SilentlyContinue
        if ($null -eq $item) { continue }
        $relative = Get-RelativePath $item.FullName
        $snapshot[$relative] = $item.LastWriteTimeUtc.Ticks
    }

    return $snapshot
}

function Get-Delta {
    param(
        [hashtable]$OldSnapshot,
        [hashtable]$NewSnapshot
    )

    $changed = [System.Collections.Generic.List[string]]::new()

    foreach ($path in $NewSnapshot.Keys) {
        if (-not $OldSnapshot.ContainsKey($path)) {
            $changed.Add($path)
            continue
        }

        if ($OldSnapshot[$path] -ne $NewSnapshot[$path]) {
            $changed.Add($path)
        }
    }

    foreach ($path in $OldSnapshot.Keys) {
        if (-not $NewSnapshot.ContainsKey($path)) {
            $changed.Add($path)
        }
    }

    return $changed
}

Write-RunLog "Codex agent watcher started. PID=$PID, poll=${PollSeconds}s, debounce=${DebounceSeconds}s, runAll=$RunAllSuites, strict=$($Strict.IsPresent), notify=$(-not $NoNotify)"
Write-Status -State 'WATCHING' -Message 'Watcher started.'
Set-Content -Path $heartbeatFile -Value (Get-Date).ToString('s') -Encoding UTF8

$snapshot = Get-Snapshot
$pending = [System.Collections.Generic.HashSet[string]]::new()
$lastChangeAt = $null

try {
    while ($true) {
        try {
            Start-Sleep -Seconds $PollSeconds

            $newSnapshot = Get-Snapshot
            $delta = Get-Delta -OldSnapshot $snapshot -NewSnapshot $newSnapshot
            $snapshot = $newSnapshot

            if ($delta.Count -gt 0) {
                foreach ($file in $delta) { $null = $pending.Add($file) }
                $lastChangeAt = Get-Date
                Write-RunLog ("Detected {0} file changes." -f $delta.Count)
                Write-Status -State 'CHANGE_DETECTED' -Message ("Detected {0} changed files." -f $pending.Count) -ChangedFiles @($pending)
                Send-ImmediateNotification -Title 'Change Detected' -Message ("{0} file(s) queued for test cycle." -f $pending.Count)
            }

            if ($pending.Count -eq 0 -or $null -eq $lastChangeAt) {
                Set-Content -Path $heartbeatFile -Value (Get-Date).ToString('s') -Encoding UTF8
                continue
            }

            $idleSeconds = ((Get-Date) - $lastChangeAt).TotalSeconds
            if ($idleSeconds -lt $DebounceSeconds) {
                Set-Content -Path $heartbeatFile -Value (Get-Date).ToString('s') -Encoding UTF8
                continue
            }

            [string[]]$changedFiles = @($pending)
            $pending.Clear()
            $lastChangeAt = $null

            Write-RunLog ("Starting auto-cycle for {0} files." -f $changedFiles.Count)
            Write-Status -State 'CYCLE_RUNNING' -Message ("Running cycle for {0} changed files." -f $changedFiles.Count) -ChangedFiles $changedFiles
            Send-ImmediateNotification -Title 'Cycle Started' -Message ("Testing started for {0} changed file(s)." -f $changedFiles.Count)

            $cycleResult = $null
            $cycleExitCode = 0
            Push-Location $repoRoot
            try {
                $cycleScriptPath = Join-Path $repoRoot 'Scripts\qa\Invoke-CodexAgentCycle.ps1'
                $argsList = @(
                    '-NoProfile',
                    '-ExecutionPolicy', 'Bypass',
                    '-File', $cycleScriptPath,
                    '-Reason', 'file-change',
                    '-Strict', ([bool]$Strict),
                    '-ChangedFiles'
                ) + $changedFiles

                if ($RunAllSuites) {
                    $argsList += '-RunAllSuites'
                }

                $cycleResult = & powershell @argsList 2>&1 | Tee-Object -FilePath $runLog -Append
                $cycleExitCode = if ($null -ne $LASTEXITCODE) { [int]$LASTEXITCODE } else { 0 }
            } finally {
                Pop-Location
            }

            $overall = if ($cycleExitCode -eq 0) { 'PASS' } else { 'FAIL' }
            $summaryPath = ''
            try {
                $lastObj = $cycleResult | Where-Object { $_ -is [pscustomobject] } | Select-Object -Last 1
                if ($lastObj -and $lastObj.PSObject.Properties['Summary']) {
                    $summaryPath = [string]$lastObj.Summary
                }
                if ($lastObj -and $lastObj.PSObject.Properties['Overall']) {
                    $overall = [string]$lastObj.Overall
                }
            } catch {
                # best effort
            }

            Write-RunLog ("Cycle finished: {0}" -f $overall)
            Write-Status -State 'CYCLE_COMPLETE' -Message ("Cycle result: {0}" -f $overall) -ChangedFiles $changedFiles -CycleOverall $overall -SummaryPath $summaryPath
            Send-ImmediateNotification -Title ('Cycle ' + $overall) -Message (if ($summaryPath) { "See summary: $summaryPath" } else { "See watcher logs for details." })

            Set-Content -Path $heartbeatFile -Value (Get-Date).ToString('s') -Encoding UTF8
        } catch {
            Write-RunLog "Watcher loop error: $($_.Exception.Message)"
            Write-Status -State 'WATCHER_ERROR' -Message $_.Exception.Message
            Send-ImmediateNotification -Title 'Watcher Error' -Message $_.Exception.Message
            Set-Content -Path $heartbeatFile -Value (Get-Date).ToString('s') -Encoding UTF8
            Start-Sleep -Seconds 1
        }
    }
} finally {
    if (Test-Path $pidFile) {
        try {
            $raw = Get-Content -Raw $pidFile | ConvertFrom-Json
            if ($raw.Pid -eq $PID) {
                Remove-Item -LiteralPath $pidFile -Force
            }
        } catch {
            Remove-Item -LiteralPath $pidFile -Force -ErrorAction SilentlyContinue
        }
    }
}
