# 02 - Feature Status and Gaps
**Scope:** What is implemented, partial, or pending across business workflows.

## A. Core Product Status (High-Level)
## Implemented and Active
- Template engine with versioning and publish flow
- Dynamic audit form and response persistence
- Audit submit/review/reopen/close lifecycle
- Corrective actions (manual + auto-create behavior)
- Reporting dashboard and report composer
- Newsletter scaffolding and template editor
- Admin settings (email routing, score targets, role assignment)
- Admin users page with role assignment and user edit UI
- Attachments and photo evidence paths
- EF migration-based schema evolution

## Partial / Needs hardening
- Distribution preview/send workflow stability
- Corrective action close/bulk-reassign edge paths
- Dashboard simplification and KPI drill clarity
- Composer UX consistency and print parity
- Watcher automation reliability and reporting signal quality

## Deferred by decision
- E-Charts integration (explicitly deferred to last)

---

## B. Requirement Mapping (From Active Business Requests)
## 1) Role-based access controls and view separation
- Status: **Partial to strong**
- Evidence:
  - role checks in backend pipeline (`AuthorizationBehavior`)
  - user-role admin pages in frontend
- Remaining:
  - run full role x route x action verification in one deterministic gate
  - verify scoped data visibility for all non-global roles

## 2) Weighted scoring (topic + group)
- Status: **Implemented in model + calculations**
- Evidence:
  - `AuditSection.Weight`
  - `AuditQuestion.Weight` / `AuditVersionQuestion.Weight`
  - response snapshots: `QuestionWeightSnapshot`, `SectionWeightSnapshot`
  - weighted logic in `GetAuditReport` and `GetAuditReview`
- Remaining:
  - UX transparency for score math (tooltips/legend in review/report UI)
  - regression tests for weighted edge scenarios

## 3) Optional/toggle sections (rope access/RT/etc.)
- Status: **Implemented foundation**
- Evidence:
  - `AuditEnabledSection` model
  - optional group handling in create/save flows
- Remaining:
  - confirm final UX behavior in new-audit toggle step and downstream visibility

## 4) Attachments
- Status: **Implemented**
- Evidence:
  - upload/list/download/delete handlers for audit attachments
  - finding and CA photo support
- Remaining:
  - audit size and file-policy guardrails for production hardening

## 5) Corrective action notifications (3-day reminders)
- Status: **Implemented**
- Evidence:
  - `CaReminderService` background job
  - `CaNotificationLog` for dedup
- Remaining:
  - confirm SMTP production mode behavior and template polish

## 6) Submission notifications with direct links
- Status: **Implemented / enhanced**
- Evidence:
  - submission email logic in `SubmitAudit`
  - recipient expansion includes routing + review group + audit admins
- Remaining:
  - end-to-end template/content validation in non-dry-run environment

## 7) Mobile view
- Status: **Partial**
- Evidence:
  - some responsive behavior exists
- Remaining:
  - explicit mobile acceptance sweep for audit form, review, CA tables, reports, composer

## 8) Auto-generated reporting and easier composer workflow
- Status: **Partial**
- Evidence:
  - report composer exists
  - scheduled reports + report generation exist
- Remaining:
  - quick-report presets and “one-click operational reports” UX
  - reduce dashboard cognitive overload

## 9) Excel exports
- Status: **Implemented**
- Evidence:
  - `ExportQuarterlySummary`, `ExportNcrReport`, `ExportCorrectiveActions`
- Remaining:
  - strict parity validation against legacy workbook shapes and filtering edge cases

---

## C. Phase2C Enhancement Batch Status (Latest Requested Change Set)
Requested items were:
1. Distribution preview modal
2. Submit-notify audit admins
3. Admin user edit button
4. CA export honors active filters
5. Close audit blocked by open CAs
6. Root cause field on CA
7. Assignee autocomplete

## Code Presence Check
- `GetDistributionPreview` handler and route: **present**
- SubmitAudit recipient query for `AuditAdmin`: **present**
- `AdminUsersView` edit dialog and update call: **present**
- ExportCorrectiveActions filter extensions: **present**
- CloseAudit open-CA blocking exception: **present**
- `CorrectiveAction.RootCause` + DTO mapping: **present**
- CA assignee `AutoComplete` usage: **present**

## Validation state
- Code appears present, but this set still requires deterministic E2E/API pass confirmation in a stable environment.

---

## D. Largest Remaining Gaps (Priority)
1. **Execution stability over feature count**
   - eliminate false negatives from environment/test harness failures first
2. **Dashboard usability**
   - simplify top-level experience and reduce scroll fatigue
3. **Role + scope contract certainty**
   - full matrix validation for demo confidence
4. **Composer polish**
   - predictable layout controls, better page-level template model, robust print output
5. **Operational exports and report trust**
   - exact filter parity and deterministic data contract checks

## E. E-Charts Position
- E-Charts remains **deferred to last** and should not block current local work.
- Keep architecture hooks and placeholder docs, but do not schedule implementation before access exists.

