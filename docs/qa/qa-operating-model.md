# QA-Only Operating Model

## Ownership
- Claude owns application feature code, refactors, and bug fixes.
- QA agent owns testing, validation, defect reporting, evidence capture, and release-readiness decisions.
- QA changes are limited to tests, QA scripts, and QA documentation.

## Required Gates
1. Pre-change baseline gate
- `npm run qa:baseline`
- Includes live audit API guard, smoke, visuals, button contract checks, and core E2E.
- Baseline artifacts are retained for comparison.

2. Post-change PR gate
- `npm run qa:pr`
- Includes full regression suite plus optional live checks (`PW_LIVE_GATE=true`).
- PR fails on any regression.

3. Pre-merge final gate
- `npm run qa:premerge`
- Re-run full suite on latest PR head.

4. Pre-release gate
- `npm run qa:prerelease`
- Re-run full suite against release candidate and compare with approved baseline evidence.

## Test Scope
- Strict scope (required now):
  - Shared shell interactions.
  - Audit module core flows (dashboard, new audit, form navigation stability).
  - Legacy parity checks for baseline behavior.
- Strict scope (required when enabled by feature flag):
  - Template admin drag/drop and versioning (`PW_AUDIT_TEMPLATE_GATE=true`).
  - KPI/reporting and scoped visibility (`PW_AUDIT_REPORTING_GATE=true`).
- Reference scope:
  - Legacy modules outside approved strict list.

## Logging and DB Validation
- Live gate checks run only when `PW_LIVE_GATE=true`.
- Audit live API guard runs by default in baseline. Set `PW_REQUIRE_AUDIT_API=false` only when intentionally running mock-only checks.
- SQL validation script for process logs:
  - `Scripts/qa/Verify-ProcessLogs.sql`
- No schema/migration changes are accepted without explicit authorization.
- CI also enforces schema authorization using pipeline variable `allowSchemaChange`.

## Evidence Requirements
- Playwright HTML report and JUnit output.
- Traces, videos, screenshots for failures.
- Updated coverage matrix and defect log entries.
- Release-readiness checklist completion.

## Required Audit Contract Suites
- `tests/e2e/audit-live-api-guard.spec.ts`
- `tests/e2e/audit-live-blank-screen-guard.spec.ts`
- `tests/e2e/audit-live-navigation-stress.spec.ts`
- `tests/e2e/audit-new-audit.spec.ts`
- `tests/e2e/audit-navigation-stability.spec.ts`
- `tests/e2e/audit-parity.spec.ts`
- `tests/e2e/audit-template-admin-contract.spec.ts` (feature-gated)
- `tests/e2e/audit-kpi-reporting-contract.spec.ts` (feature-gated)

## Required Incident Contract Suite
- `tests/e2e/incident-employee-persistence-api.spec.ts`
