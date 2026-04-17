param(
    [int]$PollSeconds = 1,
    [int]$DebounceSeconds = 3,
    [switch]$RunAllSuites,
    [switch]$Strict
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$stateDir = Join-Path $repoRoot 'qa-runtime\agent-watch'
$logDir = Join-Path $stateDir 'logs'
$pidFile = Join-Path $stateDir 'watcher.pid'
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
    $watchRoots = @('Api', 'Data', 'Shared', 'webapp', 'Scripts', '.agents', '.codex/agents')
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

Write-RunLog "Codex agent watcher started. PID=$PID, poll=${PollSeconds}s, debounce=${DebounceSeconds}s, runAll=$RunAllSuites, strict=$($Strict.IsPresent)"

$snapshot = Get-Snapshot
$pending = [System.Collections.Generic.HashSet[string]]::new()
$lastChangeAt = $null

while ($true) {
    Start-Sleep -Seconds $PollSeconds

    $newSnapshot = Get-Snapshot
    $delta = Get-Delta -OldSnapshot $snapshot -NewSnapshot $newSnapshot
    $snapshot = $newSnapshot

    if ($delta.Count -gt 0) {
        foreach ($file in $delta) { $null = $pending.Add($file) }
        $lastChangeAt = Get-Date
        Write-RunLog ("Detected {0} file changes." -f $delta.Count)
    }

    if ($pending.Count -eq 0 -or $null -eq $lastChangeAt) {
        continue
    }

    $idleSeconds = ((Get-Date) - $lastChangeAt).TotalSeconds
    if ($idleSeconds -lt $DebounceSeconds) {
        continue
    }

    [string[]]$changedFiles = @($pending)
    $pending.Clear()
    $lastChangeAt = $null

    Write-RunLog ("Starting auto-cycle for {0} files." -f $changedFiles.Count)

    try {
        Push-Location $repoRoot
        try {
            $cycleArgs = @{
                ChangedFiles = $changedFiles
                Reason       = 'file-change'
                Strict       = [bool]$Strict
            }

            if ($RunAllSuites) {
                $cycleArgs.RunAllSuites = $true
            }

            & powershell -NoProfile -ExecutionPolicy Bypass -File "Scripts\qa\Invoke-CodexAgentCycle.ps1" @cycleArgs | Tee-Object -FilePath $runLog -Append
        } finally {
            Pop-Location
        }
    } catch {
        Write-RunLog "Auto-cycle failed: $($_.Exception.Message)"
    }
}
