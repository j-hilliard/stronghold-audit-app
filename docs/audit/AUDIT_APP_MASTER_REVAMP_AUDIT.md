# AUDIT APP MASTER REVAMP AUDIT
Generated: 2026-04-28 | Auditor: Senior Staff Engineer + Principal Architect

---

## 1. EXECUTIVE SUMMARY

The Stronghold Compliance Audit App is a working, data-connected compliance platform with a sound Vue 3 + .NET 8 + EF Core stack. The domain model is coherent (34 audit models, 3 migrations, proper FK structure), the audit workflow from creation through submission through CA assignment is end-to-end functional, and key safety features like life-critical overrides, repeat finding detection, and section N/A defensibility are implemented. However, the codebase is at an inflection point: it has grown feature-by-feature without architectural guardrails, and the result is a pattern of "views doing everything." ReportsView.vue is 1,658 lines of mixed template, chart config, drill logic, localStorage, and direct API calls. AuditReviewView.vue is 1,151 lines handling 4 separate workflows. CorrectiveActionsView.vue is 918 lines with its own embedded getClient() factory. There are 28 direct `new AuditClient(...)` instantiations scattered across 15 different files. The reporting section is not one coherent system — it is 7 separate standalone views (ReportsView, ReportComposerView, NewsletterView, QuarterlySummaryView, ReportGalleryView, ScheduledReportsView, AuditsByEmployeeView) with no shared data layer and inconsistent visual languages. This is salvageable with targeted modernization — not a re-architecture from scratch. The backend is clean and follows the MediatR/handler pattern consistently. The work required is a frontend layer restructure: extracting logic out of views into composables and a service layer, unifying the reporting subsystem under a single composable engine, and codifying the design system into Tailwind tokens. No data schema changes are required for the first sprint.

---

## 2. CURRENT-STATE SCORECARD

| Dimension | Score | Evidence |
|---|---|---|
| Architecture discipline | 5/10 | Clean MediatR backend; frontend has 28 direct AuditClient instantiations across 15 files with no service layer abstraction. `getClient()` is literally copy-pasted in every view. |
| Feature structure | 7/10 | Good feature-folder organization under `features/`. Reports sub-features are all crammed into one `reports/views/` folder with 7 views, no composable isolation per feature. |
| Design system maturity | 3/10 | `tailwind.config.js` extends nothing — zero custom tokens. Colors are hardcoded throughout (`bg-slate-800`, `border-red-700/50`, `text-emerald-400`). No spacing scale, no shadow tokens, no focus tokens. Two competing styling paradigms: Tailwind + PrimeVue + raw CSS in some views (NewsletterView, QuarterlySummaryView use plain `.picker`, `.kpi-grid`, `.kpi-box` CSS classes). |
| UI consistency | 5/10 | BasePageHeader is used consistently across main views. But NewsletterView uses a completely different header pattern (`.toolbar` CSS class). QuarterlySummaryView uses `.picker`. ReportComposerView uses raw `<select>` elements instead of PrimeVue Dropdown. Inconsistent filter bar patterns across pages. |
| Reporting architecture | 4/10 | 7 separate view files, none sharing a data composable. ReportsView loads via `getClient().getAuditReport(...)` directly inline. NewsletterView loads via its own `getClient()`. QuarterlySummaryView has its own `getClient()`. These three views can call the same API endpoint with different date parameters and produce inconsistent results. |
| Workflow automation | 7/10 | CaReminderService runs daily. ScheduledReportService runs every 5 minutes. Submit triggers email routing. AI summary is generated at submission. Distribution email with attachment selection is implemented. These are real automations. Missing: escalation chain, audit scheduling, CA auto-escalation beyond reminder. |
| Email/notification readiness | 6/10 | EmailService with dry-run/dev-redirect pattern is well-built. SMTP configured. CA reminders with deduplication via CaNotificationLog exist. Distribution preview before send is implemented. Missing: in-app notification center, real-time SignalR alerts for overdue CAs, audit assignment notifications. |
| Mobile/tablet readiness | 3/10 | Some responsive classes used (`md:grid-cols-4`, `sm:grid-cols-5`). Filter bars wrap on small screens. AuditFormView is functional on tablet. But QuarterlySummaryView and NewsletterView have no responsive behavior at all. The audit form section layout and question rows are not touch-optimized. No offline capability. |
| Test/QA readiness | 6/10 | 20+ Playwright E2E test files covering core flows, visual, API guard, navigation stress. Live screenshot sweep produced 62 PNGs. QA baseline script exists in package.json. Missing: unit tests for store logic (calculateScore, visibleSections, logic rules), contract tests for the API client DTOs, component-level tests. |
| Enterprise readiness | 5/10 | Azure AD auth, role-based routing, audit trail logging, email routing rules, template versioning — all real enterprise features. Blocked by: no service layer abstraction for testing, no error boundary components, no offline capability, no SLA escalation chain, no multi-tenant scoping, handwritten API client with no code generation. |

