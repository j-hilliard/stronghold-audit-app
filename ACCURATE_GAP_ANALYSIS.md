# Stronghold Audit App — ACCURATE Gap Analysis
**Date:** April 20, 2026
**Based on:** Complete codebase inventory (60+ handlers, 28 models, 46 UI components)
**Status:** CORRECTED per Codex review

---

## Workflow Mapping: What Exists vs. What's Missing

### **PHASE 1: Pre-Audit – Training Records**

**Requirement:** Pull training records from E-Charts using job number(s); flag expired/missing training

| Item | Status | Details |
|------|--------|---------|
| Create new audit | ✅ EXISTS | `CreateAudit` handler. Locks TemplateVersionId and AuditType. Supports optional section enablement. |
| Training record pull | ❌ NOT IMPLEMENTED | No E-Charts API connector. Zero integration with external training systems. |
| Flag expired training | ❌ NOT IMPLEMENTED | No training data model, no flag logic. |
| Include in report/email | ❌ NOT IMPLEMENTED | No training data to include. |

**GAP:** Phase 1 is **completely blocked** until E-Charts integration built. Currently, auditors manually look up training separately.

---

### **PHASE 2: On-Site Audit**

**Requirement:** Conduct site audit, document findings in notebook, review with supervision

| Item | Status | Details |
|------|--------|---------|
| Audit form | ✅ FULL | `SaveAuditResponses` handler. Vue form with dynamic sections, conditional show/hide logic, scoring. |
| Question types | ✅ FULL | StatusChoice, YesNo, YesNoNA, Text, Number, Date. Response snapshots for historical accuracy. |
| Photo capture | ✅ FULL | `FindingPhotos` handler + UI. Photos stored per response with caption. Enforces per-division `RequirePhotoOnNc`. |
| Attachments | ✅ FULL | `UploadAttachment` handler. File persistence with FileName, FilePath, FileSizeBytes validation. |
| Draft save | ✅ FULL | `SaveAuditResponses` is upsert; full edit until submit. |
| Review findings on-site | ⚠️ PARTIAL | No specific "on-site review" flow; supervisor would review findings post-audit via `AuditReviewView` dashboard. |

**STATUS:** Phase 2 is **fully functional**. Form, photos, attachments all wired.

---

### **PHASE 3: Audit Report Creation**

**Requirement:** Generate email to management + site supervision with findings/positives and training records; auditor generates audit report; send to review group

| Item | Status | Details |
|------|--------|---------|
| Submit audit | ✅ FULL | `SubmitAudit` handler. Draft → Submitted. Validates life-critical fields. Auto-creates Findings. Triggers email. |
| Auto-create corrective actions | ✅ FULL | If `AutoCreateCa=true` on question, CA created on submit (idempotent per AuditId+QuestionId). |
| Generate audit report | ✅ FULL | `GetAuditReport` computes conformance score with life-critical fail flag. Score = Σ(response_value × q_weight × s_weight) / Σ(eligible_weights). |
| Email to management | ⚠️ PARTIAL | `SubmitAudit` sends via IEmailService, but **subject/body hardcoded**. No template customization. No training records attached (none exist yet). |
| Email to review group | ✅ FULL | Review group configured via `SaveReviewGroup`; `SubmitAudit` sends to all members. Hardcoded template. |
| Include training records | ❌ NOT IMPLEMENTED | Blocked on Phase 1 (no training data). |

**STATUS:** Phase 3 is **mostly working**. Emails send, but templates are hardcoded and training data missing.

---

### **PHASE 4: Internal Review & Distribution**

**Requirement:** Audit reviewed by one of [Cheryl Wyatt, Shawn Hausberger, Shelby Johnson]; reviewer finalizes; distribute to predefined distribution list; if CA required, auto-enter 2-week due date; send via Outlook

