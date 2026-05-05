# Role / Access Matrix

**Last updated:** 2026-05-05  
**Branch:** feat/phase-2c-enhancements

This document is the authoritative reference for which roles can access which areas of the Stronghold Audit application.

---

## Role Definitions

| Role | Code | Description |
|------|------|-------------|
| Administrator | `Administrator` | Full system access. Bypasses all frontend guards. |
| IT Admin | `ITAdmin` | Manages users and roles. Cannot access audit content. |
| Audit Admin | `AuditAdmin` | Full audit lifecycle access + template/settings admin. |
| Audit Manager | `AuditManager` | Manages audits and reports across divisions. Read/manage, no settings admin. |
| Audit Reviewer | `AuditReviewer` | Reviews submitted audits, writes review summaries, distributes. |
| Auditor | `Auditor` | Creates and submits audits for assigned divisions only. |
| Template Admin | `TemplateAdmin` | Manages templates and newsletter. No audit create/approve. |
| Executive | `Executive` | Read-only: reports and compliance status. |
| Executive Viewer | `ExecutiveViewer` | Same as Executive (legacy alias). |
| Normal User | `NormalUser` | Limited: corrective actions assigned to them only. |
| Read-Only Viewer | `ReadOnlyViewer` | Read-only access to reports. |

---

## Frontend Route Guards

Guards are enforced in `webapp/src/router/index.ts` and `webapp/src/modules/audit-management/router/index.ts`.

> **Important:** Frontend guards are a UX convenience only. Backend authorization via `[AllowedAuthorizationRole]` attributes is the enforced security boundary.

| Route / Area | Admin | ITAdmin | AuditAdmin | AuditManager | AuditReviewer | Auditor | TemplateAdmin | Executive | NormalUser |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| `/audit-management/audits` | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| `/audit-management/audits/new` | ✅ | ❌ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ |
| `/audit-management/audits/:id/review` | ✅ | ❌ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/audit-management/corrective-actions` | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| `/audit-management/reports` | ✅ | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |
| `/audit-management/admin/templates` | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `/audit-management/admin/settings` | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `/audit-management/admin/users` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `/audit-management/newsletter/template-editor` | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `/audit-management/print/:divisionId` | Unguarded (backend enforces) | | | | | | | | |
| `/audit-management/newsletter` | Unguarded (backend enforces) | | | | | | | | |
| `/audit-management/print-review/:auditId` | Unguarded (backend enforces) | | | | | | | | |
| `/ca/:token` | Anonymous — public token endpoint | | | | | | | | |

**Note on print/newsletter routes:** These standalone print/export routes have no frontend guard because they are used by server-side PDF generation (PuppeteerSharp makes headless requests). Backend handlers enforce authorization on the underlying data APIs they call. Direct browser access without a valid session returns no meaningful data.

---

## Backend Authorization (Handler Level)

Authorization is enforced by `AuthorizationBehavior<TRequest, TResponse>` in the MediatR pipeline. Each handler is decorated with `[AllowedAuthorizationRole(...)]`.

| Handler | Allowed Roles |
|---------|--------------|
| `GetAuditList` | AuditManager, AuditReviewer, TemplateAdmin, Administrator, AuditAdmin, Auditor, Executive |
| `SaveAuditResponses` | AuditManager, AuditReviewer, TemplateAdmin, Administrator, AuditAdmin, Auditor |
| `SubmitAudit` | AuditManager, Administrator, AuditAdmin, Auditor |
| `StartReview` | AuditManager, AuditReviewer, Administrator, AuditAdmin |
| `ApproveAudit` | AuditManager, Administrator, AuditAdmin |
| `CloseAudit` | AuditManager, Administrator, AuditAdmin |
| `ReopenAudit` | AuditManager, Administrator, AuditAdmin |
| `SendDistributionEmail` | AuditManager, AuditReviewer, Administrator, AuditAdmin |
| `GetDistributionPreview` | AuditManager, AuditReviewer, Administrator, AuditAdmin |
| `GetComplianceStatus` | AuditManager, AuditReviewer, ReadOnlyViewer, ExecutiveViewer, TemplateAdmin, Administrator, AuditAdmin, Executive |
| `GetScheduledReports` | AuditManager, AuditReviewer, TemplateAdmin, Administrator, AuditAdmin, Executive |
| `GenerateReport` | AuditManager, AuditReviewer, ReadOnlyViewer, ExecutiveViewer, TemplateAdmin, Administrator, AuditAdmin, Executive |
| `GetCorrectiveActions` | AuditManager, AuditReviewer, TemplateAdmin, Administrator, AuditAdmin, Auditor, NormalUser |
| `AssignCorrectiveAction` | AuditManager, Administrator, AuditAdmin |
| `CloseCorrectiveAction` | AuditManager, Administrator, AuditAdmin |
| `AdminUsers (GET/POST/PUT)` | Administrator, ITAdmin |

---

## Division Scope Enforcement (Backend)

Users can be assigned to specific divisions, restricting which audit data they see.

| Handler | Division Scoping |
|---------|-----------------|
| `GetAuditList` | ✅ Enforced via `AllowedDivisionIds` |
| `GetComplianceStatus` | ✅ Enforced (fixed 2026-05-05) |
| `GetScheduledReports` | ✅ Enforced (fixed 2026-05-05) |
| `GenerateReport` | ✅ Enforced |
| `GetCorrectiveActions` | ✅ Enforced |
| `GetAuditsByEmployee` | ✅ Enforced |

**Global roles** (Administrator, AuditAdmin, AuditManager, TemplateAdmin, Executive, ExecutiveViewer) see all divisions regardless of UserDivisions assignment.

---

## Known Remaining Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Print/newsletter routes unguarded on frontend | Low | Backend data APIs require authentication; PuppeteerSharp-rendered pages have no interactive attack surface |
| Scheduled report background service runs without user auth context | Low | Service runs with DB access only; cannot bypass API authorization; runs only for pre-configured schedules created by authorized admins |
| NormalUser can access corrective-actions list | By Design | NormalUser is a valid CA assignee; backend filters CAs to assigned-to-email scope |