---

## 3. WHAT IS ALREADY GOOD

**1. MediatR domain handler pattern (backend)**
Every controller action dispatches to a handler. `Api/Controllers/AuditController.cs` is thin by design — it just maps HTTP verbs to MediatR sends. The handlers in `Api/Domain/Audit/` own all business logic. This is correct and should be preserved.

**2. auditStore.ts — logic is correctly store-owned**
The audit form store (`webapp/src/modules/audit-management/stores/auditStore.ts`) owns `calculateScore`, `visibleSections` (logic rule evaluation), `saveDraftToLocalStorage`, `initResponsesFromTemplate`, and the 800ms autosave debounce. This is the right approach — view-layer state belonging in the store. The logic rule evaluation at lines 171–207 is a good piece of work.

**3. AuditFormView.vue is appropriately lean (372 lines)**
After all the prior work, the audit form view correctly delegates everything to the store. The view contains only: template rendering, section refs for collapse/expand, onMounted/onBeforeRouteUpdate hooks, and the submit/delete confirmation dialogs. This is the model all other views should follow.

**4. EmailService dry-run/dev-redirect pattern**
`Api/Services/EmailService.cs` has a production-safe dry-run mode, dev redirect interception, and SMTP config abstraction. This is the right pattern for a system that cannot accidentally email real users in dev.

**5. BasePageHeader component**
`webapp/src/components/layout/BasePageHeader.vue` is adopted consistently across 12 of the 14 primary views. It has a slot for action buttons, uses the consistent icon+title+subtitle pattern, and has the correct border treatment.

**6. CaReminderService deduplication**
`Api/Services/CaReminderService.cs` uses `CaNotificationLog` to deduplicate — one log entry per CA per type per calendar day. This prevents notification spam on long-running overdue CAs.

**7. Section N/A defensibility model**
`AuditSectionNaOverride` is a first-class model in the domain. The store tracks it in a Map keyed by sectionId, persists it to localStorage draft, saves it to the server, and the scoring computed excludes those sections. The data model supports legal defensibility for N/A decisions.

**8. Repeat finding detection**
`getClient().getRepeatFindings(id)` is called non-blockingly on audit load and the result set is used for badge display in both AuditReviewView and the form. This is a differentiating enterprise feature that competitors do not always implement.

**9. CorrectiveActionsView pagination and bulk operations**
The CA view has server-side pagination, bulk close/reassign/status change, priority and source filters, overdue toggle, and closure photo upload with drag-and-drop. These are mature features.

**10. PdfGeneratorService using PuppeteerSharp**
The PDF backend uses a headless Chromium via PuppeteerSharp with browser instance reuse. This produces high-fidelity PDFs from HTML and is deployed correctly as a singleton. The ScheduledReportService integrates with it via MediatR.

---

## 4. TOP ARCHITECTURE PROBLEMS

