# 05 - QA Status and Execution Plan
**Purpose:** Current test posture, known failures, and exact rerun plan on new PC.

## 1) Test Surface Available
### Playwright script coverage (`webapp/package.json`)
- Core:
  - `test:e2e:audit:core`
  - `test:e2e:audit:corrective-actions`
  - `test:e2e:audit:parity`
- Phase2C:
  - `test:e2e:audit:phase2c`
- Template/reporting/composer:
  - `test:e2e:audit:template-gate`
  - `test:e2e:audit:composer-gate`
  - `test:e2e:audit:reporting-gate`
- Live guards:
  - `test:e2e:audit:live-guard`
- Visual:
  - `test:e2e:audit:visual:all`
- Aggregate:
  - `qa:baseline`
  - `qa:full`

## 2) Latest Known QA Signals
## Strong positives
- Composer gate has passed in recent runs.
- Reporting gate has passed in recent runs (with some feature-gated tests skipped by design).
- EF code-first guard latest report: **PASS**.

## Strong negatives
- Auto-cycle watcher logs show repeated cycle failure from frontend compile issue:
  - duplicate class member `createUser` in `webapp/src/apiclient/client.ts`
- Watcher loop runtime error present:
  - `The term 'if' is not recognized ...`
- Result:
  - watcher may appear running while cycle status is stale/unhealthy.

## 3) Why Prior Automatic Cycles Were Unreliable
1. A frontend compile issue poisoned downstream suites.
2. Watcher loop error prevented stable continuous execution.
3. Concurrent code edits during long test runs caused moving-target failures.
4. API/environment instability (port/db mismatch) caused false UI regressions.

## 4) Current QA Artifact Locations
- Watcher state:
  - `qa-runtime/agent-watch/status.json`
  - `qa-runtime/agent-watch/events.log`
  - `qa-runtime/agent-watch/logs/`
- Auto-cycle summaries:
  - `qa-runtime/reports/agent-cycles/<timestamp>/summary.md`
- EF guard output:
  - `qa-runtime/reports/ef-guard-latest.md`
- Playwright outputs:
  - `test-results/`
  - `qa-artifacts/` (when using gate scripts)

## 5) New-PC QA Re-Baselining Plan (Order Matters)
1. **Infra sanity**
   - API 200 checks (`divisions`, `audits`, `admin/templates`)
2. **Compile sanity**
   - `npm --prefix webapp run build`
3. **Fast smoke**
   - `npm --prefix webapp run test:e2e:audit:core`
4. **Critical workflows**
   - `npm --prefix webapp run test:e2e:audit:phase2c`
5. **Reporting/composer confidence**
   - `npm --prefix webapp run test:e2e:audit:composer-gate`
   - `npm --prefix webapp run test:e2e:audit:reporting-gate`
6. **Live safety net**
   - `npm --prefix webapp run test:e2e:audit:live-guard`
7. **Visual sweep**
   - `npm --prefix webapp run test:e2e:audit:visual:all`

## 6) Evidence Requirements Per Run
For each run, save:
1. exact command
2. start/end timestamps
3. pass/fail counts
4. top 5 failing specs
5. screenshots/traces for failing specs
6. API console error snippets (if 500s appear)

## 7) Required Defect Report Structure
When reporting failures, use:
1. **Fixes Needed** (blocking defects, with file + behavior)
2. **Improvements** (usability/performance quality)
3. **UI Adjustments** (layout consistency, clarity, visual hierarchy)
4. **Proof** (test names, screenshot paths, logs)

## 8) Recommended Stabilization Before Heavy Feature Work
1. Fix compile blockers first.
2. Ensure watcher loop no longer errors.
3. Lock app during critical test pass window (avoid concurrent feature edits while running full gates).
4. Re-enable continuous cycles only after deterministic green baseline.

