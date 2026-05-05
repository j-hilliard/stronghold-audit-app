# Known Risks and Deferred Items

**Last updated:** 2026-05-05  
**Reviewed by:** Hardening pass — feat/phase-2c-enhancements

This document catalogues all known security gaps, architectural risks, and deferred improvements identified during the Phase 2c hardening pass. Each item includes a severity rating and a mitigation or owner.

---

## Security / Authorization

| # | Item | Severity | Status | Mitigation |
|---|------|----------|--------|-----------|
| S1 | Print/newsletter standalone routes have no frontend route guard | Low | Accepted | Backend APIs called by these views require a valid session. No sensitive data is rendered without authenticated API calls succeeding. |
| S2 | Scheduled report background service has no user context | Low | Accepted | Service can only execute pre-authorized schedules. Cannot create, modify, or read beyond stored recipients. Deactivated-user schedules must be manually disabled. |
| S3 | `NormalUser` can access corrective-actions list URL | By Design | Accepted | NormalUser is a valid CA assignee. Backend `GetCorrectiveActions` filters to assigned-to-email scope for NormalUser. |
| S4 | Dev role switcher was missing AuditReviewer, AuditManager, TemplateAdmin | Fixed | ✅ Fixed 2026-05-05 | All 8 testable roles now present in DevRoleSwitcher. |
| S5 | Audit review route guard was missing `isAdmin` check | Fixed | ✅ Fixed 2026-05-05 | Administrators can now reach review routes without frontend redirect to unauthorized. |
| S6 | Newsletter template editor route was missing `requiresAuditAdmin` guard | Fixed | ✅ Fixed 2026-05-05 | Route now blocked for non-AuditAdmin/Admin users at the frontend. |

---

## Data Scoping

| # | Item | Severity | Status | Mitigation |
|---|------|----------|--------|-----------|
| D1 | `GetComplianceStatus` returned all divisions regardless of user scope | Medium | ✅ Fixed 2026-05-05 | `IAuditUserContext.AllowedDivisionIds` filter applied to division query. |
| D2 | `GetScheduledReports` returned all scheduled reports regardless of user scope | Medium | ✅ Fixed 2026-05-05 | `IAuditUserContext.AllowedDivisionIds` filter applied; global schedules (null DivisionId) are still visible to all authorized roles. |
| D3 | `GetAuditsByEmployee` division scoping (verify) | Low | Unverified | Handler injected `IAuditUserContext`. Awaiting test coverage. |

---

## Architecture / EF Core

| # | Item | Severity | Status | Notes |
|---|------|----------|--------|-------|
| A1 | `CK_AppNotification_Type` does not include `ReviewStarted` | High | Pending migration | Seeded in Plan file. Migration `add_workflow_statuses_and_notifications` was generated. Verify applied. |
| A2 | CA status constraint does not include `Submitted` | High | Pending migration | Same migration batch. |
| A3 | `DbInitializer` does not accept `isProductionEnvironment` param; environment safety enforced by caller in `Program.cs` | Low | Accepted | Program.cs correctly gates demo/local seeds. No architectural risk in current form. |

---

## Workflow / Product

| # | Item | Severity | Status | Notes |
|---|------|----------|--------|-------|
| W1 | `SendDistributionEmail.cs` marks audit as Distributed before email send succeeds | Medium | Deferred | If SMTP throws, audit is already marked Distributed. A retry would re-send to recipients. Mitigation: add idempotency check or move status update after send. |
| W2 | Assignment email does not include public CA token link | Medium | Deferred | Plan file covers this in Step 8. Assignees receive email but must navigate separately. |
| W3 | External CA public page has no photo upload UI (only closure notes) | Low | Deferred | `UploadCaClosurePhoto.cs` backend handler exists. UI surface not yet built on `/ca/:token`. |
| W4 | `includeAuditPdf` checkbox results in real PDF attachment via backend send; mailto fallback does not attach | By Design | Accepted | Mailto is a fallback for SMTP-unconfigured environments. Label in UI clarifies attachment is sent when using the primary backend send path. |

---

## Testing Coverage

| # | Item | Severity | Status | Notes |
|---|------|----------|--------|-------|
| T1 | Division-scoped compliance status has no E2E test | Low | Deferred | Manual verification: log in as a division-scoped user and confirm only their divisions appear in dashboard. |
| T2 | AuditReviewer role not covered by route-guard security spec | Low | Deferred | Spec at `audit-corrective-actions-contract.spec.ts` covers contract; route guard spec exists but does not cover AuditReviewer paths. |
| T3 | TemplateAdmin role not covered by dev switcher tests | Low | Fixed | DevRoleSwitcher now includes TemplateAdmin. |