| Priority | File/Area | Problem | Impact | Recommended Refactor | Why Now |
|---|---|---|---|---|---|
| P0 | 15 view/composable files | `new AuditClient(apiStore.api.defaults.baseURL, apiStore.api)` is copy-pasted 28 times. Every view instantiates its own client. There is no shared factory. | No single place to change base URL, add auth headers, add retry logic, or add request tracing. | Create `webapp/src/services/auditService.ts` as a composable singleton: `export function useAuditService() { const apiStore = useApiStore(); return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api); }`. All views/composables call `useAuditService()` instead of `new AuditClient(...)`. Remove all 28 inline instantiations. | Before any new feature. Every new feature adds another copy. |
| P0 | `ReportsView.vue` (1,658 lines) | This single file contains: 5 tabs of HTML (overview/analysis/action-items/history/performance), 4 chart config computeds, 6 drill-down filter functions, localStorage persistence for 2 different keys, KPI count-up animation logic, a `getClient()` factory, `loadReport()`, `loadComplianceStatus()`, section filter state, trend delta calculations, auditor stats computed, division health card computed, quarterly trend computed, and all 5 tab display computeds. | Untestable, unmaintainable, any change risks breaking an unrelated tab. | Split into: `ReportsView.vue` (~200 lines, orchestration only), `useReportData.ts` (data fetch, filters, load/reload), `useReportCharts.ts` (all chart data and options computeds), `useReportDrilldown.ts` (drill-down state: auditor, location, nc-only, warn-only), `useReportKpis.ts` (count-up animations, KPI visibility, localStorage). | P0 — this is the highest-traffic view. |
| P1 | `AuditReviewView.vue` (1,151 lines) | Contains 4 distinct workflows: (1) Review display with score ring + benchmark; (2) Distribution recipient management with search/filter dialog; (3) Distribution email preview-then-send; (4) CA assign + close modals. Also has `getClient()` inline at line 1015, `buildFallbackDistributionPreview()` (95 lines of HTML string building), and `escapeHtml()` inline. | Any change to one workflow risks all four. The HTML-building function is a security concern (XSS if ever used unsanitized). | Extract: `useReviewDistribution.ts` (recipient state, add/remove, filtering, dialog state), `useReviewEmail.ts` (preview fetch, fallback, send, subject edit), `useReviewCaActions.ts` (assign/close modals, saving). Move `buildFallbackDistributionPreview` to `ReviewEmailService.ts`. | P1 — distribution email path has backward-compat fallback code that is hard to reason about. |
| P1 | `CorrectiveActionsView.vue` (918 lines) | View owns: data fetching with full pagination params, 5 KPI count-up animations, 4 filter dropdowns, 2 bulk action dialogs, 1 edit dialog, 1 close dialog with photo upload, Excel export via direct `apiStore.api.get(...)` (not even using the client), and its own `getClient()` factory. | Export path bypasses the AuditClient entirely — `apiStore.api.get('/v1/corrective-actions/export', ...)` at line 851. This is the only place in the app that bypasses the client. | Extract: `useCaList.ts` (pagination, filters, load, KPI counts), `useCaDialogs.ts` (edit/close/bulk dialog state and submit logic), `CaExportService.ts` (blob download via client method). Add `exportCorrectiveActions()` to AuditClient. | P1 — the raw axios bypass is a regression risk. |
| P1 | Reporting subsystem (7 independent views) | `ReportsView`, `NewsletterView`, `QuarterlySummaryView`, `ReportComposerView`, `ReportGalleryView`, `ScheduledReportsView`, `AuditsByEmployeeView` all independently call `getAuditReport()` or their own report endpoints. `NewsletterView` and `QuarterlySummaryView` have completely different visual styles (raw CSS classes) from the rest of the app. | Users can see different data in the newsletter vs the dashboard for the same period because they each call different endpoints with different default date ranges. No shared source of truth. | See Section 6 for the full reporting unification plan. | P1 — data divergence undermines user trust. |
| P2 | `tailwind.config.js` — zero tokens | The config is the Tailwind default with no extensions. Color tokens like `slate-700`, `emerald-900`, `red-950/40`, `blue-700/30` are scattered across every file with hardcoded opacity modifiers. | Any brand color change requires search-and-replace across 50+ files. Dark mode is not a CSS-variable theme — it is locked into the current dark palette via hardcoded classes. | Define in `tailwind.config.js`: `colors.surface.default = '#1e293b'`, `colors.surface.card = '#0f172a'`, `colors.brand.*`, `colors.status.conforming/nonConforming/warning/na`. Replace hardcoded color classes with semantic tokens. | P2 — do this before any new UI work, not after. |
| P2 | `adminStore.ts` mixes template admin + email routing + user role management | The admin store has 3 unrelated domains: template version editor operations, email routing rules, and user audit role management. All share a single `loading` and `saving` ref, which means a template save blocks the email routing save indicator. | Cannot add proper per-operation loading state without a full refactor. | Split into `useTemplateEditor.ts` composable (draft operations), `useEmailRouting.ts` composable (routing rules), `useAuditUserRoles.ts` composable. Delete adminStore entirely. | P2 — before building the template editor further. |

---

## 5. TOP UX/UI PROBLEMS

