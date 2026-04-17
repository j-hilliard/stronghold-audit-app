param(
    [string[]]$ChangedFiles = @(),
    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

function Convert-ToRelativePath {
    param([string]$AbsolutePath)

    $full = [System.IO.Path]::GetFullPath($AbsolutePath)
    if ($full.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($repoRoot.Length).TrimStart('\').Replace('\', '/')
    }

    return $AbsolutePath.Replace('\', '/')
}

function Add-Check {
    param(
        [System.Collections.Generic.List[object]]$List,
        [string]$Name,
        [string]$Status,
        [string]$Evidence,
        [string]$Action
    )

    $List.Add([pscustomobject]@{
        Name     = $Name
        Status   = $Status
        Evidence = $Evidence
        Action   = $Action
    })
}

$normalizedChanged = @()
foreach ($file in $ChangedFiles) {
    if ([string]::IsNullOrWhiteSpace($file)) { continue }
    $normalizedChanged += ($file.Replace('\', '/'))
}

$checks = [System.Collections.Generic.List[object]]::new()

$ensureCreatedMatches = @()
try {
    $ensureCreatedMatches = & rg -n --glob "*.cs" "EnsureCreated\(" Api Data 2>$null
} catch {
    $ensureCreatedMatches = @()
}

if ($ensureCreatedMatches.Count -gt 0) {
    Add-Check -List $checks `
        -Name 'No EnsureCreated() in runtime path' `
        -Status 'FAIL' `
        -Evidence ($ensureCreatedMatches -join '; ') `
        -Action 'Replace EnsureCreated with migrations-first flow (Database.Migrate + controlled seeding).'
} else {
    Add-Check -List $checks `
        -Name 'No EnsureCreated() in runtime path' `
        -Status 'PASS' `
        -Evidence 'No EnsureCreated() usage found in Api/ or Data/.' `
        -Action 'None'
}

$programHasMigrate = $false
$programPath = Join-Path $repoRoot 'Api\Program.cs'
if (Test-Path $programPath) {
    $programRaw = Get-Content -Raw $programPath
    if ($programRaw -match 'Database\.Migrate\(') {
        $programHasMigrate = $true
    }
}

if ($programHasMigrate) {
    Add-Check -List $checks `
        -Name 'Startup uses Database.Migrate()' `
        -Status 'PASS' `
        -Evidence 'Api/Program.cs contains Database.Migrate().' `
        -Action 'None'
} else {
    Add-Check -List $checks `
        -Name 'Startup uses Database.Migrate()' `
        -Status 'FAIL' `
        -Evidence 'Api/Program.cs missing Database.Migrate().' `
        -Action 'Add migration-first startup behavior before deployment.'
}

$rawSqlMatches = @()
try {
    $rawSqlMatches = & rg -n --glob "*.cs" "ExecuteSqlRaw\(" Api Data 2>$null
} catch {
    $rawSqlMatches = @()
}

$ddlIndicators = @('CREATE TABLE', 'ALTER TABLE', 'DROP TABLE', 'TRUNCATE TABLE')
$ddlMatches = @()
foreach ($line in $rawSqlMatches) {
    foreach ($indicator in $ddlIndicators) {
        if ($line.ToUpperInvariant().Contains($indicator)) {
            $ddlMatches += $line
            break
        }
    }
}

if ($ddlMatches.Count -gt 0) {
    Add-Check -List $checks `
        -Name 'No raw DDL bypassing EF migrations' `
        -Status 'FAIL' `
        -Evidence ($ddlMatches -join '; ') `
        -Action 'Move schema changes into EF migrations.'
} else {
    Add-Check -List $checks `
        -Name 'No raw DDL bypassing EF migrations' `
        -Status 'PASS' `
        -Evidence 'No obvious DDL via ExecuteSqlRaw found.' `
        -Action 'None'
}

$modelTouched = $false
$migrationTouched = $false
$contextTouched = $false
foreach ($file in $normalizedChanged) {
    if ($file -match '^Data/Models/.+\.cs$') { $modelTouched = $true }
    if ($file -match '^Data/Migrations/.+\.cs$') { $migrationTouched = $true }
    if ($file -ieq 'Data/AppDbContext.cs') { $contextTouched = $true }
}

if (($modelTouched -or $contextTouched) -and -not $migrationTouched) {
    Add-Check -List $checks `
        -Name 'Model/context changes include migration changes' `
        -Status 'WARN' `
        -Evidence 'Detected Data/Models or AppDbContext changes without Data/Migrations changes in this cycle.' `
        -Action 'Create migration before merge: dotnet ef migrations add <Name> --project Data --startup-project Api'
} elseif (($modelTouched -or $contextTouched) -and $migrationTouched) {
    Add-Check -List $checks `
        -Name 'Model/context changes include migration changes' `
        -Status 'PASS' `
        -Evidence 'Detected model/context and migration changes in the same cycle.' `
        -Action 'None'
} else {
    Add-Check -List $checks `
        -Name 'Model/context changes include migration changes' `
        -Status 'PASS' `
        -Evidence 'No model/context changes detected in this cycle.' `
        -Action 'None'
}

$designFactoryPath = Join-Path $repoRoot 'Data\DesignTimeContextFactory.cs'
if (Test-Path $designFactoryPath) {
    Add-Check -List $checks `
        -Name 'Design-time DbContext factory present' `
        -Status 'PASS' `
        -Evidence 'Data/DesignTimeContextFactory.cs exists.' `
        -Action 'None'
} else {
    Add-Check -List $checks `
        -Name 'Design-time DbContext factory present' `
        -Status 'WARN' `
        -Evidence 'Data/DesignTimeContextFactory.cs not found.' `
        -Action 'Add IDesignTimeDbContextFactory implementation for stable migration tooling.'
}

$toolManifestPath = Join-Path $repoRoot '.config\dotnet-tools.json'
$toolManifestHasEf = $false
if (Test-Path $toolManifestPath) {
    $toolManifestRaw = Get-Content -Raw $toolManifestPath
    if ($toolManifestRaw -match 'dotnet-ef') {
        $toolManifestHasEf = $true
    }
}

if ($toolManifestHasEf) {
    Add-Check -List $checks `
        -Name 'dotnet-ef tool manifest pinned' `
        -Status 'PASS' `
        -Evidence '.config/dotnet-tools.json includes dotnet-ef.' `
        -Action 'None'
} else {
    Add-Check -List $checks `
        -Name 'dotnet-ef tool manifest pinned' `
        -Status 'WARN' `
        -Evidence 'dotnet-ef not found in .config/dotnet-tools.json.' `
        -Action 'Add local tool manifest pin for dotnet-ef to keep CI/dev parity.'
}

$hasFail = $false
$hasWarn = $false
foreach ($check in $checks) {
    if ($check.Status -eq 'FAIL') { $hasFail = $true }
    if ($check.Status -eq 'WARN') { $hasWarn = $true }
}

$overallStatus = if ($hasFail) { 'FAIL' } elseif ($hasWarn) { 'WARN' } else { 'PASS' }

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $defaultDir = Join-Path $repoRoot "qa-runtime\reports\ef-guard"
    New-Item -ItemType Directory -Path $defaultDir -Force | Out-Null
    $OutputPath = Join-Path $defaultDir "ef-guard-$timestamp.md"
} else {
    $outputDir = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# EF Code-First Guard Report")
$lines.Add("")
$lines.Add("- Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$lines.Add("- Overall: **$overallStatus**")
$lines.Add("- Changed files observed: $($normalizedChanged.Count)")
$lines.Add("")
$lines.Add("| Check | Status | Evidence | Required Action |")
$lines.Add("|---|---|---|---|")
foreach ($check in $checks) {
    $evidence = $check.Evidence.Replace('|', '\|')
    $action = $check.Action.Replace('|', '\|')
    $lines.Add("| $($check.Name) | $($check.Status) | $evidence | $action |")
}

$lines.Add("")
$lines.Add("## Changed Files Seen")
if ($normalizedChanged.Count -eq 0) {
    $lines.Add("- None supplied.")
} else {
    foreach ($file in ($normalizedChanged | Sort-Object -Unique)) {
        $lines.Add(("- " + [char]96 + $file + [char]96))
    }
}

$lines | Set-Content -Path $OutputPath -Encoding UTF8

[pscustomobject]@{
    OverallStatus = $overallStatus
    OutputPath    = $OutputPath
    CheckCount    = $checks.Count
}
