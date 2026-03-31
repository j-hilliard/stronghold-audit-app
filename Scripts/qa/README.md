# QA Scripts

## Files
- `Invoke-QAGate.ps1`: Runs baseline/full/PR/pre-merge/pre-release QA gates and stores artifacts.
- `Verify-ProcessLogs.sql`: SQL query to validate logging records for live-gate verification.

## Typical Usage
```powershell
# Baseline gate (mock-backed UI checks)
.\Scripts\qa\Invoke-QAGate.ps1 -Gate baseline

# Full gate with live integration checks
.\Scripts\qa\Invoke-QAGate.ps1 -Gate full -RequireLive
```

## Output
- All gate logs, Playwright report, traces/screenshots/videos, and test results are saved under:
  - `qa-artifacts/<timestamp>-<gate>/`
