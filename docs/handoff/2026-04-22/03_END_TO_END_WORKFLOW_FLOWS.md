# 03 - End-to-End Workflow Flows
**Goal:** One place to understand how data and responsibilities move through the system.

## 1) Template Administration Flow
1. Admin opens Template Manager (`/audit-management/admin/templates`)
2. Selects template version (or clones active)
3. Adds/edits/removes sections and questions
4. Configures behaviors:
   - optional section flags
   - response rules
   - scoring flags/weights
   - photo/CA trigger settings
5. Reorders via drag/drop
6. Publishes draft version
7. New audits bind to published version; old audits retain original snapshot context

## Data path
- UI -> `AuditController` admin endpoints -> `Api/Domain/Audit/Admin/*` -> `AppDbContext` (`audit.*` tables)

---

## 2) Audit Creation and Completion Flow
1. User opens New Audit (`/audit-management/audits/new`)
2. Chooses division/template context
3. Optional sections selected (when applicable)
4. Audit draft is created
5. Auditor answers questions and saves responses
6. Response snapshots persist text/category/order and scoring snapshots
7. Auditor submits

## Submit side effects
- status transitions to `Submitted`
- findings generated from non-conforming responses
- auto-create corrective actions (where enabled)
- summary/email paths invoked

## Data path
- UI form -> `SaveAuditResponses`, `SubmitAudit`
- `Audit`, `AuditHeader`, `AuditResponse`, `AuditFinding`, `CorrectiveAction`

---

## 3) Audit Review and Distribution Flow
1. Reviewer opens audit review (`/audit-management/audits/:id/review`)
2. Reviews score/finding details
3. Assigns/updates corrective actions
4. Manages distribution recipients
5. Uses distribution preview to inspect subject/body/recipients
6. Sends distribution email
7. Closes audit when conditions are met

## Closure rule
- Audit close is blocked when open/in-progress corrective actions exist.

## Data path
- UI review -> `GetAuditReview`, `GetDistributionPreview`, `SendDistributionEmail`, `CloseAudit`
- `AuditDistributionRecipient`, `CorrectiveAction`, `AuditActionLog`/`AuditTrailLog`

---

## 4) Corrective Action Lifecycle Flow
1. CA created (auto or manual)
2. Assignee set (with autocomplete where configured)
3. Root cause and notes captured
4. Due date and priority managed
5. Closure evidence/photo captured if required
6. CA closed or voided (void on audit delete path)
7. Reminder notifications fire (due-soon/due-today/overdue)

## Data path
- UI -> CA handlers (`Assign`, `Update`, `Close`, `BulkUpdate`)
- `CorrectiveAction`, `CorrectiveActionPhoto`, `CaNotificationLog`

---

## 5) Reporting + Dashboard Flow
1. User opens Reports (`/audit-management/reports`)
2. Applies division/status/date/section filters
3. KPI cards and charts reflect filtered aggregates
4. Drilldowns route to detail tables and section-level analyses
5. Exports generated for operational reporting

## Data path
- UI filters -> reporting handlers (`GetAuditReport`, `GetSectionTrends`, `GetComplianceStatus`, exports)
- aggregated reads from `Audit`, `AuditResponse`, `CorrectiveAction`

---

## 6) Report Composer + Newsletter Flow
1. User opens Composer (`/audit-management/reports/composer`)
2. Creates report draft
3. Adds blocks (cover, heading, KPI, chart, narrative, callout, CA table, image, rows)
4. Styles and rich text edits applied
5. Saves draft; can reload/manage drafts
6. Prints/exports using print root path
7. Newsletter templates and send routes support distribution workflows

## Data path
- UI -> `ReportDraft` CRUD endpoints
- newsletter endpoints for templates/summary/send

---

## 7) Admin Governance Flow
1. Admin settings tabs:
   - Email Routing
   - Score Targets
   - User Roles
2. User admin:
   - create users
   - edit user profile
   - assign/remove roles
3. Audit log viewer:
   - action log
   - field-level audit trail

---

## 8) Role + Scope Flow
1. Request enters API
2. `AuthorizationBehavior` checks `[AllowedAuthorizationRole]`
3. User context hydrated with division scope
4. Handlers apply scope filters for data access
5. Frontend route/nav gating follows role flags

---

## 9) Environment + Migration Flow
## Local
- `Database.Migrate()` runs at startup
- local seeders execute

## Development/Production
- migrations are expected from pipeline
- runtime seeding only (no auto-migrate in shared env)

---

## 10) Failure Modes To Watch
1. API down or wrong DB -> UI 500 toast cascade (`divisions`, `audits`, `templates`, `report`).
2. Port 7221 occupied by wrong process -> API appears “up” but endpoints fail.
3. Frontend compile warnings/errors (for example duplicate class members) can poison all gate results.
4. Watcher loop runtime error can leave stale status while no healthy auto-cycle is running.

