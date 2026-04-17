# Dreamy Plan Testing Gates (QA Enforcement Pack)

Source plan reviewed:
- `C:\Users\joseph.hilliard\.claude\plans\dreamy-popping-sunrise.md`

Date:
- 2026-04-17

## Current Truth (before new work starts)

Baseline is **not green**.

- Command: `cmd /c npm run qa:baseline`
- Result: failed in `test:e2e:core`
- Failing test:
  - `Incident Management workflows › incident list supports search, new, edit, and delete actions`
  - [incident-form.spec.ts:58](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/incident-form.spec.ts:58)
- Error:
  - `Expected pattern: /\/incident-management\/incidents\/new$/`
  - `Received string: "http://127.0.0.1:7308/incident-management/incidents/inc-11"`

No Phase 2+ claim is accepted until baseline is green.

## Non-Negotiable QA Rules

1. UI claim requires real browser validation (Playwright), not unit-only.
2. Data mutation claim requires post-action API/DB assertion.
3. Email/PDF/export claim requires artifact inspection.
4. Security claim requires negative tests (unauthorized, expired/revoked/replay/rate-limit).
5. Scheduled job claim requires timing, retry, idempotency, and logging proof.

## Stage Gate 0 Required Tests

1. .NET 10 build:
- `dotnet --version`
- `dotnet build Api/Api.csproj`

2. Photo/repeat/benchmark regressions:
- Extend and run [audit-parity.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-parity.spec.ts)
- Include persistence on reload and delete verification

3. Composer + dashboard regression:
- `PW_AUDIT_COMPOSER_GATE=true` composer contract
- `PW_AUDIT_REPORTING_GATE=true` KPI/reporting contract

## Phase-to-Test Mapping (minimum)

### 2A Audit Form Intelligence
- Extend:
  - [audit-parity.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-parity.spec.ts)
  - [audit-new-audit.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-new-audit.spec.ts)
- Add server-vs-client score parity fixtures (weighted/NA/warning/life-critical)

### 2B Corrective Actions
- Add:
  - [audit-corrective-actions-contract.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-corrective-actions-contract.spec.ts)
  - [audit-ca-public-link.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-ca-public-link.spec.ts)
- Include token security negatives and rate-limit checks

### 2C Dashboard Intelligence
- Extend:
  - [audit-kpi-reporting-contract.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-kpi-reporting-contract.spec.ts)
- Add:
  - [audit-prior-audit-prefill.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-prior-audit-prefill.spec.ts)
  - [audit-question-history.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-question-history.spec.ts)

### 2D Auto Reports
- Add:
  - [audit-weekly-division-summary.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-weekly-division-summary.spec.ts)
  - [audit-ca-aging-report.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-ca-aging-report.spec.ts)
- Include idempotency/retry/send-log assertions

### 2E Template Governance
- Extend:
  - [audit-template-admin-contract.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-template-admin-contract.spec.ts)
- Include publish immutability, effective date selection, and historical render fidelity

### 2F Reliability/Security/Observability
- Add/extend:
  - [live-db-logging.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/live-db-logging.spec.ts)
  - Optional UI suite if health/flags surface in UI:
    - [audit-observability.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-observability.spec.ts)

## Command Gate Contract (must run per PR)

1. `cmd /c npm run qa:baseline`
2. `cmd /c npm run qa:pr`
3. Feature-gated when relevant:
- `$env:PW_AUDIT_COMPOSER_GATE='true'; cmd /c npx playwright test tests/e2e/audit-report-composer-contract.spec.ts --workers=1`
- `$env:PW_AUDIT_REPORTING_GATE='true'; cmd /c npx playwright test tests/e2e/audit-kpi-reporting-contract.spec.ts --workers=1`
- `$env:PW_AUDIT_TEMPLATE_GATE='true'; cmd /c npx playwright test tests/e2e/audit-template-admin-contract.spec.ts --workers=1`

## Required Artifacts Per Milestone

1. Playwright HTML/JUnit report
2. Trace/video/screenshots for all failures
3. Updated button coverage matrix
4. Updated defect log with open/closed state
5. DB/API evidence for persistence/security claims

## Blocking Policy

No merge if:
- baseline is red
- any stage gate test fails
- runtime console/page errors appear in covered flows
- required artifacts are missing