| Priority | Route/Page | User Problem | Visual Problem | Recommended Fix |
|---|---|---|---|---|
| P0 | `/audit-management/newsletter` and `/audit-management/reports/quarterly-summary` | These are standalone print views routed outside the AppLayout. They use raw CSS classes (`.toolbar`, `.picker`, `.kpi-grid`, `.kpi-box`) instead of the app's Tailwind design system. A user navigating to the newsletter sees a completely different-looking application. | Raw CSS with no dark-mode support, no responsive layout, no BasePageHeader, no PrimeVue components. Evidence: `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP.md` screenshot `120-newsletter-base.png`. | Port both views to use AppLayout with BasePageHeader, Tailwind classes, and the PrimeVue Card/DataTable system. The print-only toggle should be handled via `@media print` CSS, not separate routes. |
| P0 | `/audit-management/reports/composer` | The Report Composer page loads completely blank on entry — no default division selected, no default date range, no hint of what to do first. Uses raw `<select>` elements instead of PrimeVue Dropdown, and inline `class="bg-slate-700 border border-slate-600..."` applied directly to native elements. | Visual mismatch with the rest of the app. Evidence: `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP.md` screenshot `90-report-composer-base-empty.png`. | Add an empty state with a clear call to action ("Select a division and date range to begin"). Replace raw `<select>` and `<input>` elements with PrimeVue Dropdown, Calendar, InputText. |
| P1 | `/audit-management/audits/:id` (AuditFormView) | The score bar at the bottom of the page is a floating sticky element that can overlap content on mobile. The "Submit for Review" button is in the page header which disappears when scrolling down on mobile — the primary action is not reachable after the user starts filling out the form. | On small viewports, the header action buttons wrap to a second row and can overlap the audit content. | Move primary actions (Save Draft, Submit) to a sticky bottom action bar on mobile (`md:hidden` pattern). Keep header actions for desktop. |
| P1 | `/audit-management/reports` (ReportsView) | The Reports menu in the header opens a Menu popup to navigate to composer/gallery/quarterly. This is confusing — the "Reports" button in the Reports page header navigates away from Reports. The tab bar (Overview/Analysis/Action Items/History/Performance) is not surfaced prominently and users miss the 4 analytical tabs. | The `Reports` button and `Report Composer`, `Generate Report` sub-items suggest Reports is the parent and Composer is a child — but clicking navigates to a completely different page. | Rename the header button to "Export / Print" or "More Reports". Make the tab bar more visually prominent with a stronger active indicator. |
| P2 | `/audit-management/corrective-actions` | The filter bar has 5 dropdowns + a search + an overdue toggle on one row. On tablet (768px), these wrap awkwardly. The `Status` and `Source` dropdowns use the same width (`w-40`), but Source only has 2 options and looks sparse. | Filter rows wrap to 2 or 3 lines on tablet. The filter labels (`Division`, `Status`, `Source`, `Priority`) use `text-xs text-slate-400` but the Dropdown placeholder uses the PrimeVue default text which does not inherit the small size. | Collapse filters into a collapsible filter panel on mobile. Use a `FilterBar` shared component so this pattern is defined once. |
| P2 | `/audit-management/admin/audit-log` | Evidence from QA sweep (`78-admin-audit-log-action-tab.png`, `80-admin-audit-log-expanded-row.png`) confirms a runtime error on the Action tab row expansion. | The admin audit log has two tabs (Action / Trail) and a row-expand pattern. The expansion fails with a visible error. | Investigate AdminAuditLogView.vue — likely a null reference on the expanded row DTO. Fix the null guard in the row expand template slot. |
| P3 | All pages | Empty states are inconsistent. ReportsView shows "No report data available." (plain text, no icon). CorrectiveActionsView shows a centered icon + two lines. AuditDashboardView shows an icon + two lines. There is no shared `EmptyState` component. | Three different visual treatments for the same UX pattern. | Create `webapp/src/components/feedback/EmptyState.vue` with `icon`, `title`, `description`, and optional `action` slot props. Use it on all pages. |

---

## 6. REPORTING SYSTEM AUDIT

### Current State Map

| File | Lines | Role | Data Source | Shares with others? |
|---|---|---|---|---|
| `ReportsView.vue` | 1,658 | Primary dashboard: KPIs, 5 tabs, charts, drill-down, history, performance, action items | `getAuditReport()`, `getComplianceStatus()` | No — has own getClient() |
| `ReportComposerView.vue` | 710 | Draft-based report builder: selects sections, exports PDF | `getReportDrafts()`, `generateReport()` | Partial — uses `useReportDraft.ts` and `useReportEngine.ts` |
| `NewsletterView.vue` | 804 | Standalone print: quarterly newsletter with trend lines | `getAuditReport()` + own date logic | No — own getClient(), own CSS |
| `QuarterlySummaryView.vue` | 428 | Standalone print: simplified quarterly summary | Same endpoint, different params | No — own getClient(), own CSS |
| `ReportGalleryView.vue` | 432 | Template gallery: pick a report type and generate PDF | `generateReport()` backend call | Partial — uses some shared composables |
| `ScheduledReportsView.vue` | 304 | Manage scheduled report deliveries | `getScheduledReports()`, `upsertScheduledReport()` | No |
| `AuditsByEmployeeView.vue` | 242 | Filtered view: audits grouped by employee | `getAuditReport()` with auditor filter | No |
| `useReportEngine.ts` (composable) | ~600 | PDF section rendering logic for the Composer | PDF generation pipeline | Used only by Composer |
| `useReportDraft.ts` (composable) | ~100 | Draft CRUD for the Composer | ReportDraft model | Used only by Composer |

### Identified Overlaps and Problems

