# QA Scripts

## Files
- `Invoke-QAGate.ps1`: Runs baseline/full/PR/pre-merge/pre-release QA gates and stores artifacts.
- `Invoke-CodexAgentCycle.ps1`: Runs Codex-owned test/improvement cycle based on changed file scope.
- `Invoke-EfCodeFirstGuard.ps1`: EF Core code-first guard checks and report generation.
- `Start-CodexAgentWatch.ps1`: Starts watcher that auto-runs Codex cycle on file changes (foreground by default; use `-Background` for detached mode).
- `Run-CodexAgentWatch.ps1`: Worker loop for watcher process.
- `Stop-CodexAgentWatch.ps1`: Stops background watcher process.
- `Get-CodexAgentWatchStatus.ps1`: Shows watcher liveness + current state/heartbeat.
- `Verify-ProcessLogs.sql`: SQL query to validate logging records for live-gate verification.

## Codex Watcher Status Files
- `qa-runtime/agent-watch/status.json`: current watcher state (`WATCHING`, `CHANGE_DETECTED`, `CYCLE_RUNNING`, `CYCLE_COMPLETE`, `WATCHER_ERROR`) plus live `step`, `changedCount`, `cycleOverall`, and `summaryPath`.
- `qa-runtime/agent-watch/heartbeat.txt`: last watcher heartbeat timestamp.
- `qa-runtime/agent-watch/events.log`: timeline log of state transitions.
- `qa-runtime/agent-watch/logs/watcher-*.log`: detailed watcher + cycle output.

## Strict Auto-Cycle Behavior (Default)
- Watcher defaults are aggressive: `PollSeconds=1`, `DebounceSeconds=3`.
- Strict mode is enabled by default.
- On relevant frontend changes, strict mode runs:
  - `webapp` build (first, fail-fast)
  - `qa:baseline`
  - `test:e2e:audit:corrective-actions`
  - `test:e2e:audit:phase2c`
  - `test:e2e:audit:template-gate`
  - `test:e2e:audit:parity`
  - `test:e2e:audit:composer-gate`
  - `test:e2e:audit:reporting-gate`
- On relevant backend/data changes, strict mode runs:
  - EF code-first guard
  - backend build
- Every strict cycle also runs an isolation guard that fails if any Codex-owned files reference `.claude/*`.

## Typical Usage
```powershell
# Baseline gate (mock-backed UI checks)
.\Scripts\qa\Invoke-QAGate.ps1 -Gate baseline

# Mock-only baseline (skip live audit API guard intentionally)
$env:PW_REQUIRE_AUDIT_API = 'false'
.\Scripts\qa\Invoke-QAGate.ps1 -Gate baseline

# Full gate with live integration checks
.\Scripts\qa\Invoke-QAGate.ps1 -Gate full -RequireLive

# Full gate plus feature-gated template and reporting contract suites
.\Scripts\qa\Invoke-QAGate.ps1 -Gate full -EnableTemplateGate -EnableReportingGate

# Standalone template admin contract checks
$env:PW_AUDIT_TEMPLATE_GATE = 'true'
cmd /c "npm --prefix webapp run test:e2e:audit:template-gate"

# Standalone KPI/reporting contract checks
$env:PW_AUDIT_REPORTING_GATE = 'true'
$env:PW_AUDIT_COMPOSER_GATE = 'true'
cmd /c "npm --prefix webapp run test:e2e:audit:reporting-gate"

# Standalone composer contract checks
$env:PW_AUDIT_COMPOSER_GATE = 'true'
cmd /c "npm --prefix webapp run test:e2e:audit:composer-gate"

# Run one Codex cycle manually with explicit files
.\Scripts\qa\Invoke-CodexAgentCycle.ps1 -ChangedFiles @(
  'webapp/src/modules/audit-management/features/reports/views/ReportsView.vue',
  'Data/AppDbContext.cs'
) -Reason 'manual-check'

# Run one Codex cycle in non-strict mode
.\Scripts\qa\Invoke-CodexAgentCycle.ps1 -ChangedFiles @('webapp/src/main.ts') -Strict:$false

# Start auto watcher in foreground (default)
.\Scripts\qa\Start-CodexAgentWatch.ps1

# Start auto watcher in foreground without desktop pop-up notifications
.\Scripts\qa\Start-CodexAgentWatch.ps1 -Notify:$false

# Equivalent (preferred explicit switch)
.\Scripts\qa\Start-CodexAgentWatch.ps1 -NoNotify

# Explicit foreground (same as default)
.\Scripts\qa\Start-CodexAgentWatch.ps1 -Foreground

# Start auto watcher in background (detached)
.\Scripts\qa\Start-CodexAgentWatch.ps1 -Background

# Start watcher and force all suites on every detected change cycle
.\Scripts\qa\Start-CodexAgentWatch.ps1 -RunAllSuites

# Start watcher in non-strict mode (rarely needed)
.\Scripts\qa\Start-CodexAgentWatch.ps1 -Strict:$false

# Stop watcher
.\Scripts\qa\Stop-CodexAgentWatch.ps1

# Quick status check
.\Scripts\qa\Get-CodexAgentWatchStatus.ps1
```

## Output
- All gate logs, Playwright report, traces/screenshots/videos, and test results are saved under:
  - `qa-artifacts/<timestamp>-<gate>/`
- Auto-cycle reports are saved under:
  - `qa-runtime/reports/agent-cycles/<timestamp>/`
  - `qa-runtime/reports/ef-guard/`
  - `qa-runtime/agent-watch/logs/`
  - `qa-runtime/agent-watch/status.json`
  - `qa-runtime/agent-watch/heartbeat.txt`
