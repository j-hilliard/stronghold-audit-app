# Claude Ticket Pack — Reporting + Report Composer (Granular)

## Purpose
This is the execution ticket pack for making Reports and Report Composer production-grade, highly customizable, and QA-enforced.

Audience:
- Claude: implement feature code + test code + route wiring.
- QA agent: validate gates, evidence, and defect closure.

## Non-Negotiables
1. Do not touch production/shared databases.
2. Keep enterprise shell structure intact.
3. No skipped reporting/composer contract tests for delivered features.
4. Every ticket must ship with:
- app code changes,
- Playwright coverage updates,
- QA evidence artifacts.

## Current QA Baseline (as of 2026-04-07)
1. `npm run test:e2e:audit:core` -> passing.
2. `npm run test:e2e:audit:live-guard` -> passing in latest run.
3. `npm run test:e2e:audit:reporting-gate` -> only newsletter runs; reporting/composer contracts are skipped unless gate flags enabled.
4. Forced gates (`PW_AUDIT_REPORTING_GATE=true`, `PW_AUDIT_COMPOSER_GATE=true`) currently fail due to selector/contract mismatch.

## Delivery Sequence
Execute in this order. Do not reorder without QA sign-off.

---

## TICKET RPT-001 — Unblock Reporting/Composer Contract Gates
Priority: `P0`

### Goal
Make reporting/composer contract suites deterministic and executable in CI with no false pass via skips.

### Files
- `webapp/src/modules/audit-management/features/reports/views/ReportsView.vue`
- `webapp/src/modules/audit-management/features/reports/views/ReportComposerView.vue`
- `webapp/src/modules/audit-management/features/reports/components/*.vue`
- `webapp/tests/e2e/audit-kpi-reporting-contract.spec.ts`
- `webapp/tests/e2e/audit-report-composer-contract.spec.ts`
- `webapp/package.json`

### Required Implementation
1. Add stable `data-testid` hooks to real interactive controls:
- `report-filter-division`
- `report-filter-status`
- `report-filter-from`
- `report-filter-to`
- `report-filter-compare`
- `report-kpi-total-audits`
- `report-kpi-nc-count`
- `report-kpi-warning-count`
- `report-kpi-overdue-corrective-actions`
- `report-section-card-<slug>`
- `report-section-clear`
- `report-grid-row`
- `composer-filter-division`
- `composer-filter-from`
- `composer-filter-to`
- `composer-draft-title`
- `composer-draft-select`
- `composer-generate`
- `composer-save`
- `composer-print`
- `composer-toolbar-rail`
- `composer-properties-rail`

2. Align tests with actual control types:
- PrimeVue Dropdowns must be clicked + option selected, not `fill()` on container `div`.
- Native inputs can use `fill()`.

3. Remove skip loophole for delivered features:
- Either hard-enable these suites in a strict script,
- or gate by explicit feature flag that is set to `true` in CI for this module.

### Acceptance Criteria
1. `PW_AUDIT_REPORTING_GATE=true npx playwright test tests/e2e/audit-kpi-reporting-contract.spec.ts` passes.
2. `PW_AUDIT_COMPOSER_GATE=true npx playwright test tests/e2e/audit-report-composer-contract.spec.ts` passes.
3. No skip for implemented contracts.

### Evidence Required
1. Playwright report bundle.
2. Test output showing pass counts and zero skips for enabled suites.

---

## TICKET RPT-002 — Persistent Left/Right Rails (Sticky Composer UX)
Priority: `P0`

### Goal
Toolbar (left) and property panel (right) stay visible while center canvas scrolls.

### Files
- `webapp/src/modules/audit-management/features/reports/views/ReportComposerView.vue`
- `webapp/src/modules/audit-management/features/reports/components/ComposerToolbar.vue`
- `webapp/src/modules/audit-management/features/reports/components/ComposerCanvas.vue`
- `webapp/src/modules/audit-management/features/reports/components/ComposerPropertyPanel.vue`
- `webapp/tests/e2e/audit-report-composer-contract.spec.ts`

### Required Implementation
1. Convert composer layout to sticky rails:
- top offset below header,
- independent `overflow-y-auto` inside each rail,
- canvas scroll independent from rails.
2. Add `data-testid="composer-toolbar-rail"` and `data-testid="composer-properties-rail"`.
3. Ensure keyboard focus remains reachable for controls while canvas is long.
4. Mobile/tablet fallback:
- rails may collapse, but desktop sticky behavior is mandatory.

### Acceptance Criteria
1. On desktop viewport, scrolling the canvas does not hide left/right rails.
2. User can modify selected block style without scrolling back to top.
3. No layout overlap/jitter in sticky state.

### Evidence Required
1. Before/after screenshots on long canvas.
2. Playwright test demonstrating sticky rails during scroll.

---

