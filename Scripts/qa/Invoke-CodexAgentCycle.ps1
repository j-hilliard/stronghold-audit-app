param(
    [string[]]$ChangedFiles = @(),
    [string]$Reason = 'manual',
    [switch]$RunAllSuites,
    [bool]$Strict = $true
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$reportDir = Join-Path $repoRoot "qa-runtime\reports\agent-cycles\$timestamp"
$logDir = Join-Path $reportDir 'logs'

New-Item -ItemType Directory -Path $logDir -Force | Out-Null

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][scriptblock]$Script
    )

    $logFile = Join-Path $logDir "$Name.log"
    $started = Get-Date
    Write-Host ">> [$Name] Started $($started.ToString('HH:mm:ss'))"
    New-Item -ItemType File -Path $logFile -Force | Out-Null

    try {
        $global:LASTEXITCODE = 0
        & $Script 2>&1 | Tee-Object -FilePath $logFile
        $exitCode = if ($null -ne $global:LASTEXITCODE) { [int]$global:LASTEXITCODE } else { 0 }
        if ($exitCode -ne 0) {
            throw "Step '$Name' failed with exit code $exitCode"
        }

        return [pscustomobject]@{
            Name    = $Name
            Status  = 'PASS'
            Started = $started
            Ended   = Get-Date
            LogFile = $logFile
            Error   = ''
        }
    } catch {
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

$results = [System.Collections.Generic.List[object]]::new()

if ($frontendChanged -or $testInfraChanged -or $agentConfigChanged) {
    $results.Add((Invoke-Step -Name 'tester-baseline' -Script {
        Push-Location $repoRoot
        try {
            & cmd /c "npm --prefix webapp run qa:baseline"
        } finally {
            Pop-Location
        }
    }))

    if ($Strict) {
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

    if ($Strict) {
        $results.Add((Invoke-Step -Name 'frontend-build' -Script {
            Push-Location $repoRoot
            try {
                & cmd /c "npm --prefix webapp run build"
            } finally {
                Pop-Location
            }
        }))
    }
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

    if ($Strict) {
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

if ($Strict) {
    $results.Add((Invoke-Step -Name 'agent-isolation-guard' -Script {
        Push-Location $repoRoot
        try {
            $output = & rg -n "\.claude/|\.claude\\\\" .agents .codex/agents
            $code = if ($null -ne $global:LASTEXITCODE) { [int]$global:LASTEXITCODE } else { 0 }

            if ($code -eq 0) {
                throw ("Found forbidden .claude references: " + ($output -join '; '))
            }

            if ($code -eq 1) {
                $global:LASTEXITCODE = 0
                Write-Host "No forbidden .claude references detected."
                return
            }

            throw "rg failed while checking agent isolation (exit $code)."
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
$summaryLines.Add("- Strict mode: $Strict")
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

[pscustomobject]@{
    Overall    = $overall
    Summary    = $summaryPath
    ReportDir  = $reportDir
    StepCount  = $results.Count
    ChangeCount = $normalizedChanged.Count
}
