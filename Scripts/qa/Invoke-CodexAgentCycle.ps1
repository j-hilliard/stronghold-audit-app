param(
    [string[]]$ChangedFiles = @(),
    [string]$Reason = 'manual',
    [switch]$RunAllSuites,
    [object]$Strict = $true
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$reportDir = Join-Path $repoRoot "qa-runtime\reports\agent-cycles\$timestamp"
$logDir = Join-Path $reportDir 'logs'
$watchStateDir = Join-Path $repoRoot 'qa-runtime\agent-watch'
$eventLog = Join-Path $watchStateDir 'events.log'
$statusFile = Join-Path $watchStateDir 'status.json'

$strictEnabled = $true
if ($null -ne $Strict) {
    $strictText = $Strict.ToString().Trim().ToLowerInvariant()
    if ($strictText -in @('true', '1', '$true')) {
        $strictEnabled = $true
    } elseif ($strictText -in @('false', '0', '$false')) {
        $strictEnabled = $false
    } else {
        throw "Invalid value for -Strict: '$Strict'. Use true/false, 1/0, or `$true/`$false."
    }
}

New-Item -ItemType Directory -Path $logDir -Force | Out-Null
New-Item -ItemType Directory -Path $watchStateDir -Force | Out-Null

function Write-AgentEvent {
    param([string]$Type, [string]$Message)
    "[{0}] {1} - {2}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $Type, $Message | Add-Content -Path $eventLog -Encoding UTF8
}

function Write-WatcherStatus {
    param(
        [string]$State,
        [string]$Message,
        [string]$Step = '',
        [string]$Overall = '',
        [string]$SummaryPath = ''
    )

    $payload = [pscustomobject]@{
        pid          = ''
        state        = $State
        message      = $Message
        step         = $Step
        timestamp    = (Get-Date).ToString('s')
        changedCount = $normalizedChanged.Count
        changedFiles = $normalizedChanged
        cycleOverall = $Overall
        summaryPath  = $SummaryPath
    }

    $payload | ConvertTo-Json -Depth 6 | Set-Content -Path $statusFile -Encoding UTF8
}

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][scriptblock]$Script
    )

    $logFile = Join-Path $logDir "$Name.log"
    $started = Get-Date
    Write-Host ">> [$Name] Started $($started.ToString('HH:mm:ss'))"
    New-Item -ItemType File -Path $logFile -Force | Out-Null
    Write-AgentEvent -Type 'STEP_START' -Message $Name
    Write-WatcherStatus -State 'CYCLE_RUNNING' -Message ("Running step: {0}" -f $Name) -Step $Name

    try {
        $global:LASTEXITCODE = 0
        & $Script 2>&1 | Tee-Object -FilePath $logFile
        $exitCode = if ($null -ne $global:LASTEXITCODE) { [int]$global:LASTEXITCODE } else { 0 }
        if ($exitCode -ne 0) {
            throw "Step '$Name' failed with exit code $exitCode"
        }

        Write-AgentEvent -Type 'STEP_PASS' -Message $Name

        return [pscustomobject]@{
            Name    = $Name
            Status  = 'PASS'
            Started = $started
            Ended   = Get-Date
            LogFile = $logFile
            Error   = ''
        }
    } catch {
        Write-AgentEvent -Type 'STEP_FAIL' -Message ("{0} :: {1}" -f $Name, $_.Exception.Message)
        return [pscustomobject]@{
            Name    = $Name
            Status  = 'FAIL'
            Started = $started
            Ended   = Get-Date
            LogFile = $logFile
            Error   = $_.Exception.Message
        }
    }
}

function Has-PathMatch {
    param(
        [string[]]$Files,
        [string]$Pattern
    )

    foreach ($file in $Files) {
        if ($file -match $Pattern) { return $true }
    }
    return $false
}