1. **Three views call the same `getAuditReport()` endpoint independently**: ReportsView, NewsletterView, QuarterlySummaryView. Each has its own date range state. A user can see a different conformance score on the Reports dashboard vs the Newsletter for the "same" quarter because one uses Date objects and one uses string comparison.

2. **Two entirely separate styling systems**: ReportsView/ScheduledReports use PrimeVue + Tailwind. NewsletterView/QuarterlySummaryView use raw CSS. They are in the same `reports/views/` folder but are visually different applications.

3. **The "Reports" route is actually the analytics dashboard**: `/audit-management/reports` is named "Dashboard" in the breadcrumb and "Audit Dashboard" in the page title. The tab title says "Dashboard" in `router/index.ts`. Yet the route segment is `reports`. This is mislabeled — it is the analytics dashboard, not a report viewer.

4. **ReportComposer's `useReportEngine.ts`** (~600 lines) contains 2 direct `new AuditClient(...)` calls. The engine builds SVG charts via `SvgChartBuilder.cs` on the backend and assembles HTML sections. This is the only part of the reporting system that produces structured, server-rendered output. The Newsletter and Quarterly Summary are purely client-rendered HTML printed via `window.print()` — they are not PDFs and cannot be emailed.

5. **ScheduledReports currently only delivers Composer-style PDFs** (via `ScheduledReportService.cs` using `GenerateReport` MediatR handler). The Newsletter and Quarterly Summary cannot be scheduled because they are client-rendered.

### Verdict: Salvage and Unify

Do not replace. The underlying data model is correct. The `ScheduledReport` model is complete. The `PdfGeneratorService` works. The `useReportEngine.ts` foundation is usable. The problem is fragmentation, not a fundamental architecture failure.

### Target Reporting Model

**Single report data composable** (`useAuditReportData.ts`):
- Owns one `report` ref of type `AuditReportDto`
- Owns one `filterState` (division, status, from, to, section)
- Provides `loadReport()` and `refreshReport()` using `useAuditService()`
- Used by ReportsView, NewsletterView, QuarterlySummaryView — same data, consistent results

**Separate display concerns**:
- ReportsView: analytics dashboard (KPIs, charts, tabs) — keep as a standalone interactive page
- NewsletterView: print-optimized template — port to AppLayout with `@media print` CSS, pull data from `useAuditReportData.ts`
- QuarterlySummaryView: simplified print view — same approach

**Remove the naming confusion**: rename the `/reports` route to `/dashboard` or keep `reports` but fix the `title` meta to say "Analytics Dashboard" not "Dashboard".

---

## 7. AUTOMATION / EMAIL / NOTIFICATION AUDIT

### What Is Truly Automated

| Feature | File Evidence | Genuinely Automated? |
|---|---|---|
| Audit submission email | `Api/Domain/Audit/Audits/SubmitAudit.cs` via EmailService | Yes — fires on submit, uses routing rules |
| Distribution email on review | `AuditReviewView.vue` → `store.sendDistributionEmail()` → backend | Partially — requires manual trigger by admin |
| CA due-soon/overdue reminder | `Api/Services/CaReminderService.cs` — BackgroundService, runs at midnight | Yes — with dedup via CaNotificationLog |
| Scheduled report delivery | `Api/BackgroundServices/ScheduledReportService.cs` — runs every 5 minutes | Yes — fires on NextRunAt, generates PDF via PuppeteerSharp |
| AI audit summary on submission | Called in SubmitAudit handler via AuditSummaryService | Yes — non-blocking, stored in AiSummary field |

### What Only Looks Automated

| Feature | Problem |
|---|---|
| "Send Distribution Email" | This is a manual button in AuditReviewView. The distribution only fires when an admin clicks it. Despite the routing rules being configured, no email is sent on audit status change — only on explicit send action. |
| Scheduled reports | Work correctly, but delivery only covers Composer-style PDFs. Newsletter and Quarterly Summary are client-rendered and cannot be delivered by the background service. |

### What Is Still Manual

- Audit assignment to a division/auditor — no scheduled audit calendar
- CA escalation beyond the initial reminder — no second notice if overdue > 14 days
- Reopen notification to the original auditor — only the admin can reopen; no notification sent
- Closure verification — CA owner closes it but there is no notification to the audit admin that all CAs are resolved
- Monthly/quarterly compliance status alert — no proactive alert if a division misses its audit window

### Target Event Model

Every state change should trigger a defined event. The priority events:

```
AuditSubmitted        → email to routing rule recipients (exists) + in-app notification (missing)
AuditClosed           → email to auditor (missing)
AuditReopened         → email to auditor (missing)
CaCreated             → email to assignee (missing) + in-app (missing)
CaOverdue (day 15)    → escalation email to EscalationEmail configured on Division SLA (missing)
CaOverdue (day 30)    → second escalation (missing)
DivisionAuditOverdue  → email to division manager (missing)
DistributionSent      → email to all recipients (exists — manual trigger)
ScheduledReportFired  → email delivery (exists)
```

