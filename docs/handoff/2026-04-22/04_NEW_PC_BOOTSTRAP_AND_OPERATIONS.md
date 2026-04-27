# 04 - New PC Bootstrap and Operations Runbook
**Objective:** Get from fresh machine to reliable local execution quickly.

## 1) Prerequisites
- .NET SDK matching `global.json` (project currently targets .NET 10)
- Node.js LTS (Node 20+ recommended for current Vite/Playwright toolchain)
- npm
- SQL Server local instance or container reachable by local connection string
- Git access to repo

## 2) Clone + Install
```powershell
git clone <repo-url>
cd "Stronghold Audit App"
dotnet restore
npm --prefix webapp install
```

## 3) Local Configuration
Create/update local config files:
- `Api/appsettings.Local.json` (connection string, optional email/AI settings)
- `webapp/.env.local` (API base URL and optional auth bypass flags)

## 4) Start App (Recommended)
```powershell
.\dev-start.bat
```
This should start:
- frontend at `http://localhost:7220`
- API at `http://localhost:7221`

## 5) Manual Start (Fallback)
### API
```powershell
cd Api
$env:ASPNETCORE_ENVIRONMENT = "Local"
$env:ASPNETCORE_URLS = "http://localhost:7221"
dotnet build Api.csproj -p:skipNswagClientGeneration=true
dotnet run --no-build --no-launch-profile
```

### Frontend
```powershell
cd webapp
npm run dev
```

## 6) Health Checks (Do Not Skip)
Run after startup:
```powershell
Invoke-WebRequest http://localhost:7221/v1/divisions -UseBasicParsing
Invoke-WebRequest "http://localhost:7221/v1/audits?take=1" -UseBasicParsing
Invoke-WebRequest http://localhost:7221/v1/admin/templates -UseBasicParsing
```
Expected: HTTP `200` for all three.

If these fail, frontend will show cascading “Failed to load …” errors.

## 7) Common Recovery Commands
## Find process using API port
```powershell
netstat -aon | findstr :7221
```

## Kill conflicting process
```powershell
Stop-Process -Id <PID> -Force
```

## Re-run API
```powershell
cd Api
$env:ASPNETCORE_ENVIRONMENT = "Local"
$env:ASPNETCORE_URLS = "http://localhost:7221"
dotnet run --no-build --no-launch-profile
```

## 8) EF Code-First Operations
## Validate EF guard
```powershell
.\Scripts\qa\Invoke-EfCodeFirstGuard.ps1 -OutputPath .\qa-runtime\reports\ef-guard-latest.md
```

## Add migration
```powershell
dotnet ef migrations add <migration_name> --project Data --startup-project Api --context AppDbContext
```

## Apply migration local
```powershell
dotnet ef database update --project Data --startup-project Api --context AppDbContext
```

## 9) Watcher Operations (Codex-Owned)
## Start
```powershell
.\Scripts\qa\Start-CodexAgentWatch.ps1 -Foreground -NoNotify
```

## Status
```powershell
.\Scripts\qa\Get-CodexAgentWatchStatus.ps1
```

## Stop
```powershell
.\Scripts\qa\Stop-CodexAgentWatch.ps1
```

## Watcher health files
- `qa-runtime/agent-watch/status.json`
- `qa-runtime/agent-watch/heartbeat.txt`
- `qa-runtime/agent-watch/events.log`

## 10) Known Runtime Pitfalls
1. API points to wrong DB instance -> app loads with 500s and “no old audits.”
2. Port collision on `7221` -> hidden startup failure.
3. Frontend compile error can cascade into all E2E runs as false red.
4. Watcher can report stale state when loop throws runtime error.

## 11) Move-PC Validation Checklist (Must Pass)
1. API starts without migration exceptions.
2. Health check endpoints all return 200.
3. Frontend loads audits list without error toasts.
4. At least one focused E2E command runs to completion (pass or legitimate assertion fail, not infra fail).
5. EF guard reports PASS.
6. Watcher status shows active heartbeat and valid state transitions.

