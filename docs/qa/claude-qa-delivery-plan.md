# Claude + QA Delivery Plan

## Operating Split
- Claude owns application feature code and fixes.
- QA owns test code, validation, defect evidence, and release-readiness decisions.
- QA does not edit application feature code.

## Phase Plan

| Phase | Claude Build Scope | QA Scope | Exit Criteria |
|---|---|---|---|
| 1 | Enterprise shell route scaffolding for audit module pages | Smoke, route visibility, shell integrity checks | All audit routes render in shell with no runtime errors |
| 2 | Audit schema, migrations, seed data | DB validation, migration impact review, seed verification | Schema and seed data verified; no unauthorized schema drift |
| 3 | Audit API + CQRS handlers | API contract checks, auth/scope validation, error contract checks | Required endpoints available and contract-compliant |
| 4 | Audit form engine | Functional parity tests vs legacy form, score logic tests, save/reopen tests | Audits can be created, saved, resumed, and submitted correctly |
| 5 | Review/PDF/email routing | Review workflow E2E, output verification, routing rule checks | Review and distribution workflow reliable end-to-end |
| 6 | Template manager (add/edit/archive/reorder/drag-drop/publish) | Template-admin contract tests and drag-drop reorder checks | Template versioning and reordering validated |
| 7 | KPI dashboard and reports | KPI data-accuracy tests, filter tests, scope-based visibility tests, visual baseline | Reporting outputs accurate and permission-scoped |

## Required QA Suites
- `tests/e2e/audit-live-api-guard.spec.ts`
- `tests/e2e/audit-live-blank-screen-guard.spec.ts`
- `tests/e2e/audit-live-navigation-stress.spec.ts`
- `tests/e2e/audit-new-audit.spec.ts`
- `tests/e2e/audit-navigation-stability.spec.ts`
- `tests/e2e/audit-parity.spec.ts`
- `tests/e2e/audit-template-admin-contract.spec.ts` (feature-gated)
- `tests/e2e/audit-kpi-reporting-contract.spec.ts` (feature-gated)
- `tests/e2e/button-contract.spec.ts`
- `tests/e2e/visual.spec.ts`
- `tests/e2e/live-db-logging.spec.ts` (live gate)

## Feature-Gated Contract Suites
- Enable template-admin contract checks with:
  - `PW_AUDIT_TEMPLATE_GATE=true`
- Enable KPI/reporting contract checks with:
  - `PW_AUDIT_REPORTING_GATE=true`

These suites intentionally stay in the repo before full feature completion so QA can enforce requirements immediately when screens are delivered.

## Defect Escalation Rule
When QA finds a defect:
1. Log in `docs/qa/defect-log.md`.
2. Handoff to Claude using `docs/qa/claude-defect-handoff-template.md`.
3. Include a proposed root cause and acceptance criteria.
4. Re-run impacted QA gates before closure.

## If Claude Gets Blocked
- QA provides:
  - reproduction in deterministic Playwright steps,
  - failing selector/assertion evidence,
  - likely code area to patch,
  - expected post-fix behavior.
- Claude then implements; QA revalidates and closes.