$normalizedChanged = @()
foreach ($file in $ChangedFiles) {
    if ([string]::IsNullOrWhiteSpace($file)) { continue }
    $normalizedChanged += $file.Replace('\', '/')
}

$frontendChanged = Has-PathMatch -Files $normalizedChanged -Pattern '^webapp/'
$backendChanged = Has-PathMatch -Files $normalizedChanged -Pattern '^(Api|Data|Shared)/'
$testInfraChanged = Has-PathMatch -Files $normalizedChanged -Pattern '^(Scripts/qa|webapp/tests)/'
$agentConfigChanged = Has-PathMatch -Files $normalizedChanged -Pattern '^(\.agents|\.codex/agents)/'

if ($RunAllSuites) {
    $frontendChanged = $true
    $backendChanged = $true
}

Write-AgentEvent -Type 'CYCLE_START' -Message ("Reason={0}; Strict={1}; ChangedFiles={2}" -f $Reason, $strictEnabled, $normalizedChanged.Count)
Write-WatcherStatus -State 'CYCLE_RUNNING' -Message 'Starting cycle.'

$results = [System.Collections.Generic.List[object]]::new()

if ($frontendChanged -or $testInfraChanged -or $agentConfigChanged) {
    $results.Add((Invoke-Step -Name 'ui-audit-visual-coverage-report' -Script {
        Push-Location $repoRoot
        try {
            & powershell -NoProfile -ExecutionPolicy Bypass -File "Scripts\qa\Invoke-AuditVisualCoverageReport.ps1" -OutputPath (Join-Path $reportDir 'audit-visual-coverage.md')
        } finally {
            Pop-Location
        }
    }))

    if ($strictEnabled) {
        $results.Add((Invoke-Step -Name 'frontend-build' -Script {
            Push-Location $repoRoot
            try {
                & cmd /c "npm --prefix webapp run build"
            } finally {
                Pop-Location
            }
        }))

        $results.Add((Invoke-Step -Name 'ui-audit-visual-all-pages' -Script {
            Push-Location $repoRoot
            try {
                & cmd /c "npm --prefix webapp run test:e2e:audit:visual:all"
            } finally {
                Pop-Location
            }
        }))
    }

    $results.Add((Invoke-Step -Name 'tester-baseline' -Script {
        Push-Location $repoRoot
        try {
            & cmd /c "npm --prefix webapp run qa:baseline"
        } finally {
            Pop-Location
        }
    }))

    if ($strictEnabled) {
        $results.Add((Invoke-Step -Name 'tester-template-gate' -Script {
            Push-Location $repoRoot
            try {
                & cmd /c "npm --prefix webapp run test:e2e:audit:template-gate"
            } finally {
                Pop-Location
            }
        }))

        $results.Add((Invoke-Step -Name 'tester-audit-parity' -Script {
            Push-Location $repoRoot
            try {
                & cmd /c "npm --prefix webapp run test:e2e:audit:parity"
            } finally {
                Pop-Location
            }
        }))

        $results.Add((Invoke-Step -Name 'tester-corrective-actions-gate' -Script {
            Push-Location $repoRoot
            try {
                & cmd /c "npm --prefix webapp run test:e2e:audit:corrective-actions"
            } finally {
                Pop-Location
            }
        }))

        $results.Add((Invoke-Step -Name 'tester-phase2c-workflow-gate' -Script {
            Push-Location $repoRoot
            try {
                & cmd /c "npm --prefix webapp run test:e2e:audit:phase2c"
            } finally {
                Pop-Location
            }
        }))
    }

    $results.Add((Invoke-Step -Name 'ui-composer-gate' -Script {
        Push-Location $repoRoot
        try {
            & cmd /c "npm --prefix webapp run test:e2e:audit:composer-gate"
        } finally {
            Pop-Location
        }
    }))

    $results.Add((Invoke-Step -Name 'ui-reporting-gate' -Script {
        Push-Location $repoRoot
        try {
            & cmd /c "npm --prefix webapp run test:e2e:audit:reporting-gate"
        } finally {
            Pop-Location
        }
    }))

}

if ($backendChanged -or $testInfraChanged -or $agentConfigChanged) {
    $results.Add((Invoke-Step -Name 'ef-guard-agent' -Script {
        Push-Location $repoRoot
        try {
            & powershell -NoProfile -ExecutionPolicy Bypass -File "Scripts\qa\Invoke-EfCodeFirstGuard.ps1" -ChangedFiles $normalizedChanged -OutputPath (Join-Path $reportDir 'ef-guard.md')
        } finally {
            Pop-Location
        }
    }))

    if ($strictEnabled) {
        $results.Add((Invoke-Step -Name 'improver-backend-build' -Script {
            Push-Location $repoRoot
            try {
                $env:DOTNET_CLI_HOME = Join-Path $repoRoot 'qa-runtime\dotnet-cli-home'
                $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
                $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
                New-Item -ItemType Directory -Path $env:DOTNET_CLI_HOME -Force | Out-Null
                & dotnet build Api/Api.csproj -nologo -p:OutDir=qa-runtime/build/api/ -p:UseSharedCompilation=false /nodeReuse:false
            } finally {
                Pop-Location
            }
        }))
    } else {
        $results.Add((Invoke-Step -Name 'improver-backend-build-lite' -Script {
            Push-Location $repoRoot
            try {
                $env:DOTNET_CLI_HOME = Join-Path $repoRoot 'qa-runtime\dotnet-cli-home'
                $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
                $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
                New-Item -ItemType Directory -Path $env:DOTNET_CLI_HOME -Force | Out-Null
                & dotnet build Data/Data.csproj -nologo
            } finally {
                Pop-Location
            }
        }))
    }
}

if ($strictEnabled) {
    $results.Add((Invoke-Step -Name 'agent-isolation-guard' -Script {
        Push-Location $repoRoot
        try {
            $pattern = '\.claude/|\.claude\\'
            $matches = @()

            if (Get-Command rg -ErrorAction SilentlyContinue) {
                $output = & rg -n $pattern .agents .codex/agents
                $code = if ($null -ne $global:LASTEXITCODE) { [int]$global:LASTEXITCODE } else { 0 }

                if ($code -eq 0) {
                    $matches = @($output)
                } elseif ($code -eq 1) {
                    $global:LASTEXITCODE = 0
                } else {
                    throw "rg failed while checking agent isolation (exit $code)."
                }
            } else {
                $scanRoots = @('.agents', '.codex/agents') | ForEach-Object { Join-Path $repoRoot $_ } | Where-Object { Test-Path $_ }
                foreach ($root in $scanRoots) {
                    $fileMatches = Get-ChildItem -Path $root -Recurse -File -ErrorAction SilentlyContinue |
                        Select-String -Pattern $pattern -ErrorAction SilentlyContinue
                    if ($fileMatches) {
                        $matches += $fileMatches | ForEach-Object { "{0}:{1}:{2}" -f $_.Path, $_.LineNumber, $_.Line.Trim() }
                    }
                }
            }

            if ($matches.Count -gt 0) {
                throw ("Found forbidden .claude references: " + ($matches -join '; '))
            }

            Write-Host "No forbidden .claude references detected."
        } finally {
            Pop-Location
        }
    }))
}

if ($results.Count -eq 0) {
    $results.Add([pscustomobject]@{
        Name    = 'no-op'
        Status  = 'PASS'
        Started = Get-Date
        Ended   = Get-Date
        LogFile = ''
        Error   = 'No watched file category changed.'
    })
}

$overall = 'PASS'
foreach ($result in $results) {
    if ($result.Status -eq 'FAIL') {
        $overall = 'FAIL'
        break
    }
}

$summaryPath = Join-Path $reportDir 'summary.md'
$summaryLines = [System.Collections.Generic.List[string]]::new()
$summaryLines.Add("# Codex Agent Auto-Cycle")
$summaryLines.Add("")
$summaryLines.Add("- Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$summaryLines.Add("- Reason: $Reason")
$summaryLines.Add("- Strict mode: $strictEnabled")
$summaryLines.Add("- Overall: **$overall**")
$summaryLines.Add("- Changed files in cycle: $($normalizedChanged.Count)")
$summaryLines.Add("")
$summaryLines.Add("## Step Results")
$summaryLines.Add("| Step | Status | Started | Ended | Log | Error |")
$summaryLines.Add("|---|---|---|---|---|---|")
foreach ($result in $results) {
    if ([string]::IsNullOrWhiteSpace($result.LogFile)) {
        $logDisplay = '-'
    } elseif (Test-Path $result.LogFile) {
        $logDisplay = (Resolve-Path -LiteralPath $result.LogFile).Path
    } else {
        $logDisplay = "$($result.LogFile) (missing)"
    }

    $errorText = if ([string]::IsNullOrWhiteSpace($result.Error)) { '-' } else { $result.Error.Replace('|', '\|') }
    $summaryLines.Add("| $($result.Name) | $($result.Status) | $($result.Started.ToString('HH:mm:ss')) | $($result.Ended.ToString('HH:mm:ss')) | $logDisplay | $errorText |")
}

$summaryLines.Add("")
$summaryLines.Add("## Changed Files")
if ($normalizedChanged.Count -eq 0) {
    $summaryLines.Add("- None supplied.")
} else {
    foreach ($file in ($normalizedChanged | Sort-Object -Unique)) {
        $summaryLines.Add(("- " + [char]96 + $file + [char]96))
    }
}

$summaryLines | Set-Content -Path $summaryPath -Encoding UTF8

Write-Host ""
Write-Host "Codex agent cycle complete: $overall"
Write-Host "Summary: $summaryPath"
Write-AgentEvent -Type 'CYCLE_COMPLETE' -Message ("Overall={0}; Summary={1}" -f $overall, $summaryPath)
Write-WatcherStatus -State 'CYCLE_COMPLETE' -Message ("Cycle complete: {0}" -f $overall) -Overall $overall -SummaryPath $summaryPath

[pscustomobject]@{
    Overall    = $overall
    Summary    = $summaryPath
    ReportDir  = $reportDir
    StepCount  = $results.Count
    ChangeCount = $normalizedChanged.Count
}

if ($overall -eq 'FAIL') {
    exit 1
}

exit 0
