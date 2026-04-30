# AUDIT APP REPORTING REDESIGN SPEC
Generated: 2026-04-28

---

## 1. CURRENT STATE MAP

### File Inventory

| File | Lines | What It Does | How It Gets Data | Produces |
|---|---|---|---|---|
| `ReportsView.vue` | 1,658 | Analytics dashboard: KPI tiles, 5 tabs (Overview/Analysis/Action Items/History/Performance), drill-down filters, chart configs, auditor performance table | Calls `getClient().getAuditReport()` inline; has own `getClient()` at line 1043 | Interactive dashboard — not a deliverable report |
| `ReportComposerView.vue` | 710 | Draft-based builder: pick division/sections/date/title, build PDF | Uses `useReportEngine.ts` + `useReportDraft.ts`; calls `new AuditClient()` 6 times (lines 339, 349, 364, 390, 536, 567) | Server-generated PDF via PuppeteerSharp |
| `NewsletterView.vue` | 804 | Quarterly newsletter: trend lines, findings, CA summary, chart.js charts | Calls `getClient().getAuditReport()` via own `getClient()` at line 237 | Client-rendered HTML — `window.print()` only, not email-deliverable |
| `QuarterlySummaryView.vue` | 428 | Simplified quarterly summary: KPIs, division breakdown, top NCs | Calls `getClient().getAuditReport()` via own `getClient()` at line 248 | Client-rendered HTML — `window.print()` only |
| `ReportGalleryView.vue` | 432 | Template gallery: pick a predefined report type, set date range, generate PDF | Calls `generateReport()` backend handler | Server-generated PDF via PuppeteerSharp |
| `ScheduledReportsView.vue` | 304 | Manage automated report schedules: CRUD, active/inactive toggle | Own `getClient()` at line 213 | Configures `ScheduledReport` records |
| `AuditsByEmployeeView.vue` | 242 | Filtered audit list grouped by employee | Uses `getAuditReport()` with auditor filter param | Interactive table — not a deliverable report |
| `useReportEngine.ts` | ~600 | PDF assembly logic for Composer: section rendering, SVG chart building, HTML template construction | 2x `new AuditClient()` (lines 484, 534) | HTML string → backend → PDF bytes |
| `useReportDraft.ts` | ~100 | CRUD for ReportDraft model | 1x `new AuditClient()` (line 95) | `ReportDraft` persistence |
| `NewsletterTemplateEditorView.vue` | ~300 | Edit newsletter branding (colors, logo, tagline) | 2x `new AuditClient()` (lines 291, 385) | `NewsletterTemplate` record |

### Backend Services

| Service/Handler | File | Role |
|---|---|---|
| `GenerateReport` | `Api/Domain/Audit/Reports/GenerateReport.cs` | MediatR handler: loads report data, uses `SvgChartBuilder`, assembles HTML, calls `IPdfGeneratorService` |
| `PdfGeneratorService` | `Api/Services/PdfGeneratorService.cs` | PuppeteerSharp wrapper — HTML → PDF bytes via headless Chromium |
| `SvgChartBuilder` | `Api/Services/SvgChartBuilder.cs` | Server-side SVG charts for PDF reports |
| `ScheduledReportService` | `Api/BackgroundServices/ScheduledReportService.cs` | Background service: every 5 min, fires due `ScheduledReport` records → `GenerateReport` → email |
| `AuditSummaryService` | `Api/Services/AuditSummaryService.cs` | AI summary generation on audit submission |

### Data Models

| Model | File | Purpose |
|---|---|---|
| `ScheduledReport` | `Data/Models/Audit/ScheduledReport.cs` | Recurring delivery config: frequency, recipients, template type, date range preset |
| `ReportDraft` | `Data/Models/Audit/ReportDraft.cs` | User-created report draft in the Composer |
| `NewsletterTemplate` | `Data/Models/Audit/NewsletterTemplate.cs` | Newsletter branding configuration |

---

## 2. PROBLEM STATEMENT

**Problem 1: Data divergence between report views.**

`ReportsView.vue`, `NewsletterView.vue`, and `QuarterlySummaryView.vue` all call `getAuditReport()` independently. Each has its own date state. `NewsletterView` uses `selectedQuarter`/`selectedYear` logic to compute a date range. `QuarterlySummaryView` does the same but with different quarter boundary calculation. `ReportsView` uses a `Calendar` datepicker returning a `Date` object. The same "Q1 2026" period produces different date strings from each view, resulting in potentially different data for the same logical period.

**Problem 2: Newsletter and Quarterly Summary cannot be scheduled or emailed.**

`ScheduledReportService.cs` uses `IPdfGeneratorService` which renders HTML to PDF. But `NewsletterView` and `QuarterlySummaryView` are client-side rendered with Chart.js charts and `window.print()`. There is no server-side HTML template for either. When a user configures a scheduled "Newsletter" report, the scheduler has no way to produce the newsletter content — it only has the `GenerateReport` MediatR handler which produces a different, Composer-style report.

