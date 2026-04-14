# Claude Execution Brief (Template Engine + Reporting)

Use this brief when handing implementation work to Claude.

## Hard Constraints
- Keep enterprise shell intact (header/sidebar/theme/auth/page layout).
- Do not hardcode audit questions in Vue files.
- Use template versioning for all checklist changes.
- Keep auditor workflow and admin workflow in separate modules/components.
- Enforce role + scope authorization in API and query layer.

## Build Priorities
1. Template Manager
- Add/edit/archive questions.
- Drag/drop reorder for sections and questions.
- Clone draft version from active version.
- Publish draft to active.

2. Access Control
- Role model: `SystemAdmin`, `TemplateAdmin`, `AuditManager`, `Auditor`, `AuditReviewer`, `CorrectiveActionOwner`, `ReadOnlyViewer`, optional `ExecutiveViewer`.
- Scope model: division/site/company/audit type.
- Secure audit listing/detail by scope.

3. KPI and Reporting
- KPI cards and trend views.
- Filters for division/site/date/auditor/status.
- Recurring NC insights and corrective action aging.
- Report queries must be scope-aware.

4. Report Composer UX Hardening
- Keep left toolbar and right property panel persistently available during canvas scroll (desktop sticky rails).
- Add rich text editing controls for narrative and commentary:
  - font size, text color, bold, italic, underline, bullet/numbered list.
- Preserve formatting across save/load/regenerate and print output.
- Ensure keyboard accessibility for formatting controls and rail actions.

5. Composer Layout Customization
- Implement layout zones per page (not only a single vertical flow).
- Support drag/drop + resize placement of blocks inside zones.
- Support page layout presets plus per-page custom overrides.
- Persist full layout schema in draft storage and restore accurately.
- Enforce print-safe layout constraints and page-break behavior.

## Required Testability Hooks
Claude should include these `data-testid` hooks to support QA contracts:
- Template manager:
  - `question-text-input`
  - `question-row`
  - `section-row`
- Reports:
  - `report-filter-division`
  - `report-filter-site`
  - `report-filter-from`
  - `report-filter-to`
  - `report-filter-compare`
  - `kpi-total-audits`
  - `kpi-nc-count`
  - `kpi-warning-count`
  - `kpi-overdue-corrective-actions`
  - `report-section-card-<slug>`
  - `report-section-clear`
  - `report-grid-row`
- Report Composer:
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
  - `block-caption-input`
  - `block-narrative-text`
  - `block-style-font-size`
  - `block-style-text-color`
  - `block-style-bold`
  - `block-style-italic`
  - `block-style-underline`
  - `block-style-bulleted-list`
  - `block-style-numbered-list`

## QA Contract Suites To Pass
- `tests/e2e/audit-template-admin-contract.spec.ts`
- `tests/e2e/audit-kpi-reporting-contract.spec.ts`
- `tests/e2e/audit-report-composer-contract.spec.ts`
- `tests/e2e/audit-new-audit.spec.ts`
- `tests/e2e/audit-navigation-stability.spec.ts`
- `tests/e2e/audit-parity.spec.ts`
- `tests/e2e/audit-live-navigation-stress.spec.ts`

## Mandatory Acceptance Criteria (Do Not Skip)
1. Sticky Rails
- On desktop, scrolling a long report keeps both side rails visible and usable.
- Toolbar actions and property edits are available without returning to top of page.

2. Rich Text
- User can apply font size, text color, bold/italic/underline, and bullet/numbered list.
- Formatting persists after save/reload and after data regeneration.
- Print/PDF preserves authored formatting.

3. Layout Customization
- User can place blocks into custom page regions (example: page 1 left-side panel).
- Resized/repositioned blocks persist after save/reload.
- Print preview honors custom layout without overlap/clipping.

4. Reporting Interaction
- Section KPI cards are clickable and apply scoped filtering.
- Active section filter is visible and clearable.
- Filter state survives refresh/back navigation.

5. Regression Safety
- No blank-screen navigation regressions.
- No API 5xx during live navigation stress.

## Delivery Rule
After each milestone Claude must provide:
- files changed,
- endpoints/components delivered,
- risks/assumptions,
- exact test commands QA should run.
