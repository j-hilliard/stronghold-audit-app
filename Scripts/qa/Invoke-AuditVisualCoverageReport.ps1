param(
    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $coverageDir = Join-Path $repoRoot 'qa-runtime\reports\visual-coverage'
    New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null
    $OutputPath = Join-Path $coverageDir "audit-visual-coverage-$timestamp.md"
}

$moduleRouterPath = Join-Path $repoRoot 'webapp\src\modules\audit-management\router\index.ts'
$rootRouterPath = Join-Path $repoRoot 'webapp\src\router\index.ts'
$snapshotSpecs = @(
    (Join-Path $repoRoot 'webapp\tests\e2e\audit-visual.spec.ts'),
    (Join-Path $repoRoot 'webapp\tests\e2e\audit-visual-all-pages.spec.ts')
)

function Normalize-RoutePath {
    param([string]$PathValue)

    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        return ''
    }

    $normalized = $PathValue.Trim()
    $queryIndex = $normalized.IndexOf('?')
    if ($queryIndex -ge 0) {
        $normalized = $normalized.Substring(0, $queryIndex)
    }

    if (-not $normalized.StartsWith('/')) {
        $normalized = "/$normalized"
    }

    if ($normalized.Length -gt 1) {
        $normalized = $normalized.TrimEnd('/')
    }

    return $normalized
}

function Get-RoutesFromAuditModuleRouter {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) {
        throw "Missing router file: $FilePath"
    }

    $content = Get-Content -LiteralPath $FilePath -Raw
    $matches = [regex]::Matches($content, "path:\s*'(?<path>[^']+)'")

    $routes = New-Object System.Collections.Generic.List[string]
    $routes.Add('/audit-management')

    foreach ($match in $matches) {
        $routePath = $match.Groups['path'].Value
        if ($routePath -eq '') {
            continue
        }

        if ($routePath.StartsWith('/')) {
            $routes.Add((Normalize-RoutePath $routePath))
        } else {
            $routes.Add((Normalize-RoutePath "/audit-management/$routePath"))
        }
    }

    return $routes
}

function Get-StandaloneAuditRoutes {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) {
        throw "Missing router file: $FilePath"
    }

    $content = Get-Content -LiteralPath $FilePath -Raw
    $matches = [regex]::Matches($content, "path:\s*'(?<path>/audit-management[^']*)'")
    $routes = New-Object System.Collections.Generic.List[string]

    foreach ($match in $matches) {
        $routes.Add((Normalize-RoutePath $match.Groups['path'].Value))
    }

    return $routes
}

function Get-SnapshotMap {
    param([string[]]$SpecPaths)

    $map = @{}
    $pattern = "(?s)\{\s*path:\s*'(?<path>[^']+)'\s*,\s*screenshotName:\s*'(?<shot>[^']+)'"

    foreach ($specPath in $SpecPaths) {
        if (-not (Test-Path $specPath)) {
            continue
        }

        $content = Get-Content -LiteralPath $specPath -Raw
        $matches = [regex]::Matches($content, $pattern)

        foreach ($match in $matches) {
            $routePath = Normalize-RoutePath $match.Groups['path'].Value
            $shotName = $match.Groups['shot'].Value
            if ([string]::IsNullOrWhiteSpace($routePath) -or [string]::IsNullOrWhiteSpace($shotName)) {
                continue
            }

            if (-not $map.ContainsKey($routePath)) {
                $map[$routePath] = New-Object System.Collections.Generic.HashSet[string]
            }
            [void]$map[$routePath].Add($shotName)
        }
    }

    return $map
}

function Test-DynamicRouteCovered {
    param(
        [string]$DynamicRoute,
        [string[]]$SnapshotRoutes
    )

    $escaped = [regex]::Escape($DynamicRoute)
    $pattern = '^' + ($escaped -replace ':[A-Za-z0-9_]+', '[^/]+') + '$'

    foreach ($snapshotRoute in $SnapshotRoutes) {
        if ($snapshotRoute -match $pattern) {
            return $true
        }
    }

    return $false
}