**Problem 3: No unified report run log.**

When a scheduled report fires, there is no record of: what was generated, what date range was used, how many pages, which recipients received it, whether the email succeeded or bounced. `ScheduledReport.LastRunAt` is updated on success, but there is no per-delivery log. If a report fails, there is no history to investigate.

**Problem 4: Two visual systems for report output.**

Composer reports use `SvgChartBuilder` server-side charts and a structured HTML template. Newsletter and Quarterly Summary use client-side Chart.js charts and hand-written CSS. These produce visually inconsistent output when printed or converted to PDF.

**Problem 5: Report Composer has no first-use guidance.**

Evidence from `90-report-composer-base-empty.png`. On first load, the composer shows no content, no draft selected, no default division. The user does not know what to do. The only hint is "Select division…" in the dropdown.

---

## 3. PROPOSED UNIFIED REPORT MODEL

### `ReportDefinition` — the canonical report descriptor

```typescript
// webapp/src/modules/audit-management/features/reports/types/ReportDefinition.ts

export type ReportTemplateId =
    | 'quarterly-summary'
    | 'newsletter'
    | 'annual-review'
    | 'post-audit-summary'
    | 'ncr-report'
    | 'executive-dashboard'
    | 'ca-aging'
    | 'auditor-performance';

export type DeliveryMethod = 'download' | 'email' | 'scheduled';
export type DateRangePreset = 'last30days' | 'thisquarter' | 'lastquarter' | 'thisyear' | 'lastyear' | 'custom';

export interface ReportDefinition {
    templateId: ReportTemplateId;
    title: string;
    divisionId: number | null;         // null = all divisions
    dateRangePreset: DateRangePreset;
    customDateFrom?: string | null;    // ISO date, only when preset = 'custom'
    customDateTo?: string | null;
    primaryColor?: string | null;      // hex
    recipients?: string[];             // for email delivery
    includeCharts?: boolean;
    includeFindings?: boolean;
    includeCorrectiveActions?: boolean;
    includeAuditorPerformance?: boolean;
}
```

This structure maps 1:1 to the existing `ScheduledReport` model and to the `GenerateReportRequest` in the backend. Adopting this as the single input shape allows all report triggers (on-demand, scheduled, gallery) to share one code path.

---

## 4. PROPOSED REPORT ENGINE ARCHITECTURE

### Target Structure

```
webapp/src/modules/audit-management/features/reports/
├── composables/
│   ├── useAuditReportData.ts          ← NEW: shared data source for all report views
│   ├── useReportCharts.ts             ← EXTRACT from ReportsView.vue
│   ├── useReportDrilldown.ts          ← EXTRACT from ReportsView.vue
│   ├── useReportKpis.ts               ← EXTRACT from ReportsView.vue
│   ├── useReportEngine.ts             ← EXISTING: keep, cleanup AuditClient calls
│   └── useReportDraft.ts              ← EXISTING: keep
├── services/
│   └── ReportDeliveryService.ts       ← NEW: wraps generateReport + download/email trigger
├── types/
│   └── ReportDefinition.ts            ← NEW: canonical report descriptor type
└── views/
    ├── ReportsView.vue                ← SHRINK to ~250 lines
    ├── NewsletterView.vue             ← PORT to AppLayout + useAuditReportData
    ├── QuarterlySummaryView.vue       ← PORT to AppLayout + useAuditReportData
    ├── ReportComposerView.vue         ← CLEAN UP inputs, add empty state
    ├── ReportGalleryView.vue          ← keep
    ├── ScheduledReportsView.vue       ← keep
    └── AuditsByEmployeeView.vue       ← keep
```

### `useAuditReportData.ts` — the shared composable

```typescript
// Purpose: single source of truth for AuditReportDto.
// Used by ReportsView, NewsletterView, QuarterlySummaryView.
// Owns filter state, date range computation, and load/reload.

export function useAuditReportData() {
    const service = useAuditService();
    const report = ref<AuditReportDto | null>(null);
    const loading = ref(false);
    const filterDivisionId = ref<number | null>(null);
    const filterStatus = ref<string | null>(null);
    const filterDateFrom = ref<string | null>(null);  // ISO date string
    const filterDateTo = ref<string | null>(null);

    // Shared quarter → date range conversion
    function setQuarterRange(year: number, quarter: 1 | 2 | 3 | 4) {
        const quarterStarts = ['-01-01', '-04-01', '-07-01', '-10-01'];
        const quarterEnds   = ['-03-31', '-06-30', '-09-30', '-12-31'];
        filterDateFrom.value = `${year}${quarterStarts[quarter - 1]}`;
        filterDateTo.value   = `${year}${quarterEnds[quarter - 1]}`;
    }

    async function loadReport() {
        loading.value = true;
        try {
            report.value = await service.getAuditReport(
                filterDivisionId.value,
                filterStatus.value,
                filterDateFrom.value,
                filterDateTo.value,
                null,
            );
        } finally {
            loading.value = false;
        }
    }

    return { report, loading, filterDivisionId, filterStatus, filterDateFrom, filterDateTo, setQuarterRange, loadReport };
}
```

