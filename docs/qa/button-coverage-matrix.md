# Button Coverage Matrix

## Scope
- Strict gate scope (active): Shared shell + incident module + audit core routes.
- Strict gate scope (feature-gated):
  - Template admin controls: enabled with `PW_AUDIT_TEMPLATE_GATE=true`
  - KPI/reporting controls: enabled with `PW_AUDIT_REPORTING_GATE=true`

| Coverage ID | Scope | Route | UI Element | Expected Behavior | Test Case | Gate |
|---|---|---|---|---|---|---|
| shared-shell-menu-button | Shared shell | `/incident-management/incidents` | Menu button | Clickable; shell nav remains available | `button-contract.spec.ts` | Active |
| shared-shell-profile-menu-button | Shared shell | `/incident-management/incidents` | Profile button | Opens profile menu with logout action | `button-contract.spec.ts` | Active |
| shared-shell-logo-home-link | Shared shell | `/incident-management/incidents` | Header logo | Navigates to dashboard | `button-contract.spec.ts` | Active |
| incident-list-new-incident-button | Incident mgmt | `/incident-management/incidents` | New Incident | Navigates to new incident form | `button-contract.spec.ts` | Active |
| incident-list-search-button | Incident mgmt | `/incident-management/incidents` | Search icon button | Executes list filtering | `button-contract.spec.ts` | Active |
| incident-form-cancel-button | Incident mgmt | `/incident-management/incidents/new` | Cancel | Returns to list without submit | `button-contract.spec.ts` | Active |
| incident-form-save-draft-button | Incident mgmt | `/incident-management/incidents/new` | Save Draft | Persists draft and enters edit mode | `button-contract.spec.ts` | Active |
| incident-form-submit-button | Incident mgmt | `/incident-management/incidents/new` | Submit | Saves and returns to list | `button-contract.spec.ts` | Active |
| reference-table-add-company-button | Incident mgmt | `/incident-management/ref-tables` | Add Company | Opens create dialog | `button-contract.spec.ts` | Active |
| reference-table-regions-tab | Incident mgmt | `/incident-management/ref-tables` | Regions tab | Switches tab content | `button-contract.spec.ts` | Active |
| reference-table-lookup-tab | Incident mgmt | `/incident-management/ref-tables` | Lookup Tables tab | Switches tab content | `button-contract.spec.ts` | Active |
| audit-dashboard-new-audit-button | Audit mgmt | `/audit-management/audits` | New Audit | Routes to new audit page | `audit-navigation-stability.spec.ts` | Active |
| audit-new-audit-division-dropdown | Audit mgmt | `/audit-management/audits/new` | Division dropdown | Selects one division; selected division drives required job prefix / site code fields | `audit-new-audit.spec.ts` | Active |
| audit-new-audit-start-button | Audit mgmt | `/audit-management/audits/new` | Start Audit | Starts draft and routes to `/audits/:id` | `audit-new-audit.spec.ts` | Active |
| audit-new-audit-cancel-button | Audit mgmt | `/audit-management/audits/new` | Cancel | Routes back to audit dashboard | `audit-new-audit.spec.ts` | Active |
| audit-form-status-buttons | Audit mgmt | `/audit-management/audits/:id` | C/NC/W/NA | Applies status and updates summary | `audit-parity.spec.ts` | Active |
| audit-form-collapse-expand | Audit mgmt | `/audit-management/audits/:id` | Collapse All / Expand All | Toggles section visibility | `audit-parity.spec.ts` | Active |
| audit-form-save-draft | Audit mgmt | `/audit-management/audits/:id` | Save Draft | Sends save request and persists edits | `audit-parity.spec.ts` | Active |
| audit-sidebar-multi-click-stability | Audit mgmt | `/audit-management/*` | Sidebar links | Repeated clicks keep pages rendered and interactive | `audit-navigation-stability.spec.ts` | Active |
| audit-live-api-divisions-health | Audit mgmt | `API /v1/divisions` | Live endpoint | Returns `200` JSON (not HTML, not 5xx) | `audit-live-api-guard.spec.ts` | Active |
| audit-live-api-audits-health | Audit mgmt | `API /v1/audits` | Live endpoint | Returns `200` JSON (not HTML, not 5xx) | `audit-live-api-guard.spec.ts` | Active |
| audit-live-no-load-failure-toast | Audit mgmt | `/audit-management/audits/new` | Load failure toast | No `Failed to load divisions/audits` toast in healthy environment | `audit-live-blank-screen-guard.spec.ts` | Active |
| audit-live-nav-stress-no-blank | Audit mgmt | `/audit-management/*` | Repeated sidebar navigation | No blank screens, no failed-load toasts, no API 5xx during 5 cycles | `audit-live-navigation-stress.spec.ts` | Active |
| template-admin-create-version | Template admin | `/audit-management/admin/templates` | Create New Version | Clones active version into editable draft | `audit-template-admin-contract.spec.ts` | Feature-gated |
| template-admin-add-question | Template admin | `/audit-management/admin/templates` | Add Question | Adds new question to draft section | `audit-template-admin-contract.spec.ts` | Feature-gated |
| template-admin-drag-drop-question | Template admin | `/audit-management/admin/templates` | Question drag handle | Reorders questions and persists order | `audit-template-admin-contract.spec.ts` | Feature-gated |
| template-admin-publish-version | Template admin | `/audit-management/admin/templates` | Publish Version | Promotes draft to active and locks content | `audit-template-admin-contract.spec.ts` | Feature-gated |
| reports-apply-filters | Reporting | `/audit-management/reports` | Apply Filters | Filters KPI/cards/table by scope and date | `audit-kpi-reporting-contract.spec.ts` | Feature-gated |
| reports-download-export | Reporting | `/audit-management/reports` | Export | Exports selected report payload | `audit-kpi-reporting-contract.spec.ts` | Feature-gated |
| reports-view-audit-action | Reporting | `/audit-management/reports` | View Audit | Opens selected audit detail route | `audit-kpi-reporting-contract.spec.ts` | Feature-gated |

## Pending Full-Stack Audit Coverage Expansion
- `AUDIT-20260427-FULLSTACK-BENCHMARK` must add or update rows for dashboard action clusters, audit-list filters, corrective-action bulk controls, template draft/edit/publish controls, reports menu/dropdowns, newsletter generation/template-editor actions, report composer draft/bulk delete/print/export controls, blank-form print dialog controls, and route-guard-visible navigation controls.
- Candidate UI items U-001 through U-008 need screenshot-backed coverage before being promoted from `VERIFY` to `OPEN` or `ACCEPTED AS-IS`.
- Candidate logic items B-001 through B-015 need matching automated or manual coverage listed in `QA_REGRESSION_CHECKLIST.md` section 15 before closure.

## Update Rule
- Every newly introduced button in strict scope must be added here before merge.