The escalation email field is already modeled: `SetDivisionSlaRequest` in AuditController includes `EscalationEmail`. The wire-up to `CaReminderService.cs` is the missing step.

---

## 8. DESIGN SYSTEM BLUEPRINT

### Current State

The `tailwind.config.js` is:
```js
module.exports = {
    content: ['./src/**/*.{html,js,ts,vue}'],
    theme: { extend: {} },
    plugins: [],
};
```

There are zero custom tokens. Every color in every component is hardcoded.

### Required Token Set

**Colors (add to `tailwind.config.js` → `theme.extend.colors`)**:

```js
surface: {
  page:    '#0f172a',    // bg-slate-950 equivalent
  card:    '#1e293b',    // bg-slate-800 equivalent
  input:   '#334155',    // bg-slate-700 equivalent
  border:  '#475569',    // border-slate-600 equivalent
  muted:   '#64748b',    // text-slate-500 equivalent
},
brand: {
  primary:  '#3b82f6',  // blue-500
  hover:    '#2563eb',  // blue-600
},
status: {
  conforming:    '#34d399',  // emerald-400
  nonConforming: '#f87171',  // red-400
  warning:       '#fbbf24',  // amber-400
  na:            '#64748b',  // slate-500
}
```

**Spacing**: define `spacing.page` as `1rem` (mobile) / `1.5rem` (md) for consistent page padding.

### Required Shared Primitives

**Immediately (before any new feature work)**:

1. `webapp/src/components/feedback/EmptyState.vue` — props: `icon`, `title`, `description`, `actionLabel`, `actionClick`. Used on every list page.
2. `webapp/src/components/feedback/ErrorBanner.vue` — props: `message`. Currently every view has inline error display.
3. `webapp/src/components/layout/FilterBar.vue` — wraps the filter row pattern (used on 5+ pages identically). Props: `loading`. Slot for filter controls.
4. `webapp/src/components/data/KpiCard.vue` — props: `value`, `label`, `icon`, `severity`, `delta`, `href`. Used on ReportsView, CorrectiveActionsView, AdminUsersView.
5. `webapp/src/components/layout/StickyBottomActions.vue` — mobile-only sticky action bar. Used on AuditFormView.

### Page Shell Standard

Every page inside AppLayout must:
1. Use `<BasePageHeader icon title subtitle>` with action buttons in the slot
2. Have a `FilterBar` below the header when filters exist
3. Use `<EmptyState>` when data is empty
4. Use `<ErrorBanner>` for load failures (not just toast)
5. Use `ProgressSpinner` centered in a `flex justify-center py-16` wrapper during load
6. Have page-level padding of `p-4` on the content area

NewsletterView and QuarterlySummaryView violate every one of these. ReportComposerView violates the filter element standard.

### How to Prevent Future Drift

Add a lint rule (ESLint + eslint-plugin-vue) that flags raw `<input type="...">` and `<select>` elements in `.vue` files under `src/modules/`. All form inputs must be PrimeVue components.

---

## 9. TARGET ARCHITECTURE

### Views (orchestration only — ~200 lines each)

Each view should:
- Declare `const store = useX()` and `const service = useAuditService()`
- Call composables for data loading and mutations
- Own only navigation decisions (`router.push(...)`) and dialog visibility toggles
- Not contain any `async function` that calls the API directly

Files that must be broken down: `ReportsView.vue`, `AuditReviewView.vue`, `CorrectiveActionsView.vue`, `ReportComposerView.vue`.

### Components (UI only)

- `AuditSection.vue` and `AuditQuestionRow.vue` — currently correct (UI + store.setResponse calls)
- `AuditAttachments.vue` — has its own `getClient()` at line 122. Move to `useAuditAttachments.ts` composable
- `AuditQuestionRow.vue` — has `getClient()` at line 357 for photo upload. Move to `useQuestionPhoto.ts` composable

### Composables (logic/state)

Priority composables to create:
- `useAuditService()` — replaces the 28 scattered `new AuditClient(...)` calls
- `useReportData()` — shared report data for ReportsView + Newsletter + Quarterly
- `useReportCharts()` — all chart configs (extracted from ReportsView ~300 lines of chart computeds)
- `useReportDrilldown()` — drill state management extracted from ReportsView
- `useReviewDistribution()` — distribution recipient management extracted from AuditReviewView
- `useReviewEmail()` — distribution email preview/send extracted from AuditReviewView
- `useCaList()` — CA list pagination/filter/load extracted from CorrectiveActionsView

### Service Layer

