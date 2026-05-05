#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Regenerates webapp/src/apiclient/auditClient.ts from the .NET API via NSwag.

.DESCRIPTION
    NSwag cannot run directly from the OneDrive path because the folder name contains
    a comma ("Quanta Services Management Partnership, L.P"), which breaks MSBuild tooling.
    This script works around that by creating a temporary directory junction at C:\StrongholdDev
    (no special characters), running NSwag from there, then removing the junction.

    The generated file will be written to:
        webapp/src/apiclient/client.g.ts

    Review the generated output and merge any new DTOs or endpoint changes into
    the handwritten auditClient.ts. The handwritten file is the single source of truth
    for the Vue app — do not replace it wholesale with the generated output without review.

.NOTES
    Run this script from an elevated PowerShell prompt (Run as Administrator).
    The API project must build successfully before running this script.

.EXAMPLE
    .\Scripts\generate-api-client.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$junctionPath = 'C:\StrongholdDev'
$scriptDir    = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot  = Split-Path -Parent $scriptDir

Write-Host "NSwag API client generator" -ForegroundColor Cyan
Write-Host "Project root : $projectRoot"
Write-Host "Junction path: $junctionPath"
Write-Host ""

# 1. Remove stale junction if present
if (Test-Path $junctionPath) {
    $item = Get-Item $junctionPath
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        Write-Host "Removing stale junction at $junctionPath ..."
        cmd /c "rmdir `"$junctionPath`""
    } else {
        throw "ERROR: $junctionPath already exists and is NOT a junction. Remove it manually first."
    }
}

# 2. Create junction
Write-Host "Creating junction $junctionPath -> $projectRoot ..."
cmd /c "mklink /J `"$junctionPath`" `"$projectRoot`""

try {
    # 3. Restore .NET tools via junction path
    $apiPath = "$junctionPath\Api"
    Write-Host "Restoring .NET local tools ..."
    Push-Location $apiPath
    dotnet tool restore
    Pop-Location

    # 4. Build the API to ensure latest DLLs
    Write-Host "Building API project ..."
    dotnet build "$apiPath\Api.csproj" --no-incremental -c Debug

    # 5. Run NSwag
    $nswagJson = "$apiPath\nswag.json"
    Write-Host "Running NSwag from $nswagJson ..."
    Push-Location $apiPath
    dotnet nswag run nswag.json /runtime:Net80
    Pop-Location

    $generatedSrc = "$junctionPath\WebApp\src\apiclient\client.ts"
    $generatedDst = "$projectRoot\webapp\src\apiclient\client.g.ts"

    if (Test-Path $generatedSrc) {
        Copy-Item $generatedSrc $generatedDst -Force
        Write-Host ""
        Write-Host "Generated client written to: webapp/src/apiclient/client.g.ts" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "  1. Review client.g.ts for new DTOs, changed fields, renamed endpoints."
        Write-Host "  2. Merge changes into webapp/src/apiclient/auditClient.ts (the live source)."
        Write-Host "  3. Delete client.g.ts when merge is complete (it is gitignored)."
    } else {
        Write-Warning "NSwag did not produce $generatedSrc — check NSwag output above for errors."
    }
} finally {
    # 6. Always remove the junction
    if (Test-Path $junctionPath) {
        Write-Host "Removing junction $junctionPath ..."
        cmd /c "rmdir `"$junctionPath`""
    }
}
