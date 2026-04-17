# FIX NOW Ticket for Claude (2026-04-17)

## Objective
Bring QA back to a trustworthy green state. Claims of “complete” are not accepted until these defects are fixed and the required commands pass.

## Current Evidence
- Baseline run command:
  - `cmd /c npm run qa:baseline`
- Result:
  - Fails in `test:e2e:core` on incident workflow navigation behavior.
- Additional reliability failures:
  - Intermittent Playwright launcher failure on targeted runs:
  - `Error: spawn EPERM`
- Prior composer runtime evidence:
  - Tiptap module export error from composer flows:
  - `@tiptap_extension-text-style ... does not provide an export named 'default'`

## Defect 1 (High): Incident “New/Edit” workflow route mismatch
- Area:
  - Incident management workflow
- Failing test:
  - [incident-form.spec.ts](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/incident-form.spec.ts)
- Symptom:
  - During list workflow, navigation lands on an unexpected incident route (`.../inc-11`) instead of expected route progression used by the test contract.
- Required fix:
  - Make “New Incident” and edit row actions deterministic and consistent with the route contract.
  - Ensure list -> new -> save draft -> list -> edit opens the exact created incident ID.
  - Confirm no stale draft redirect side effects.

## Defect 2 (High): Composer runtime editor import crash
- Area:
  - Report Composer narrative editor
- File:
  - [RichTextEditor.vue](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/src/modules/audit-management/features/reports/components/RichTextEditor.vue)
- Symptom:
  - Runtime module failure for Tiptap text-style import.
- Required fix:
  - Correct import/export usage for installed Tiptap version.
  - Composer must load narrative block without runtime page errors.
  - Composer print/export and persistence contract tests must execute without runtime editor crash.

## Defect 3 (Medium): Chart lifecycle runtime errors on navigation stress
- Area:
  - Reports/dashboard chart cleanup on route changes
- Symptom:
  - Intermittent runtime errors in stress test:
  - `Failed to create chart: can't acquire context`
  - `Canvas is already in use... must be destroyed`
- Required fix:
  - Ensure chart instances are destroyed/disposed on unmount and re-render transitions.
  - Remove console/page errors from repeated Audit nav cycles.

## Defect 4 (Medium): Playwright gate reliability on Windows
- Area:
  - Test execution reliability
- Symptom:
  - Intermittent `spawn EPERM` on targeted Playwright commands.
- Required fix:
  - Stabilize run path so gates are consistently executable on this machine.
  - If root cause is process lifecycle/lock contention, implement deterministic cleanup and rerun strategy in QA scripts.
  - Do not mask failures. Fix runner reliability.

## Required Verification (Must Pass)
Run in `webapp/` unless noted.

1. `cmd /c npm run qa:baseline`
2. `$env:PW_AUDIT_COMPOSER_GATE='true'; cmd /c npx playwright test tests/e2e/audit-report-composer-contract.spec.ts --workers=1`
3. `cmd /c npm run test:e2e:audit:live-guard`
4. `$env:PW_AUDIT_REPORTING_GATE='true'; cmd /c npx playwright test tests/e2e/audit-kpi-reporting-contract.spec.ts --workers=1`

## Acceptance Criteria
- All commands above complete without `spawn EPERM`.
- No runtime `pageerror` or chart reuse errors in live navigation stress.
- Composer narrative block renders and can be edited/persisted/reloaded.
- Incident workflow route behavior matches test expectations end-to-end.
- No test weakening without QA approval.

## Delivery Format Required from Claude
- List of changed files.
- Root cause per defect.
- Before vs after behavior summary.
- Exact command outputs for verification commands.
- Any residual risks.
