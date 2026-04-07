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
  - `kpi-total-audits`
  - `kpi-nc-count`
  - `kpi-warning-count`
  - `kpi-overdue-corrective-actions`
  - `report-grid-row`

## QA Contract Suites To Pass
- `tests/e2e/audit-template-admin-contract.spec.ts`
- `tests/e2e/audit-kpi-reporting-contract.spec.ts`
- `tests/e2e/audit-new-audit.spec.ts`
- `tests/e2e/audit-navigation-stability.spec.ts`
- `tests/e2e/audit-parity.spec.ts`

## Delivery Rule
After each milestone Claude must provide:
- files changed,
- endpoints/components delivered,
- risks/assumptions,
- exact test commands QA should run.