Create `webapp/src/services/` directory with:
- `AuditApiService.ts` — thin wrapper returning `useAuditService()` composable (DI-friendly)
- `ReportExportService.ts` — CSV, Excel, PDF download functions currently scattered across views
- `ReviewEmailService.ts` — fallback distribution preview builder currently inline in AuditReviewView

### Stores (what stays, what shrinks)

- `auditStore.ts` — keep as-is. Audit form state is correctly here. The `getClient()` factory method is acceptable in the store.
- `adminStore.ts` — delete after extracting to composables. The combined loading indicator is the primary defect.
- `userStore.ts` — keep as-is. Small, clean, single responsibility.
- `apiStore.ts` — keep as-is. Axios instance management and interceptors belong here.

### Reporting Engine

Target: `webapp/src/modules/audit-management/features/reports/`
```
composables/
  useAuditReportData.ts    (shared data source for all report views)
  useReportCharts.ts       (chart data configs)
  useReportDrilldown.ts    (drill-down state)
  useReportKpis.ts         (count-up, visibility, localStorage)
  useReportEngine.ts       (existing — keep, cleanup AuditClient calls)
  useReportDraft.ts        (existing — keep)
views/
  ReportsView.vue           (analytics dashboard — shrink to ~250 lines)
  NewsletterView.vue        (port to AppLayout — use useAuditReportData)
  QuarterlySummaryView.vue  (port to AppLayout — use useAuditReportData)
  ReportComposerView.vue    (keep, replace native inputs with PrimeVue)
  ReportGalleryView.vue     (keep)
  ScheduledReportsView.vue  (keep)
  AuditsByEmployeeView.vue  (keep)
```

### Background Jobs

Current: `ScheduledReportService` (every 5 min), `CaReminderService` (daily).

Required additions:
- `DivisionAuditOverdueService` — daily job to detect divisions past their `AuditFrequencyDays` window and notify
- CA escalation logic inside `CaReminderService` — add Day 15 and Day 30 escalation passes using the `EscalationEmail` on the division SLA config

### Notifications

Phase 1 (email only — infrastructure already exists):
- Wire CA assignment notification to CaCreated event
- Wire audit reopen notification to the original auditor
- Wire division audit overdue to division manager via EmailRoutingRule

Phase 2 (in-app):
- Add `Notification` model and `NotificationController`
- SignalR hub already exists (`@microsoft/signalr` in package.json) — wire to notification push on CA assignment

### Shared UI System

Move `BasePageHeader`, `BaseDataTable`, `BaseButtonSave/Create/Delete`, `BaseFormField` to `webapp/src/components/shared/` with JSDoc. Create `KpiCard`, `EmptyState`, `ErrorBanner`, `FilterBar`, `StickyBottomActions`.

---

## 10. MOBILE / TABLET FIELD READINESS

### Current State

