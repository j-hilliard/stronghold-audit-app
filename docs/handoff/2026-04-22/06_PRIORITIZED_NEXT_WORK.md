# 06 - Prioritized Next Work
**Objective:** What to do next, in execution order, with business-aligned sequencing.

## Priority 0 - Stabilize Execution Environment (Do First)
1. Remove/test-fix compile blockers in frontend client generation area.
2. Fix watcher loop runtime failure so status/heartbeat are trustworthy.
3. Confirm API/db/port startup consistency on new PC.
4. Produce one clean QA cycle report with deterministic outputs.

## Definition of done
- one reproducible baseline run with no infra-induced false negatives
- watcher status transitions valid (`WATCHING` -> `CHANGE_DETECTED` -> `CYCLE_RUNNING` -> `CYCLE_COMPLETE`)

---

## Priority 1 - Phase2C Contract Closure
Focus on features already coded but needing proof-level verification:
1. Distribution preview modal and send path
2. Submit email recipient expansion (including audit admins)
3. Admin user edit flow
4. Corrective action export honoring all active filters
5. Close-audit gate when CAs remain open
6. Root cause field persistence
7. Assignee autocomplete behavior

## Definition of done
- all relevant specs green in `test:e2e:audit:phase2c`
- direct DB/API spot checks confirm saved values and filter parity

---

## Priority 2 - Dashboard Usability and Simplification
User concern: dashboard remains overwhelming and too scroll-heavy.

## Work package
1. Reduce top-load visual complexity
2. Clarify filter context banner and denominator language
3. Make KPI drilldowns explicitly label filtered context
4. Normalize card sizing and spacing
5. Add “top N + expand” behavior for dense section grids

## Definition of done
- before/after screenshot comparisons for dashboard states
- task-based usability walkthrough passes (no ambiguity in context)

---

## Priority 3 - Report Composer UX Upgrade
User goal: richer layout customization, faster authoring, less friction.

## Work package
1. persistent side rails and scroll-safe editing
2. stronger page-template controls
3. reliable print/export graph rendering
4. rich text reliability (save/reload/print)
5. better discoverability for blocks, themes, and layout actions

## Definition of done
- composer gate green
- print output parity checklist passes
- no blank chart exports in print preview

---

## Priority 4 - Role/Scope Hardening Pass
1. validate each role view and action capability
2. verify scope-restricted users cannot access out-of-scope data via URL/API
3. produce role x endpoint matrix evidence

## Definition of done
- route-guard and role-switcher specs green
- manual security spot checks documented

---

## Priority 5 - Reporting and Export Trust
1. verify KPI math and labeling under all filters
2. verify export content exactly matches active filter criteria
3. validate recurring report generation quality

## Definition of done
- export parity checklist completed
- user-accepted sample outputs for demo-ready reports

---

## Deferred Last - E-Charts Integration
Per current business decision:
- Do not schedule E-Charts implementation now.
- Keep as final integration phase once access/contract is available.

## Pre-work allowed now
- keep interface contract placeholders
- define data model extension points
- prepare test harness scaffolding (without live API dependency)

---

## Decision Points Requiring User Approval
1. Dashboard redesign depth:
   - incremental polish vs structural tab split
2. Composer architecture:
   - continue block-first improvements vs page-template refactor
3. Notification behavior:
   - strict send now vs dry-run/redirect in local demo mode
4. QA strictness:
   - fail fast on any visual diff vs curated approval workflow

---

## Suggested 7-Day Tactical Plan
1. Day 1: new-PC bootstrap + environment stabilization
2. Day 2: Phase2C contract failures and fixes
3. Day 3: role/scope verification sweep
4. Day 4: dashboard simplification iteration 1 + visual proof
5. Day 5: composer print and UX hardening + visual proof
6. Day 6: reporting/export parity verification
7. Day 7: demo-readiness regression pack and defect burn-down

