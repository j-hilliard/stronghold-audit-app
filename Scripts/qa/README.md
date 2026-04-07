# QA Scripts

## Files
- `Invoke-QAGate.ps1`: Runs baseline/full/PR/pre-merge/pre-release QA gates and stores artifacts.
- `Verify-ProcessLogs.sql`: SQL query to validate logging records for live-gate verification.

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
cmd /c "npm --prefix webapp run test:e2e:audit:reporting-gate"
```

## Output
- All gate logs, Playwright report, traces/screenshots/videos, and test results are saved under:
  - `qa-artifacts/<timestamp>-<gate>/`