The audit form is technically usable on a tablet. The responsive classes (`px-4`, Tailwind's `md:` prefix) prevent complete breakage. But the following are not field-ready:

1. **No sticky mobile action bar**: "Save Draft" and "Submit for Review" are in the page header. On mobile, the user must scroll to the top to save. During a field audit with 50+ questions, this is a workflow blocker.

2. **AuditQuestionRow tap targets**: The Conforming/NonConforming/Warning/NA buttons must be comfortably tappable with a gloved finger. Without reviewing the exact rendered size, the current PrimeVue button implementation is likely 32–36px tall — below the 44px minimum for field use.

3. **No offline capability**: The `saveDraftToLocalStorage` autosave is good. But if the network drops mid-audit, the `saveResponses` API call fails and the user sees a toast error. There is no service worker, no queue, no retry. In field conditions (cellular coverage gaps), this is a critical gap.

4. **NewsletterView and QuarterlySummaryView**: Zero responsive CSS. These are desktop-only print views.

5. **No camera integration**: `AuditAttachments.vue` has a file input with `accept="image/*"`. On iOS/Android, this triggers the native camera chooser. However, there is no direct "Take Photo" button with camera API access, no EXIF stripping for privacy, and no photo annotation capability.

### What Must Change for Field-Usable Tablet

1. `AuditFormView.vue`: Add `<StickyBottomActions>` component that replaces header save/submit on viewports < 768px.
2. `AuditQuestionRow.vue`: Ensure response buttons have `min-h-[44px] min-w-[44px]` touch targets.
3. Evaluate PWA + service worker for offline draft queueing. The `auditStore.ts` localStorage draft is the foundation — extend it with a `pendingSync` queue that retries when network is restored.
4. Port `NewsletterView` and `QuarterlySummaryView` to AppLayout with `@media print` CSS.

---

## 11. SPECIFIC ANSWERS

**1. Is the reports section salvageable, or should it be re-architected?**

Salvageable. The data model is correct. The `ScheduledReport` model, `PdfGeneratorService`, `useReportEngine.ts`, and `useReportDraft.ts` are keepers. The problem is 7 isolated views sharing no data composable and two completely different style systems. Extract `useAuditReportData.ts` as the single shared data source. Port `NewsletterView` and `QuarterlySummaryView` to Tailwind+AppLayout. The analytics dashboard stays at `/reports`. No routes need to be deleted.

**2. What is the single highest-leverage architecture refactor?**

Create `useAuditService()` as a shared composable factory that replaces all 28 `new AuditClient(...)` instantiations. One change, one file, eliminates the primary scattered-API-access problem. This is the unlocking change for every other refactor — once the service is a proper composable, adding auth refresh, request tracing, and retry logic benefits all 15 files at once.

**3. What is missing to fully automate quarterly summary and newsletter generation?**

The `QuarterlySummaryView` and `NewsletterView` are `window.print()` print views. They cannot be emailed by the scheduler because they require a browser to render. To make them schedulable: (a) create a server-side HTML template for each that mirrors the client view, (b) pass it through `PdfGeneratorService` to produce a byte[], (c) add a `newsletter` and `quarterly-summary` TemplateId to the `ScheduledReport.TemplateId` enum, (d) wire the `ScheduledReportService` to handle those template IDs. The PuppeteerSharp infrastructure already handles HTML→PDF. The gap is the server-side HTML template.

**4. What is missing to truly auto-send reports, emails, and notifications?**

Three specific gaps: (a) Distribution email still requires a manual admin trigger — wire it to `AuditSubmitted` event automatically based on the routing rules that are already configured. (b) CA assignment has no notification — add `CaCreated` event notification to the assignee. (c) Division audit overdue has no notification — add the `DivisionAuditOverdueService` background job. The email infrastructure (EmailService, routing rules, SMTP config) is ready. Only the event→trigger wiring is missing.

**5. What should the app-wide design system consist of?**

Surface color tokens (page/card/input/border/muted), brand color tokens (primary/hover), status color tokens (conforming/nonConforming/warning/na), border-radius tokens (sm/md/lg), shadow tokens (card/elevated), and 5 shared primitives: EmptyState, ErrorBanner, FilterBar, KpiCard, StickyBottomActions. All defined in `tailwind.config.js` and documented in `webapp/src/components/shared/`. See Section 8 for the full token spec.

**6. Which files should be split first?**

In order: (1) `ReportsView.vue` — 1,658 lines, highest traffic, most business logic leakage. (2) `AuditReviewView.vue` — 1,151 lines, 4 workflows, security concern in `buildFallbackDistributionPreview`. (3) `CorrectiveActionsView.vue` — 918 lines, raw axios bypass, needs composable extraction. (4) `adminStore.ts` — combined loading indicator bug, 3 unrelated domains.

**7. Which 3 pages need the most urgent UX overhaul?**

(1) `NewsletterView` — completely different visual language from the rest of the app, not responsive, uses raw CSS classes, labeled misleadingly as a "Compliance Newsletter" when it is a quarterly report. (2) `ReportComposerView` — blank on load, raw HTML inputs, no empty state guidance. (3) `AuditFormView` — no sticky mobile action bar, save/submit unreachable mid-form on mobile, which is where field auditors use it.

**8. Which current features are strongest and should be preserved?**

Life-critical auto-fail detection, section N/A defensibility model, repeat finding detection across history, distribution email preview-before-send, CA reminder deduplication via `CaNotificationLog`, prior audit prefill with diff tracking, template versioning with draft/publish workflow, weighted scoring and score targets per division, and the `PdfGeneratorService` + `ScheduledReportService` infrastructure.

**9. What should the first implementation PR contain?**

See File 4 (30-60-90 Plan) for the detailed PR1 specification. In summary: create `useAuditService()` composable, replace all 28 `new AuditClient(...)` calls, add `tailwind.config.js` token set, create `EmptyState` component, and fix `AdminAuditLogView.vue` runtime error. No feature changes — only structural cleanup and one verified bug fix.

**10. What should NOT be changed early because it would create unnecessary risk?**

Do not touch: the EF migration chain (3 migrations, currently clean), the `auditStore.ts` core logic (score calculation, visibleSections, logic rules — these have E2E tests), the `MediatR` handler dispatch pattern in the backend (it is the cleanest part of the codebase), the `CaReminderService` deduplication logic (complex timing, any change could cause notification spam), and the session storage print pipeline (`AuditDashboardView` → `print-blank-form-data` → `PrintableAuditFormView`). The print pipeline is fragile and intentional — do not refactor it until a proper server-side PDF route replaces it.