Both `NewsletterView` and `QuarterlySummaryView` call `setQuarterRange(year, quarter)` before `loadReport()`. This guarantees identical date computation across all views.

---

## 5. PROPOSED DELIVERY MODEL

### Three Delivery Triggers

| Trigger | How | Who Initiates | Backend Path |
|---|---|---|---|
| On-demand download | User clicks "Generate PDF" | User | POST `/v1/reports/generate` → `GenerateReport` → PDF bytes → download |
| On-demand email | User enters recipients + clicks "Send Report" | User (admin) | POST `/v1/reports/generate` + `IEmailService.SendAsync()` |
| Scheduled delivery | `ScheduledReportService` background job | System (cron) | `ScheduledReport` record → `GenerateReport` → `IEmailService.SendAsync()` |

All three triggers share the same `GenerateReport` MediatR handler. The difference is only in who collects recipients and when the trigger fires.

### Adding Newsletter and Quarterly Summary to the Scheduler

**Step 1**: Create server-side HTML templates for `newsletter` and `quarterly-summary` in the `GenerateReport` handler (currently only handles the Composer-style templates).

The `GenerateReport` handler should have a switch on `TemplateId`:
```csharp
var html = request.TemplateId switch {
    "newsletter"           => await BuildNewsletterHtmlAsync(data, request, ct),
    "quarterly-summary"    => await BuildQuarterlySummaryHtmlAsync(data, request, ct),
    "annual-review"        => await BuildAnnualReviewHtmlAsync(data, request, ct),
    "post-audit-summary"   => await BuildPostAuditSummaryHtmlAsync(data, request, ct),
    _                      => await BuildComposerHtmlAsync(data, request, ct),
};
```

**Step 2**: Each `Build*HtmlAsync` method produces an HTML string using `SvgChartBuilder` for charts. The newsletter HTML mirrors the `NewsletterView.vue` layout but is server-generated.

**Step 3**: The `ScheduledReportService` passes the generated PDF bytes to `IEmailService.SendAsync()` with the PDF attached.

---

## 6. REPORT TYPES TO SUPPORT

| Report Type | TemplateId | Key Sections | Scheduler Support | Current Status |
|---|---|---|---|---|
| Quarterly Summary | `quarterly-summary` | KPIs, division health, top NCs by section, CA aging | Must add | Client-render only |
| Newsletter | `newsletter` | Cover, KPI grid, conformance trend chart, top findings, open CAs, auditor performance | Must add | Client-render only |
| Compliance Summary | `compliance-summary` | Division compliance status, overdue audit list, score targets vs actuals | Must add | Not yet built |
| Executive Dashboard | `executive-dashboard` | High-level: total audits, avg score, NC trend, overdue CA count, by-division summary | Exists in gallery | Partial |
| Audit History | `annual-review` | All audits in period, scores, NC counts, auditor breakdown | Exists | Composer only |
| CA Aging Report | `ca-aging` | All open CAs by age bucket (0-7, 8-14, 15-30, 30+), by division, by assignee | Exists in gallery | Composer only |
| Auditor Performance | `auditor-performance` | Audit count, avg score, NC rate, by auditor, by period | Not yet built | Data in ReportsView Performance tab |
| Post-Audit Summary | `post-audit-summary` | Single audit: score, findings, CAs, auditor notes | Exists | Composer only |

---

## 7. REPORT HISTORY / RUN LOG / DELIVERY LOG MODEL

**New Model: `ReportRunLog`**

```csharp
// Data/Models/Audit/ReportRunLog.cs
public class ReportRunLog
{
    public int Id { get; set; }
    public int? ScheduledReportId { get; set; }  // null = on-demand
    public string TemplateId { get; set; } = null!;
    public int? DivisionId { get; set; }
    public string DateFrom { get; set; } = null!;
    public string DateTo { get; set; } = null!;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string GeneratedBy { get; set; } = null!;   // user email or "SYSTEM"
    public int FileSizeBytes { get; set; }
    public string? DeliveryMethod { get; set; }        // "download" | "email" | "scheduled"
    public string? RecipientsJson { get; set; }        // JSON array of email addresses
    public bool DeliverySuccess { get; set; }
    public string? DeliveryError { get; set; }
    public int? AuditCount { get; set; }               // how many audits were in the report
    public ScheduledReport? ScheduledReport { get; set; }
    public Division? Division { get; set; }
}
```