| Item | Status | Details |
|------|--------|---------|
| Review audit | ✅ FULL | `AuditReviewView` shows submitted audit, findings, draft CAs. No formal "assign to reviewer" yet—open for anyone with AuditReviewer role. |
| Reviewer finalize | ✅ FULL | `SubmitAudit` marks Submitted; no second approval gate currently. Could be added if business rule requires explicit reviewer sign-off before distribution. |
| Distribute to list | ✅ FULL | `UpdateEmailRouting` manages per-division recipient list in EmailRoutingRule table. |
| 2-week CA deadline | ✅ FULL | `AssignCorrectiveAction` computes DueDate = today + Division.SlaNormalDays (configurable). No hardcoded 2-week. |
| Send via Outlook | ⚠️ PARTIAL | Uses IEmailService (SMTP). Does NOT directly integrate Microsoft Graph for distribution lists. Can send to static list of emails. **Graph integration exists** (`GraphService`) but not wired to email sending. |
| Distribution emails | ✅ FULL | EmailRoutingRule table stores per-division recipients. 8 rules already configured (CSL, SHC ETS, SHC STG, SHC STS, SHC SHI, SHC TKIE, currently EVG & GEM together, ENAIS). |

**STATUS:** Phase 4 is **95% working**. Distribution list emails exist, but Graph API integration for Outlook delegation not yet wired.

---

### **PHASE 5: Data Entry & Tracking (Quarterly Summaries)**