$allRoutes = New-Object System.Collections.Generic.HashSet[string]

foreach ($route in (Get-RoutesFromAuditModuleRouter -FilePath $moduleRouterPath)) {
    [void]$allRoutes.Add($route)
}

foreach ($route in (Get-StandaloneAuditRoutes -FilePath $rootRouterPath)) {
    [void]$allRoutes.Add($route)
}

$snapshotMap = Get-SnapshotMap -SpecPaths $snapshotSpecs
$snapshotRoutes = @($snapshotMap.Keys)

$rows = New-Object System.Collections.Generic.List[object]
$coveredCount = 0
$missingCount = 0

foreach ($route in ($allRoutes | Sort-Object)) {
    $isDynamic = $route.Contains(':')
    $covered = $false
    $snapshots = @()

    if ($snapshotMap.ContainsKey($route)) {
        $covered = $true
        $snapshots = @($snapshotMap[$route] | Sort-Object)
    } elseif ($isDynamic) {
        $covered = Test-DynamicRouteCovered -DynamicRoute $route -SnapshotRoutes $snapshotRoutes
        if ($covered) {
            $matched = @()
            $escaped = [regex]::Escape($route)
            $pattern = '^' + ($escaped -replace ':[A-Za-z0-9_]+', '[^/]+') + '$'
            foreach ($snapshotRoute in $snapshotRoutes) {
                if ($snapshotRoute -match $pattern) {
                    $matched += @($snapshotMap[$snapshotRoute] | Sort-Object)
                }
            }
            $snapshots = $matched | Sort-Object -Unique
        }
    }

    if ($covered) {
        $coveredCount += 1
    } else {
        $missingCount += 1
    }

    $rows.Add([pscustomobject]@{
        Route      = $route
        Type       = if ($isDynamic) { 'dynamic' } else { 'static' }
        Covered    = $covered
        Snapshots  = if ($snapshots.Count -gt 0) { ($snapshots -join ', ') } else { '-' }
    }) | Out-Null
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('# Audit Playwright Visual Coverage')
$lines.Add('')
$lines.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$lines.Add("- Total audit-management routes: $($rows.Count)")
$lines.Add("- Covered by visual snapshots: $coveredCount")
$lines.Add("- Missing visual snapshots: $missingCount")
$lines.Add("- Snapshot specs scanned: " + ($snapshotSpecs -join ', '))
$lines.Add('')
$lines.Add('## Route Coverage')
$lines.Add('| Route | Type | Visual Snapshot Coverage | Snapshot Files |')
$lines.Add('|---|---|---|---|')

foreach ($row in $rows) {
    $coverageText = if ($row.Covered) { 'Yes' } else { 'No' }
    $lines.Add("| $($row.Route) | $($row.Type) | $coverageText | $($row.Snapshots) |")
}

$missingRoutes = $rows | Where-Object { -not $_.Covered }
$lines.Add('')
$lines.Add('## Missing Route Snapshots')
if ($missingRoutes.Count -eq 0) {
    $lines.Add('- None.')
} else {
    foreach ($route in $missingRoutes) {
        $lines.Add("- " + $route.Route)
    }
}

$lines.Add('')
$lines.Add('## UX Findings (Automated)')
if ($missingRoutes.Count -eq 0) {
    $lines.Add('- PASS: Every audit-management route currently has visual snapshot coverage.')
} else {
    $lines.Add("- FAIL: $($missingRoutes.Count) routes are missing visual baselines. These routes are blind spots for visual regressions and Vite overlay detection.")
    $lines.Add('- Action: add these routes to webapp/tests/e2e/audit-visual*.spec.ts with toHaveScreenshot(...) assertions.')
}

$parentDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $parentDir)) {
    New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
}

$lines | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "Audit visual coverage report generated:"
Write-Host $OutputPath

[pscustomobject]@{
    OutputPath = $OutputPath
    TotalRoutes = $rows.Count
    Covered = $coveredCount
    Missing = $missingCount
}
