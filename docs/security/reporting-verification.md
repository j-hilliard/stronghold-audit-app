# Reporting Audit — Verification Summary

**Last updated:** 2026-05-05

---

## Report Types

| Report | Route | Backend Handler | Notes |
|--------|-------|-----------------|-------|
| Compliance Status Dashboard | `/audit-management/reports` | `GetComplianceStatus` | Live per-division audit due/overdue status |
| Generate Report (PDF) | `/audit-management/reports/composer` | `GenerateReport` | Annual review, quarterly summary, NCR, executive dashboard, CA aging |
| Quarterly Summary (print view) | `/audit-management/reports/quarterly-summary` | `ExportQuarterlySummary` | Standalone print — no AppLayout |
| Newsletter | `/audit-management/newsletter` | (client-side render + print) | Vue-rendered, no backend report handler |
| Newsletter Template Editor | `/audit-management/newsletter/template-editor` | n/a | AuditAdmin-only frontend |
| Scheduled Reports | `/audit-management/reports/scheduled` | `GetScheduledReports` / `SaveScheduledReport` | Background service fires on schedule |

---

## Permission Verification

### GetComplianceStatus
- **Allowed roles:** AuditManager, AuditReviewer, ReadOnlyViewer, ExecutiveViewer, TemplateAdmin, Administrator, AuditAdmin, Executive
- **Division scoping:** ✅ Applied (fixed 2026-05-05) — non-global users see only their assigned divisions
- **Auditor access:** ❌ Not allowed — Auditors do not see the compliance dashboard (by design; they work at the audit level)

### GenerateReport
- **Allowed roles:** AuditManager, AuditReviewer, ReadOnlyViewer, ExecutiveViewer, TemplateAdmin, Administrator, AuditAdmin, Executive
- **Division scoping:** ✅ Applied — `AllowedDivisionIds` filter active for non-global users
- **PDF output:** PuppeteerSharp renders the report HTML to PDF server-side

### GetScheduledReports
- **Allowed roles:** AuditManager, AuditReviewer, TemplateAdmin, Administrator, AuditAdmin, Executive
- **Division scoping:** ✅ Applied (fixed 2026-05-05) — non-global users see only schedules for their allowed divisions (or global schedules with null DivisionId)

### SaveScheduledReport
- **Allowed roles:** AuditManager, TemplateAdmin, Administrator, AuditAdmin (write-capable roles only)
- **Division scoping:** Validates that the requesting user has access to the specified DivisionId before saving

---

## Scheduled Report Background Service

File: `Api/BackgroundServices/ScheduledReportService.cs`

- Runs every 5 minutes, polls `ScheduledReports` where `NextRunAt <= UtcNow && IsActive = true`
- **No user auth context** — operates with direct DB access
- Generates PDF via `IPdfGeneratorService`, sends via `IEmailService`
- Recipients are stored in the schedule definition (`RecipientsJson`)
- **Security posture:** Background service cannot create or modify schedules — it only reads pre-authorized schedules created by users with write permission. A deactivated user's schedules continue to fire (by design — report delivery is decoupled from user lifecycle).
- **Risk:** If a user is deactivated, their schedules must be manually disabled via Admin UI to stop delivery.

---

## Newsletter Template Editor

- Route: `/audit-management/newsletter/template-editor`
- Guard: `requiresAuditAdmin: true` (frontend, added 2026-05-05)
- Backend: No separate API endpoint — template state is persisted to `localStorage` or exported as JSON
- **Note:** Newsletter template changes are local to the browser session unless exported/imported. There is no server-side template persistence for the newsletter editor at this time.

---

## Known Gaps

| Gap | Severity | Notes |
|-----|----------|-------|
| Newsletter standalone route has no frontend guard | Low | No sensitive data rendered without a real audit selected; backend APIs called by the newsletter page require auth |
| Scheduled report service runs for deactivated users' schedules | Low | Requires manual admin action to disable; does not expose data outside the pre-configured recipient list |
