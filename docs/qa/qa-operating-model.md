# QA-Only Operating Model

## Ownership
- Claude owns application feature code, refactors, and bug fixes.
- QA agent owns testing, validation, defect reporting, evidence capture, and release-readiness decisions.
- QA changes are limited to tests, QA scripts, and QA documentation.

## Audit-Orchestrator Sweep Protocol
- Read `LIVE_QA_TODO.md`, `QA_REGRESSION_CHECKLIST.md`, audit requirements, defect handoffs, coverage matrix, and evidence docs before starting any sweep.
- Treat `LIVE_QA_TODO.md` as the living scoreboard. Open items require investigation, not automatic fixes.
- Do not change application code unless Joseph explicitly approves a specific fix. Subagents are read-only auditors.
- For full-stack audit sweeps, coordinate exactly 3 read-only subagents in this order:
  1. UI / End-User Agent: screens, flows, layout, visual hierarchy, discoverability, empty/loading/error states, screenshots.
  2. Logic / Workflow Agent: audit form state, scoring, skip logic, submit/reopen/close, corrective actions, reporting math, date windows, template versioning, composer drafts, print/export, route guards.
  3. Code / Architecture Agent: maintainability, duplication, client/API contract drift, brittle state handling, frontend data masking, testability, regression risk.
- Each subagent must read the live TODO first, report relevant item status, collect evidence, and return `OPEN`, `VERIFY`, `BLOCKED`, or `ACCEPTED AS-IS` findings. Unproven candidate issues stay `VERIFY`.
- Every confirmed finding must include what is wrong, why it matters, user/business impact, likely root cause, likely files, recommended fix, regression test needed, and evidence path.
- Full-stack benchmark sweeps require current web research on compliance audit / inspection / EHS / field-audit products, but competitor features are recommendations until Stronghold evidence proves a gap.
- Every sweep must update the live TODO, regression checklist, and evidence packet. Screenshots belong under `docs/qa-evidence/<sweep-id>/`; the markdown handoff belongs under `docs/`.
- Live screenshot sweeps must prove the API is healthy before capture. If `GET /v1/divisions` fails on the frontend-configured API base, stop and fix the environment first; do not collect or report mock screenshots as live evidence.
- The evidence packet must record the API base URL, screenshot count, Playwright result, and any console errors captured during the sweep.

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