## TICKET RPT-003 — Section KPI Drilldown (True Report Focus Mode)
Priority: `P0`

### Goal
Each section KPI card acts as a true drilldown entry point across report outputs.

### Files
- `webapp/src/modules/audit-management/features/reports/views/ReportsView.vue`
- `webapp/src/apiclient/auditClient.ts`
- `Api/Controllers/AuditController.cs` (if contract update needed)
- `Api/Domain/Audit/Audits/GetAuditReport.cs` (if query semantics need adjustment)
- `webapp/tests/e2e/audit-kpi-reporting-contract.spec.ts`

### Required Implementation
1. Clicking section KPI sets active section focus.
2. Persist focus in URL query (`?section=<name>`).
3. On page load/back/refresh, restore section focus from URL.
4. Show active chip and clear action.
5. Apply section focus consistently to:
- KPI context,
- section chart,
- detail table rows,
- corrective action table (where relevant and traceable).

### Acceptance Criteria
1. All section cards are keyboard and mouse actionable.
2. Active filter visibly applied and clearable.
3. Data in all dependent widgets reflects selected section focus.
4. Clearing section returns global view.

### Evidence Required
1. Video trace: click section -> view narrows -> refresh -> state restored.
2. Assertions in reporting contract suite for focused vs unfocused counts.

---

## TICKET RPT-004 — Equal KPI Card Sizing + Dense Responsive Grid
Priority: `P0`

### Goal
Section KPI cards are visually consistent (same height/structure) across rows and breakpoints.

### Files
- `webapp/src/modules/audit-management/features/reports/views/ReportsView.vue`
- `webapp/tests/e2e/visual.spec.ts`

### Required Implementation
1. Replace fixed breakpoint grid with robust auto-fill/minmax strategy.
2. Enforce uniform card height (`min-height` + consistent content slots).
3. Clamp long titles to avoid stretching cards.
4. Keep value alignment consistent (headline, unit, totals).

### Acceptance Criteria
1. No “short/long” mixed card heights in section KPI grid at:
- 1366x768,
- 1280x720,
- 1024x768.
2. Visual snapshots approved.

### Evidence Required
1. Updated visual snapshots and diff approval.
2. Screenshot set for three viewport widths.

---

## TICKET RPT-005 — Rich Text Editing for Narrative/Commentary
Priority: `P0`

### Goal
Narrative and chart commentary support full rich text controls and preserve formatting end-to-end.

### Files
- `webapp/src/modules/audit-management/features/reports/components/blocks/NarrativeBlock.vue`
- `webapp/src/modules/audit-management/features/reports/components/ComposerPropertyPanel.vue`
- `webapp/src/modules/audit-management/features/reports/types/report-block.ts`
- `webapp/src/modules/audit-management/features/reports/composables/useReportDraft.ts`
- `webapp/src/modules/audit-management/features/reports/composables/useReportEngine.ts`
- `webapp/tests/e2e/audit-report-composer-contract.spec.ts`

### Required Implementation
1. Add editor support for:
- font size,
- text color,
- bold,
- italic,
- underline,
- bullet list,
- numbered list.
2. Add testids:
- `block-style-font-size`
- `block-style-text-color`
- `block-style-bold`
- `block-style-italic`
- `block-style-underline`
- `block-style-bulleted-list`
- `block-style-numbered-list`
- `block-caption-input`
- `block-narrative-text`
3. Persist formatting in draft payload.
4. Preserve formatting after:
- save/reload,
- regenerate data,
- print preview.
5. Sanitize rich text payload before render.

### Acceptance Criteria
1. Formatting survives save/load and regenerate.
2. Rich text renders correctly on canvas and print output.
3. No unsafe HTML execution.

### Evidence Required
1. Contract test with formatting apply -> save -> reload -> assert HTML/serialization preserved.
2. Print snapshot evidence.

---

## TICKET RPT-006 — Layout Customization Engine (Page Zones)
Priority: `P1`

### Goal
Allow custom page structure beyond vertical block stacks.

### Files
- `webapp/src/modules/audit-management/features/reports/types/report-block.ts`
- `webapp/src/modules/audit-management/features/reports/views/ReportComposerView.vue`
- `webapp/src/modules/audit-management/features/reports/components/ComposerCanvas.vue`
- `webapp/src/modules/audit-management/features/reports/composables/useReportDraft.ts`
- `Api/Models/Audit/ReportDraftDto.cs` (if schema extension needed)
- `Data/Models/Audit/ReportDraft.cs` (if schema extension needed)

### Required Implementation
1. Introduce layout schema:
- pages,
- zones (left/main/right/header/footer),
- block placement metadata (`pageId`, `zoneId`, coordinates/order/size).
2. Add page layout presets:
- single-column,
- two-column equal,
- asymmetric left-rail,
- executive cover.
3. Enable per-page overrides.
4. Persist and restore layout schema with drafts.

