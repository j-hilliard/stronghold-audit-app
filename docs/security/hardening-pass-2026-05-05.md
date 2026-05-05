# Hardening Pass — 2026-05-05

**Branch:** feat/phase-2c-enhancements  
**Reviewed by:** Claude Code (automated hardening pass)  
**Purpose:** Pre-PR security, scoping, and auditability pass before merging Phase 2c to main.

---

## What Was Audited

1. Backend authorization — role attributes on all MediatR handlers
2. Division scope enforcement — `IAuditUserContext.AllowedDivisionIds` propagation
3. Frontend route guards — `router/index.ts` and module router meta flags
4. Dev tooling accuracy — `DevRoleSwitcher` role list vs. `AuthorizationRole.cs`
5. Database / EF bootstrap — environment safety matrix
6. Reporting — permission and scope coverage for all report types

---

## Fixes Applied

### Backend (C#)

| File | Change | Reason |
|------|--------|--------|
| `Api/Domain/Audit/Reports/GetComplianceStatus.cs` | Injected `IAuditUserContext`; filter divisions by `AllowedDivisionIds` when not global | Division-scoped users (AuditManager, AuditReviewer) were seeing all divisions in compliance dashboard |
| `Api/Domain/Audit/Reports/GetScheduledReports.cs` | Injected `IAuditUserContext`; filter scheduled reports by `AllowedDivisionIds` when not global | Division-scoped users were seeing all scheduled reports across all divisions |

### Frontend (Vue/TS)

| File | Change | Reason |
|------|--------|--------|
| `webapp/src/modules/audit-management/router/index.ts` | Added `requiresAuditAdmin: true` to `newsletter/template-editor` route meta | Template editor was accessible to any authenticated user |
| `webapp/src/router/index.ts` | Added `\|\| userStore.isAdmin` to audit review route guard | Administrators were blocked from review routes by the frontend guard |
| `webapp/src/components/layout/DevRoleSwitcher.vue` | Added `AuditReviewer`, `AuditManager`, `TemplateAdmin` to ROLES list | These three roles had no dev-mode impersonation path for local testing |

---

## Documentation Created

| File | Contents |
|------|----------|
| `docs/security/role-access-matrix.md` | Full role × feature access matrix (frontend + backend) |
| `docs/security/user-provisioning.md` | How users are created, roles assigned, environment-specific seeding |
| `docs/security/environment-matrix.md` | Behavior differences between Local, Development, and Production |
| `docs/security/reporting-verification.md` | Permission and scope verification for all report types |
| `docs/security/known-risks.md` | Catalogued risks, severity, status, and mitigations |

---

## Test Results (Post-Fix)

Run after all code changes. See E2E regression results in session artifacts.

- Smoke gate: pass
- Corrective actions contract: 28/34 pass (6 pending — pre-existing, tracked in known-risks)
- Route guard security spec: pass
- Admin users contract: pass
- External CA public flow: pass
- Reviewer mode roundtrip: pass

---

## Sign-Off Checklist

- [x] All confirmed security gaps fixed or accepted with documented rationale
- [x] Division scoping verified for compliance status and scheduled reports
- [x] Frontend guards aligned with backend authorization matrix
- [x] Dev tooling covers all testable roles
- [x] No schema changes required for this hardening pass
- [x] Documentation complete
- [ ] PR created and reviewed (pending)
