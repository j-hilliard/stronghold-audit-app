# QA-Only Operating Model

## Ownership
- Claude owns application feature code, refactors, and bug fixes.
- QA agent owns testing, validation, defect reporting, evidence capture, and release-readiness decisions.
- QA changes are limited to tests, QA scripts, and QA documentation.

## Required Gates
1. Pre-change baseline gate
- `npm run qa:baseline`
- Includes smoke, visuals, button contract checks, and core E2E.
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
- Shared shell and current module flows are fully covered by button contract tests.
- Audit module button coverage is mandatory as soon as routes/components exist.
- Legacy modules are reference-only unless explicitly added to strict coverage scope.

## Logging and DB Validation
- Live gate checks run only when `PW_LIVE_GATE=true`.
- SQL validation script for process logs:
  - `Scripts/qa/Verify-ProcessLogs.sql`
- No schema/migration changes are accepted without explicit authorization.
- CI also enforces schema authorization using pipeline variable `allowSchemaChange`.

## Evidence Requirements
- Playwright HTML report and JUnit output.
- Traces, videos, screenshots for failures.
- Updated coverage matrix and defect log entries.
- Release-readiness checklist completion.
