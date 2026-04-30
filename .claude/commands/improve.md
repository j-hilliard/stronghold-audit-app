---
description: Mandatory enforcement command for full pre-change and post-change QA with screenshots, API health checks, and sub-agent execution. Fails if evidence is missing.
---

# Enforced Improvement Cycle (No Exceptions)

Run this sequence exactly. Do not skip steps. Do not claim completion without evidence.

## 1) Pre-Change Baseline (before first code edit)

- Verify API health (must all be 200):
  - `/v1/divisions`
  - `/v1/audits`
  - `/v1/admin/templates`
  - `/v1/reports/compliance-status`

- Capture baseline screenshots for all audit routes (desktop/tablet/mobile, dark/light).
- Save artifact paths under `.claude/visual-tests/screenshots/before/`.

If any API check fails, STOP and fix runtime before editing code.

## 2) Execute Change

- Apply requested code changes.
- Keep edits scoped and minimal.

## 3) Post-Change Validation

Run all of the following:

- `npm --prefix webapp run qa:baseline`
- `npm --prefix webapp run test:e2e:audit:live-guard`
- `npm --prefix webapp run test:e2e:audit:visual:all`

Then:
- Capture post-change screenshots for all audit routes.
- Save to `.claude/visual-tests/screenshots/after/`.
- Produce diff/failure evidence.

## 4) Sub-Agent Execution Contract

Run these in parallel and include their report paths:
- `tester`
- `ui-agent`
- `db-agent`
- `ef-agent`

Then run `improver` on the combined failures.
Then re-run `tester` + `ui-agent` for verification.

## 5) Required Output (must be included every time)

Provide:
- Exact commands executed.
- Test totals: passed/failed/skipped.
- API health status table.
- Before/after screenshot artifact paths.
- List of defects fixed.
- List of defects remaining (severity + owner).

## 6) Hard Fail Rules

- If API health fails: task is blocked.
- If screenshots are missing: task is not complete.
- If tests were not executed: task is not complete.
- If evidence paths are missing: task is not complete.