### Acceptance Criteria
1. User can create page 1 with a left-side section and main content region.
2. Layout persists after save/reload.
3. Existing drafts without layout schema migrate safely.

### Evidence Required
1. Demo JSON payload of saved layout.
2. E2E create-custom-layout -> save -> reload -> verify region placement.

---

## TICKET RPT-007 — Drag/Resize Placement in Composer
Priority: `P1`

### Goal
Users can drag and resize blocks within zones for custom composition.

### Files
- `webapp/src/modules/audit-management/features/reports/components/ComposerCanvas.vue`
- chosen layout library integration files
- `webapp/tests/e2e/audit-report-composer-contract.spec.ts`

### Required Implementation
1. Implement DnD + resize interactions inside page zones.
2. Add snap-to-grid behavior.
3. Add undo/redo for layout operations.
4. Add reset-to-template option.

### Acceptance Criteria
1. Drag and resize are stable and persisted.
2. Undo/redo works for move/resize.
3. No overlap clipping in normal viewport.

### Evidence Required
1. Playwright trace for drag/resize persistence.
2. Screen capture of undo/redo workflow.

---

## TICKET RPT-008 — Print-Safe Composition + Page Break Controls
Priority: `P1`

### Goal
Generated PDF/print output respects custom layout and readability constraints.

### Files
- `webapp/src/modules/audit-management/features/reports/views/ReportComposerView.vue`
- `webapp/src/modules/audit-management/features/reports/components/blocks/*.vue`
- print stylesheet files
- `webapp/tests/e2e/audit-report-composer-contract.spec.ts`

### Required Implementation
1. Add print-safe boundaries and overflow checks.
2. Add block-level page break preferences:
- allow split,
- keep together,
- start new page.
3. Validate rich text rendering in print.

### Acceptance Criteria
1. No clipped text/charts in print output.
2. Page-break rules honored.
3. Branded output remains readable and consistent.

### Evidence Required
1. Print PDF samples for 2+ presets.
2. QA sign-off snapshots.

---

## TICKET RPT-009 — Role/Scope-Aware Report Views
Priority: `P1`

### Goal
Users only see report data they are authorized to see (division/site/company scope).

### Files
- `Api/Domain/Audit/Audits/GetAuditReport.cs`
- `Api/Domain/Audit/Audits/GetSectionTrends.cs`
- access scope services/entities
- `webapp/src/modules/audit-management/features/reports/views/ReportsView.vue`
- relevant E2E tests

### Required Implementation
1. Enforce role + scope at query layer.
2. Hide or disable out-of-scope filter options on UI.
3. Prevent direct URL or API bypass of scope.

### Acceptance Criteria
1. Out-of-scope records never returned.
2. Scoped users only see allowed divisions/sites.
3. Contract tests include negative access checks.

### Evidence Required
1. API test proving scope restriction.
2. UI test proving filter list scoping.

---

## TICKET RPT-010 — Saved Views + Dashboard Personalization
Priority: `P2`

### Goal
Users can save reusable report views and quickly switch context.

### Files
- reports view/store files
- report draft or report view entity models
- tests and docs

### Required Implementation
1. Save named filter/view presets:
- division/site/date/status/compare/section/chart type.
2. Set default view per user role if configured.
3. Quick apply from preset menu.

### Acceptance Criteria
1. Saved views restore full report state.
2. Presets respect role/scope constraints.

### Evidence Required
1. E2E save view -> reload -> apply -> assert.

---

## QA Gate Commands (Required Per Milestone)
Run from `webapp`:

1. Core stability:
`npm run test:e2e:audit:core`

2. Live stability:
`npm run test:e2e:audit:live-guard`

3. Reporting/composer strict contracts:
`$env:PW_AUDIT_REPORTING_GATE='true'; $env:PW_AUDIT_COMPOSER_GATE='true'; npx playwright test tests/e2e/audit-kpi-reporting-contract.spec.ts tests/e2e/audit-report-composer-contract.spec.ts`

4. Reporting gate aggregate:
`npm run test:e2e:audit:reporting-gate`

5. If baseline requested:
`npm run qa:baseline`

## Required QA Artifacts Per Ticket
1. Playwright HTML report.
2. Traces/videos/screenshots for failures and final pass.
3. Defect entry updates in `docs/qa/defect-log.md`.
4. Button/interaction updates in `docs/qa/button-coverage-matrix.md`.
5. QA impact notes in `docs/qa/migration-qa-impact-log.md` if schema changed.

## Done Definition (Per Ticket)
1. Code implemented.
2. Tests updated and passing.
3. No unresolved P0 regressions introduced.
4. Evidence attached.
5. Ticket summary posted with:
- changed files,
- behavior delivered,
- risks/assumptions,
- exact command outputs.

