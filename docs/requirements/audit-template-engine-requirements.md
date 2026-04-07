# Audit Template Engine Requirements

## Purpose
Build the Compliance Audit product as a versioned template engine, not a hardcoded form.

This document is the implementation baseline for:
- template management (business-controlled),
- audit execution (auditor-controlled),
- reporting and KPI stability (analytics-controlled),
- access control (role + scope),
- QA validation and release gates.

## Mandatory Product Requirements

### R-001 Template Engine
- The system shall manage audits through templates and template versions.
- The system shall prevent direct editing of published versions.
- The system shall require cloning/publishing workflow for any live checklist changes.

### R-002 Section and Question Management
- The system shall store sections and questions as database data (not Vue constants).
- Admin users shall be able to add, edit, archive, and reorder sections/questions in draft versions.
- The system shall support drag-and-drop question and section ordering in Template Manager.

### R-003 Stable Reporting Model
- The system shall separate display layout from reporting taxonomy:
  - Display Section: what auditors see.
  - Reporting Category: how analytics roll up.
- Renaming or moving display sections shall not break historical trend reporting.

### R-004 Audit Version Lock
- New audits shall bind to the currently active template version at creation time.
- Existing audits shall remain tied to their original template version even after new versions are published.

### R-005 Response and Rule Behavior
- Each question shall support:
  - status response sets (`Conforming`, `Non-Conforming`, `Warning`, `N/A`),
  - required/optional behavior,
  - conditional comment requirements,
  - corrected-on-site behavior,
  - corrective action trigger behavior,
  - scoreable/non-scoreable configuration,
  - applicability rules.
- Non-conforming behavior from the legacy tool must be preserved.

### R-006 Snapshot Integrity
- Completed audits shall store response snapshots that preserve:
  - question text,
  - section label,
  - reporting category,
  - sort/order context.
- Snapshot integrity is required for defensibility and historical reporting.

### R-007 Role and Scope Security
- Access shall be enforced with role + scope, not role-only.
- Supported scope dimensions shall include at minimum:
  - Division,
  - Site,
  - Company/Business unit,
  - Audit type.
- Users must only see audits within their allowed scopes.

### R-008 Role Set
- Required roles:
  - `SystemAdmin`
  - `TemplateAdmin`
  - `AuditManager`
  - `Auditor`
  - `AuditReviewer`
  - `CorrectiveActionOwner`
  - `ReadOnlyViewer`
  - `ExecutiveViewer` (optional but recommended)

### R-009 Reporting and KPI Dashboard
- The Reports module shall provide:
  - KPI summary cards,
  - filters by division/site/date/auditor/status,
  - trends by reporting category,
  - repeated non-conformance analysis,
  - corrective action aging/overdue views,
  - audit throughput and completion metrics.
- Dashboard queries shall respect role/scope restrictions.

### R-010 Admin Ownership
- Admin features shall be isolated from auditor workflows.
- Template admin screens shall not be combined with audit completion screens.
- User/permission management shall be centralized in admin settings.

### R-011 Audit Workflow Baseline
- The system shall preserve baseline workflow:
  - draft save/reopen,
  - submit + review flow,
  - PDF/print output,
  - email routing by division,
  - live score behavior,
  - required-field and unanswered-item validation.

### R-012 Logging and Auditability
- Key events shall be logged (template publish, audit submit, corrective action updates, permission changes).
- Logs must be queryable for QA validation.

## Data Model Requirements
- SQL Server/Azure SQL is the primary store.
- Required core entities:
  - `AuditTemplate`
  - `AuditTemplateVersion`
  - `AuditSection`
  - `AuditQuestion`
  - `ResponseType`
  - `ResponseOption`
  - `ReportingCategory`
  - `Audit`
  - `AuditResponse`
  - `AuditFinding`
  - `CorrectiveAction`
  - `EmailRoutingRule`
  - `TemplateChangeLog`
  - `UserDivision` / scope mapping entities
- Constraints required:
  - one active version per template,
  - uniqueness of question order per section/version,
  - uniqueness for response rows per audit/question,
  - status check constraints.

## API Requirements
- Required endpoints must cover:
  - division lookup,
  - active template retrieval by division,
  - create/load/save/submit/review audit lifecycle,
  - template clone/publish/reorder/add/remove operations,
  - email routing administration,
  - reporting and KPI aggregation,
  - scoped audit listing and secure detail retrieval.

## UI Requirements

### Auditor UI
- `/audit-management/audits`
- `/audit-management/audits/new`
- `/audit-management/audits/:id`
- `/audit-management/audits/:id/review`

### Admin UI
- `/audit-management/admin/templates`
- `/audit-management/admin/settings`

### Reporting UI
- `/audit-management/reports`

## Non-Functional Requirements
- Do not alter enterprise shell layout behavior (header/sidebar/theme/auth wrapper).
- All changes must be incremental and buildable.
- Every change must pass QA gate requirements and produce artifacts.

## QA Acceptance and Gates

### Required automation coverage
- Button clickability and functionality (strict coverage).
- Visual regression with manual snapshot approval.
- Form logic and scoring accuracy.
- Drag/drop reorder behavior in template admin.
- KPI/reporting data accuracy and scope enforcement.
- DB and logging checks for critical actions.
- End-to-end workflow coverage.

### Required gates
1. Pre-change baseline gate.
2. Post-change PR gate.
3. Pre-merge final gate.
4. Pre-release gate.

## Delivery Phasing
1. Phase 1: shell + route scaffolding.
2. Phase 2: schema + migration + seed data.
3. Phase 3: API/CQRS.
4. Phase 4: audit form engine.
5. Phase 5: review/PDF/email routing.
6. Phase 6: template manager (including drag/drop).
7. Phase 7: KPI/reports + scoped analytics.

## Change Control
- Any schema change requires explicit authorization and migration QA impact logging.
- Any requirement change must update this document and `docs/qa` gate artifacts in the same PR.