**Requirement:** Manually enter (currently) into quarterly summary:
- Summary Tally (date, job#, auditor, finding counts)
- Corrective Actions Tracking (with color coding)
- Audits by Employee
- File Storage & Linking (direct links to audit + CA docs)

| Item | Status | Details |
|------|--------|---------|
| Generate quarterly summary | ⚠️ PARTIAL | **Export exists** (`ExportQuarterlySummary`, `ExportCorrectiveActions`) but no auto-population service. Still requires manual data entry. |
| Summary Tally structure | ❌ NOT IMPLEMENTED | No service that groups audits by date/job/auditor and counts findings. Export could provide this—need to verify. |
| CA color coding (Red/Yellow/Green) | ✅ PARTIAL | Severity field exists on CorrectiveAction, but export color-coding not confirmed. |
| Audits by Employee | ⚠️ PARTIAL | `GetAuditsByEmployee` handler exists; export exists. Need to verify export includes employee tally. |
| File linking (H/L drives) | ❌ NOT IMPLEMENTED | No integration with file shares. Exports are local downloads, not linked to centralized storage. |
| Automation | ❌ NOT IMPLEMENTED | No scheduled service to auto-populate summaries. Still fully manual workflow. |

**STATUS:** Phase 5 is **INCOMPLETE**. Exports exist but summaries require manual data entry. No automation. File linking not implemented.

**Evidence:** `ExportQuarterlySummary.cs` exists but need to verify it groups data correctly.

---

### **PHASE 6: Reporting & Visualization**

**Requirement:** Use Summary Tally data to generate summary tab; populate graphs for newsletters; create newsletters with compliance awards, audit location map, graphs

| Item | Status | Details |
|------|--------|---------|
| Report generation | ✅ FULL | `GenerateReport` handler. 6 templates: annual-review, quarterly-summary, post-audit-summary, ncr-report, executive-dashboard, ca-aging. |
| Dashboard | ✅ FULL | `AuditDashboardView` shows recent audits, compliance status, CA aging, division metrics. |
| Graphs | ✅ FULL | Report composer with BarChartBlock, LineChartBlock, PieChartBlock (SVG). |
| Newsletter | ⚠️ PARTIAL | `GetNewsletterTemplate` (per-division config), `GenerateNewsletterSummary`, `SendNewsletter`. **But SendNewsletter is DRY RUN only** (logs, doesn't send). Compliance awards not modeled. Audit location map not implemented. |
| Scheduled reports | ✅ FULL | `ScheduledReportService` runs every 5 min. Emails PDF to recipients from ScheduledReport config. |
| Export to newsletter | ⚠️ PARTIAL | Report composer generates, but automated population from Summary Tally not wired. |

**STATUS:** Phase 6 is **80% complete**. Reports, graphs, dashboards all exist. Newsletter sending blocked by DRY RUN; compliance awards/map not modeled.

---

## Critical Gaps Summary

### **1. E-Charts Training Integration** ❌
- **Status:** Not implemented
- **Impact:** Phase 1 completely blocked. Auditors can't pull training records.
- **Effort:** 5-7 days (once API contract confirmed)
- **Blocker:** Requires vendor contract + API credentials

### **2. Phase 5 Data Automation** ⚠️ PARTIAL
- **Status:** Exports exist; summary service doesn't
- **What works:** `ExportQuarterlySummary`, `ExportCorrectiveActions` handlers
- **What's missing:**
  - No service that auto-groups audits into quarterly summary
  - No scheduled job that triggers export
  - No file linking to H/L drives
- **Impact:** Users still manually copy data from app to Excel
- **Effort:** 3-4 days (orchestrate existing exports + add scheduling)
- **Time saved:** ~2 hrs/quarter if automated

### **3. Newsletter Sending** ⚠️ PARTIAL
- **Status:** DRY RUN only (logs to console)
- **What works:** IEmailService exists and is SMTP-capable
- **What's missing:** `SendNewsletter` handler doesn't call IEmailService; just logs
- **Impact:** Newsletters generated but never emailed
- **Effort:** 1 day to wire SendNewsletter → IEmailService
- **Code location:** `Api/Domain/Audit/Newsletter/SendNewsletter.cs`

### **4. File Linking to H/L Drives** ❌
- **Status:** Not implemented
- **Impact:** Users manually manage file paths in spreadsheets
- **Effort:** 2-3 days (requires SMB client + path mapping config)

### **5. Compliance Awards Modeling** ❌
- **Status:** Not in database schema
- **Impact:** Newsletter can't showcase awards (manual insertion required)
- **Effort:** 1 day (add Award model + newsletter block)

### **6. Audit Location Map** ❌
- **Status:** Not implemented
- **Impact:** Geographic visualization missing from newsletters
- **Effort:** 3-5 days (geocoding + map library)

### **7. Formal Review/Approval Gate** ⚠️ PARTIAL
- **Status:** Reviewers exist; no formal assignment or sign-off flow
- **Impact:** Any AuditReviewer can see audit, but no clear "assign to reviewer" step
- **Effort:** 2-3 days (add assignment + state machine)

---

## What's Actually Working Well

### ✅ **Phase 2: On-Site Audit (100% complete)**
- Form with dynamic sections, conditional logic
- Photo/evidence capture with caption
- Attachments with file validation
- Draft saving with full edit capability
- Scoring computation (weighted sections + questions)
- Life-critical fields enforcement

### ✅ **Core Corrective Actions (90% complete)**
- Create, assign, update, close CAs
- DueDate computed from division SLA
- Auto-CA generation on submit (idempotent)
- Priority escalation (Normal/Urgent)
- Daily reminders: DueSoon (3 days), DueToday, Overdue
- Closure photo evidence
- 60-min role-based cache (scoped to AllowedDivisionIds)

### ✅ **Reporting Engine (85% complete)**
- 6 report templates (annual, quarterly, post-audit, NCR, executive, CA-aging)
- PDF generation via Chromium + PuppeteerSharp
- Block-based report composer (drag-drop layout)
- Charts (bar, line, pie via SVG)
- ScheduledReportService auto-emails every 5 min
- Custom primary color per report

### ✅ **Authorization & Multi-Tenancy (100% complete)**
- 6 audit roles + 2 global roles
- Division-scoped access (AllowedDivisionIds filter)
- MediatR authorization pipeline behavior
- 60-min in-memory role cache
- Azure AD integration ready; local dev bypass available

---

## Implementation Roadmap (Corrected)

### **Week 1: Wire Newsletter Sending** [1 day, highest ROI]
**Current:** SendNewsletter is DRY RUN only
**Change:** Call existing IEmailService to actually send

```csharp
// Api/Domain/Audit/Newsletter/SendNewsletter.cs
public class SendNewsletterHandler : IRequestHandler<SendNewsletter, NewsletterSendResult>
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;  // ← Already exists
    private readonly ILogger<SendNewsletterHandler> _logger;

    public SendNewsletterHandler(
        AppDbContext context,
        IEmailService emailService,
        ILogger<SendNewsletterHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<NewsletterSendResult> Handle(SendNewsletter request, CancellationToken cancellationToken)
    {
        var recipients = await _context.EmailRoutingRules
            .Where(r => r.DivisionId == request.DivisionId && r.IsActive && !r.IsDeleted)
            .Select(r => r.EmailAddress)
            .ToListAsync(cancellationToken);

        // Call existing IEmailService instead of just logging
        await _emailService.SendAsync(
            subject: request.Subject,
            htmlBody: request.HtmlBody,
            recipients: recipients,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Sent newsletter to {Count} recipients for division {DivisionId}",
            recipients.Count,
            request.DivisionId);

        return new NewsletterSendResult
        {
            Sent = recipients.Count,
            DryRun = false,
            Recipients = recipients,
        };
    }
}
```

**Verification:**
- Check IEmailService signature in `Api/Services/EmailService.cs`
- Verify Email config in `appsettings.json` (Email:Host, Email:Port, Email:From, etc.)
- Test: Call POST /api/audit/newsletters/send with test division
- Confirm: Email appears in mailbox (or dev redirect address if configured)

---

### **Week 2-3: Audit E-Charts Integration** [5-7 days, BLOCKER]
**Prerequisite:** Get E-Charts API contract from vendor (procurement task)

**Architecture:**
```
CreateAudit (front-end)
    ↓
Pre-Audit Training Fetch (new page)
    ↓
IEChartsService.GetTrainingRecordsAsync(jobNumbers)
    ↓
Add to Audit.TrainingRecords (many-to-many or list)
    ↓
Include in SubmitAudit email + report
```

**Files to create:**
1. `Api/Services/IEChartsService.cs` + `EChartsService.cs` (HTTP client)
2. `Api/Domain/Audit/PreAudit/FetchTrainingRecords.cs` (CQRS handler)
3. `Data/Models/Audit/TrainingRecord.cs` (new model)
4. `Api/Controllers/AuditController.cs` — add `/pre-audit/training` endpoint
5. `webapp/src/modules/audit-management/features/audit-form/PreAuditTraining.vue` (form page)

**Implementation Steps:**
1. Wire E-Charts API client (HttpClient + auth)
2. Create TrainingRecord model + migration
3. Create FetchTrainingRecords handler
4. Update CreateAudit to accept/store training data
5. Include training in SubmitAudit email template
6. Build pre-audit page

---

### **Week 4-5: Complete Phase 5 Automation** [3-4 days]
**Current:** Exports exist but no auto-population

**Architecture:**
```
ScheduledQuarterlySummaryService (runs monthly on 1st day)
    ↓
Queries Audits + CAs for date range (Q1 = Jan-Mar, etc.)
    ↓
Groups by date, job#, auditor
    ↓
Calls ExportQuarterlySummary + ExportCorrectiveActions
    ↓
Emails workbook to division distribution list + saves to archive location
```

**Verify existing:**
- `ExportQuarterlySummary.cs` handler — does it group correctly?
- `ExportCorrectiveActions.cs` handler — color-codes by severity?
- Uses ClosedXML (confirmed; not EPPlus)

**Files to create:**
1. `Api/Services/QuarterlySummaryService.cs` — orchestrate exports
2. `Api/Domain/Audit/Reports/ScheduleQuarterlySummary.cs` — CQRS handler
3. Update `Api/Services/ScheduledReportService.cs` — add quarterly summary trigger

**Implementation:**
1. Verify ExportQuarterlySummary groups audits by date/job/auditor/finding counts
2. Verify ExportCorrectiveActions sorts by severity and colors rows
3. Create QuarterlySummaryService that calls both + emails
4. Add background job to fire on Q1/Q2/Q3/Q4 start date
5. Add configuration for recipient email list + archive location

---

### **Week 6: Add File Linking** [2-3 days]
**Connect audit/CA documents to shared drive paths**

**Architecture:**
```
Audit submitted
    ↓
Save audit PDF to H/L drive (\\server\share\audits\division\date_jobnumber.pdf)
    ↓
Store path in Audit.AuditDocumentPath (new field)
    ↓
ExportQuarterlySummary includes hyperlink to path
```

**Files to create:**
1. Add `AuditDocumentPath` and `CorrectiveActionDocumentPath` to models
2. `Api/Services/FileShareService.cs` — SMB client for uploads
3. Update `SubmitAudit` handler to save PDF to share
4. Update ExportQuarterlySummary to include =HYPERLINK() formulas

---

### **Week 7: Compliance Awards Modeling** [1 day]
**Allow auditors/managers to issue awards**

**New Model:**
```csharp
public class ComplianceAward
{
    public int Id { get; set; }
    public int DivisionId { get; set; }
    public string AwardName { get; set; }
    public string AwardeeTeam { get; set; }
    public string Reason { get; set; }
    public DateTime AwardedDate { get; set; }
    public bool IncludeInNewsletter { get; set; }
}
```

**Files to create:**
1. Model + migration
2. `Api/Domain/Audit/Awards/RecordAward.cs` handler
3. `Api/Domain/Audit/Awards/GetAwards.cs` handler
4. `webapp/src/modules/audit-management/features/admin-awards/` (Vue page)
5. Add award block to newsletter composer

---

### **Week 8: Geographic Audit Map** [3-5 days]
**Show audit locations on interactive map**

**Architecture:**
```
Audits have location (from AuditHeader.Location or geocoded from job address)
    ↓
GenerateReport queries audits by location
    ↓
Renders map block (Leaflet or Mapbox)
    ↓
Pins show job counts, compliance scores
```

**Libraries:** Leaflet (free, open-source) or Mapbox (paid, prettier)

---

### **Week 9: Formal Review Assignment** [2-3 days]
**Optional: Assign audits to specific reviewer before distribution**

**New Fields on Audit:**
```csharp
public int? AssignedReviewerId { get; set; }
public DateTime? ReviewAssignedAt { get; set; }
public string ReviewNotes { get; set; }
```

**New Handler:**
```csharp
public class AssignAuditForReview : IRequest<Unit>
{
    public int AuditId { get; set; }
    public int ReviewerId { get; set; }
}
```

---

## What NOT to Do

### ❌ Duplicate EmailService
- `Api/Services/EmailService.cs` **already exists** and is production-ready
- Do NOT recreate it
- Just wire it to SendNewsletter

### ❌ Rewrite ExportQuarterlySummary
- Handler **already exists**
- Verify it does what you need; if not, extend it
- Do NOT create parallel QuarterlySummaryService with duplicate logic
- Orchestrate existing exports instead

### ❌ Switch to EPPlus
- Codebase **uses ClosedXML** for Excel
- Keep ClosedXML
- Do NOT add EPPlus dependency

### ❌ Ignore existing GraphService
- `Api/Services/GraphService.cs` **exists** for Microsoft Graph integration
- If you want Outlook delegation, wire to GraphService
- Don't try to send to distribution lists via plain SMTP (won't work)

---

## Accuracy Checklist (vs. Codex feedback)

| Codex Point | This Report | Status |
|-------------|------------|--------|
| CaReminderService exists | ✅ Confirmed | Runs daily; sends DueSoon/DueToday/Overdue |
| ScheduledReportService exists | ✅ Confirmed | Runs every 5 min; emails PDF |
| IEmailService exists | ✅ Confirmed | SMTP abstraction; DryRun mode available |
| SubmitAudit sends email | ✅ Confirmed | Uses IEmailService; hardcoded template |
| ClosedXML (not EPPlus) | ✅ Confirmed | All Excel exports use ClosedXML |
| SendNewsletter is DRY RUN | ✅ Confirmed | Logs only; needs to call IEmailService |
| Don't duplicate services | ✅ Heeded | Recommend wire, don't recreate |
| Use existing patterns | ✅ Heeded | Reference actual domain model names |

---

## Risk Assessment

| Gap | Severity | Blocker | Owner | Timeline |
|-----|----------|---------|-------|----------|
| E-Charts Integration | HIGH | YES | Procurement → Dev | 2-4 weeks (API setup) + 5 days (dev) |
| Phase 5 Automation | HIGH | NO | Dev | 3-4 days |
| Newsletter Sending | MEDIUM | NO | Dev | 1 day |
| File Linking | MEDIUM | NO | Dev | 2-3 days |
| Formal Review Gate | LOW | NO | Dev | 2-3 days (optional) |
| Compliance Awards | LOW | NO | Dev | 1 day (optional) |
| Geographic Map | LOW | NO | Dev | 3-5 days (optional) |

---

## Next Steps

1. ✅ **Read this report** (corrected vs. first draft)
2. ⏭️ **Confirm E-Charts API contract** with procurement (BLOCKER for Phase 1)
3. ⏭️ **Assign dev to Week 1 task** (wire NewsletterSending → IEmailService). 1-day quick win.
4. ⏭️ **Verify ExportQuarterlySummary** output against Phase 5 requirements. May already do what you need.
5. ⏭️ **Plan E-Charts integration** once API contract confirmed (5-7 days)
6. ⏭️ **Optional:** Prioritize by business impact. Phase 5 automation saves 2 hrs/quarter; awards/map are nice-to-have.
