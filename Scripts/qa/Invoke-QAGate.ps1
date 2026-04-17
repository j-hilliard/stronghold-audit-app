param(
    [ValidateSet('baseline', 'full', 'pr', 'premerge', 'prerelease')]
    [string]$Gate = 'baseline',
    [switch]$RequireLive,
    [switch]$EnableTemplateGate,
    [switch]$EnableReportingGate,
    [string]$ArtifactsRoot = 'qa-artifacts'
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$webappPath = Join-Path $repoRoot 'webapp'
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$artifactPath = Join-Path $repoRoot "$ArtifactsRoot\$timestamp-$Gate"
$logPath = Join-Path $artifactPath 'logs'

# Always start from a clean transient Playwright state.
$transientPaths = @(
    (Join-Path $webappPath 'playwright-report'),
    (Join-Path $webappPath 'test-results'),
    (Join-Path $webappPath '.playwright')
)
foreach ($path in $transientPaths) {
    if (Test-Path $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
    }
}

New-Item -ItemType Directory -Path $logPath -Force | Out-Null

if ($RequireLive) {
    $env:PW_LIVE_GATE = 'true'
} else {
    $env:PW_LIVE_GATE = 'false'
}

$env:PW_AUDIT_TEMPLATE_GATE = if ($EnableTemplateGate) { 'true' } else { 'false' }
$env:PW_AUDIT_REPORTING_GATE = if ($EnableReportingGate) { 'true' } else { 'false' }
$env:PW_AUDIT_COMPOSER_GATE = if ($EnableReportingGate) { 'true' } else { 'false' }

function Invoke-GateStep {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Command
    )

    $logFile = Join-Path $logPath "$Name.log"
    Write-Host ">> [$Name] $Command"

    Push-Location $webappPath
    try {
        & cmd /c $Command 2>&1 | Tee-Object -FilePath $logFile
        if ($LASTEXITCODE -ne 0) {
            throw "Step '$Name' failed with exit code $LASTEXITCODE."
        }
    } finally {
        Pop-Location
    }
}

function Copy-Artifacts {
    param([string]$SourcePath)

    if (Test-Path $SourcePath) {
        $name = Split-Path $SourcePath -Leaf
        Copy-Item -Path $SourcePath -Destination (Join-Path $artifactPath $name) -Recurse -Force
    }
}

switch ($Gate) {
    'baseline' {
        Invoke-GateStep -Name 'qa-baseline' -Command 'npm run qa:baseline'
    }
    'full' {
        Invoke-GateStep -Name 'qa-full' -Command 'npm run qa:full'
    }
    'pr' {
        Invoke-GateStep -Name 'qa-pr' -Command 'npm run qa:pr'
    }
    'premerge' {
        Invoke-GateStep -Name 'qa-premerge' -Command 'npm run qa:premerge'
    }
    'prerelease' {
        Invoke-GateStep -Name 'qa-prerelease' -Command 'npm run qa:prerelease'
    }
}

Copy-Artifacts -SourcePath (Join-Path $webappPath 'playwright-report')
Copy-Artifacts -SourcePath (Join-Path $webappPath 'test-results')

$summary = @"
QA Gate: $Gate
Require live checks: $RequireLive
Template admin gate enabled: $EnableTemplateGate
Reporting gate enabled: $EnableReportingGate
Run time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Artifacts: $artifactPath
"@

$summary | Set-Content -Path (Join-Path $artifactPath 'summary.txt') -Encoding UTF8

Write-Host ''
Write-Host "QA gate completed successfully."
Write-Host "Artifacts: $artifactPath"
