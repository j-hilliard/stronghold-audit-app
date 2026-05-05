# Role / Access Matrix

**Last updated:** 2026-05-05 (Pass B)
**Branch:** feat/phase-2c-enhancements

This document is the authoritative reference for which roles can access which areas of the Stronghold Audit application.

---

## Role Definitions

| Role | Code | Description |
|------|------|-------------|
| Administrator | `Administrator` | Full system access. Bypasses all frontend guards. |
| IT Admin | `ITAdmin` | Manages users and roles only. No access to audit content, reports, or templates. |
| Audit Admin | `AuditAdmin` | Full audit lifecycle access + template/settings admin. Global division view. |
| Audit Manager | `AuditManager` | Manages audits and reports across divisions. No settings/template admin. |
| Audit Reviewer | `AuditReviewer` | Reviews submitted audits: edits responses on UnderReview audits, writes review summaries, approves, distributes, views reports. |
| Auditor | `Auditor` | Creates and submits audits for assigned divisions only. Sees only own audits. |
| Template Admin | `TemplateAdmin` | Manages templates and newsletter. Included in AuditAdmin access group for guards. |
| Executive | `Executive` | Read-only: reports, compliance dashboard, audits. No editing. |
| Executive Viewer | `ExecutiveViewer` | Same as Executive (legacy alias). |
| Normal User | `NormalUser` | Corrective actions assigned to them only. No audit or report access. |
| Read-Only Viewer | `ReadOnlyViewer` | Read-only access to reports. Backend only — no distinct frontend route. |

---

## Frontend Route Guards

Guards are enforced via route meta flags in `webapp/src/modules/audit-management/router/index.ts`
and the `beforeEach` hook in `webapp/src/router/index.ts`.

> **Important:** Frontend guards are a UX convenience layer only. Backend `[AllowedAuthorizationRole]` attributes on MediatR handlers are the enforced security boundary.

| Route / Area | Guard Meta | Admin | ITAdmin | AuditAdmin | AuditManager | AuditReviewer | Auditor | TemplateAdmin | Executive | NormalUser |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| `/audits` | `isAuditRoute` | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅¹ | ✅ | ❌ |
| `/audits/new` | `requiresCreateAudit` | ✅ | ❌ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ |
| `/audits/:id` | `isAuditRoute` | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅¹ | ✅ | ❌ |
| `/audits/:id/review` | review regex | ✅ | ❌ | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/corrective-actions` | path check | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅¹ | ✅² | ✅ |
| `/reports` (all sub-routes) | `requiresReports` | ✅ | ❌ | ✅ | ✅ | **✅³** | ❌ | ✅¹ | ✅ | ❌ |
| `/admin/templates` | `requiresAuditAdmin` | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `/admin/settings` | `requiresAuditAdmin` | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `/admin/audit-log` | `requiresAuditAdmin` | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `/admin/users` | `requiresITAdmin` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `/newsletter/template-editor` | `requiresAuditAdmin` | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `/print/:divisionId` | Unguarded (backend enforces) | — | — | — | — | — | — | — | — | — |
| `/newsletter` | Unguarded (backend enforces) | — | — | — | — | — | — | — | — | — |
| `/print-review/:auditId` | Unguarded (backend enforces) | — | — | — | — | — | — | — | — | — |
| `/ca/:token` | Anonymous — public token endpoint | — | — | — | — | — | — | — | — | — |

¹ TemplateAdmin is included in `isAuditAdmin` computed, which is checked in most guards.  
² Executive can view CAs read-only; backend blocks write operations.  
³ **Fixed in Pass B** — AuditReviewer was previously blocked from all reports pages.

**Note on unguarded print/newsletter routes:** These are standalone print/export views used by PuppeteerSharp (headless Chromium) for server-side PDF generation. They have no session context when rendered by the PDF service. Direct browser access without a real session only renders an empty page — no authenticated data is exposed.

---

## Backend Authorization (Handler Level)

Authorization is enforced by `AuthorizationBehavior<TRequest, TResponse>` in the MediatR pipeline. Each handler must declare `[AllowedAuthorizationRole(...)]` — handlers without this attribute throw `UnauthorizedAccessException` for all callers.

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
| `SaveScheduledReport` (create/edit) | AuditManager, TemplateAdmin, Administrator, AuditAdmin |
| `GenerateReport` | AuditManager, AuditReviewer, ReadOnlyViewer, ExecutiveViewer, TemplateAdmin, Administrator, AuditAdmin, Executive |
| `GetCorrectiveActions` | AuditManager, AuditReviewer, TemplateAdmin, Administrator, AuditAdmin, Auditor, NormalUser |
| `AssignCorrectiveAction` | AuditManager, Administrator, AuditAdmin |
| `CloseCorrectiveAction` | AuditManager, Administrator, AuditAdmin |
| `AdminUsers (GET/POST/PUT)` | Administrator, ITAdmin |

---

## Division Scope Enforcement (Backend)

Users can be assigned to specific divisions, limiting the audit data they see. `IAuditUserContext` carries `AllowedDivisionIds` set by `AuthorizationBehavior` from the `UserDivisions` table.

| Handler | Division Scoping |
|---------|-----------------|
| `GetAuditList` | ✅ Enforced |
| `GetComplianceStatus` | ✅ Enforced (Pass A) |
| `GetScheduledReports` | ✅ Enforced (Pass A) |
| `GenerateReport` | ✅ Enforced |
| `GetCorrectiveActions` | ✅ Enforced |
| `GetAuditsByEmployee` | ✅ Enforced |

**Global roles** (Administrator, AuditAdmin, AuditManager, TemplateAdmin, Executive, ExecutiveViewer) bypass `AllowedDivisionIds` and see all divisions.

---

## Auth Cache

Roles and division assignments are cached per user in `IMemoryCache` with a **5-minute absolute TTL** (reduced from 60 min in Pass B). Changes take effect within one cache window. Cache key: `AuditAuth_{azureAdObjectId}`.

No explicit invalidation path exists — the 5-minute TTL is the invalidation mechanism. If a security-critical role change must take effect immediately, restart the API process.

---

## Known Remaining Risks

| Risk | Severity | Status | Mitigation |
|------|----------|--------|-----------|
| CORS: `Cors:AllowedOrigins` not yet set in production App Config | Medium | Pending operator action | Code is ready; falls back to open with log warning until configured |
| Auth cache: no explicit invalidation on role change | Low | Accepted | 5-min TTL is adequate for role change propagation in this use case |
| Print/newsletter/print-review routes unguarded on frontend | Low | Accepted | No sensitive data rendered without authenticated API calls succeeding |
| ScheduledReportService: deactivated user's schedules continue firing | Low | Accepted | Service reads stored schedules only; admin must manually disable stale schedules |
| NormalUser can reach `/corrective-actions` URL | By Design | Accepted | Backend filters to assigned-to-email CAs only |