This model answers: "When was this report last generated? By whom? To whom? Did it succeed? How many audits were in scope?"

The `ScheduledReportsView` should show a "Run History" column with the last 3 delivery timestamps and success/failure status.

---

## 8. SPECIFIC FILE CHANGES

### Delete

None. No files should be deleted in the first phase. All 7 report views stay — the goal is to share their data layer and fix their visual inconsistencies.

### Create

- `webapp/src/modules/audit-management/features/reports/composables/useAuditReportData.ts` — shared data composable
- `webapp/src/modules/audit-management/features/reports/composables/useReportCharts.ts` — extracted from ReportsView.vue lines ~1100-1600
- `webapp/src/modules/audit-management/features/reports/composables/useReportDrilldown.ts` — extracted from ReportsView.vue lines ~870-940
- `webapp/src/modules/audit-management/features/reports/composables/useReportKpis.ts` — extracted from ReportsView.vue lines ~940-1000
- `webapp/src/modules/audit-management/features/reports/services/ReportDeliveryService.ts` — wraps PDF download + email trigger
- `webapp/src/modules/audit-management/features/reports/types/ReportDefinition.ts` — canonical type
- `Data/Models/Audit/ReportRunLog.cs` — delivery history model
- `Data/Migrations/<timestamp>_AddReportRunLog.cs` — via `/migrate AddReportRunLog`
- `Api/Domain/Audit/Reports/GetReportHistory.cs` — MediatR handler for run log

### Modify

- `ReportsView.vue` — shrink from 1,658 to ~250 lines by extracting 4 composables. View becomes orchestration-only.
- `NewsletterView.vue` — port to AppLayout, replace all raw CSS with Tailwind, call `useAuditReportData`
- `QuarterlySummaryView.vue` — port to AppLayout, replace all raw CSS with Tailwind, call `useAuditReportData`
- `ReportComposerView.vue` — replace raw `<select>`/`<input>` with PrimeVue components, add EmptyState
- `ScheduledReportsView.vue` — add run history column
- `useReportEngine.ts` — replace 2x `new AuditClient()` with `useAuditService()`
- `useReportDraft.ts` — replace 1x `new AuditClient()` with `useAuditService()`
- `Api/Domain/Audit/Reports/GenerateReport.cs` — add newsletter + quarterly-summary template branches
- `Api/BackgroundServices/ScheduledReportService.cs` — write to `ReportRunLog` on success/failure
- `tailwind.config.js` — add color tokens
- `webapp/src/router/index.ts` — fix NewsletterView and QuarterlySummaryView to route inside AppLayout

---

## 9. MIGRATION PATH

### Phase 1 — Foundation (no user-visible changes, 1 week)

1. Create `useAuditService()` composable — replaces all 28 `new AuditClient()` calls
2. Add Tailwind color tokens to `tailwind.config.js`
3. Create `useAuditReportData.ts` — shared data composable
4. Extract `useReportCharts.ts`, `useReportDrilldown.ts`, `useReportKpis.ts` from `ReportsView.vue`
5. Shrink `ReportsView.vue` to ~250 lines by calling the new composables
6. Verify: all existing E2E tests pass, ReportsView behavior unchanged

### Phase 2 — Newsletter and Quarterly Summary (1–2 weeks)

1. Port `NewsletterView.vue` to AppLayout + Tailwind + `useAuditReportData`
2. Port `QuarterlySummaryView.vue` to AppLayout + Tailwind + `useAuditReportData`
3. Fix the router to route both views inside AppLayout
4. Add `@media print` CSS to preserve print layout
5. Verify: newsletter still prints correctly, data matches ReportsView for same period

### Phase 3 — Composer and Delivery (1–2 weeks)

1. Replace raw `<select>`/`<input>` in `ReportComposerView.vue` with PrimeVue components
2. Add EmptyState and loading guidance to ReportComposerView
3. Create `ReportDeliveryService.ts`
4. Add `ReportRunLog` model + migration
5. Update `ScheduledReportService.cs` to log runs
6. Add newsletter + quarterly-summary branches to `GenerateReport` handler

### Phase 4 — Scheduler Enhancement (1 week)

1. Wire newsletter and quarterly-summary to the scheduler
2. Add run history to `ScheduledReportsView`
3. Verify: scheduled newsletter delivery produces correct PDF and logs the run

### What Does NOT Break During Migration

- The audit form workflow (completely separate from reporting)
- The corrective actions workflow (completely separate)
- Existing scheduled reports that use `annual-review`, `quarterly-summary` (existing Composer-style templates continue to work until Phase 3)
- All E2E tests (the data endpoints do not change, only who calls them)
- The `PdfGeneratorService` and `SvgChartBuilder` (no changes required in Phase 1-2)
