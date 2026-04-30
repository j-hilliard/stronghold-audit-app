# AUDIT APP SCREEN SWEEP REVIEW
Generated: 2026-04-28 | Based on QA_AUDIT_20260427_SCREEN_SWEEP.md + Code Analysis

---

## 1. PAGE INVENTORY TABLE

| Route | Page Name | Purpose | Major Actions | Data Dependencies | Health |
|---|---|---|---|---|---|
| `/audit-management/reports` | Analytics Dashboard | KPI overview, trends, analysis tabs, drill-down, action items | Filter by division/status/date, export CSV, navigate to sub-reports, drill to auditor/location | `getAuditReport()`, `getComplianceStatus()`, `getDivisions()` | 7/10 — functional but 1,658-line view is a maintenance risk |
| `/audit-management/audits` | Audit List | View and manage all audits, filter, bulk delete | Filter, row double-click to open, bulk delete, print blank form | `getAuditList()`, `getDivisions()` | 8/10 — clean BaseDataTable integration |
| `/audit-management/audits/new` | New Audit | Select division + optional sections, start audit | Division select, optional section toggles, create audit | `getDivisions()`, `getActiveTemplate()`, `createAudit()` | 8/10 — good UX flow |
| `/audit-management/audits/:id` | Audit Form | Answer compliance questions section by section | Answer questions, mark N/A, attach photos, save draft, submit | `getAudit()`, `getActiveTemplate()`, `saveResponses()`, `submitAudit()` | 8/10 — lean view, good store delegation; mobile action reachability is a gap |
| `/audit-management/audits/:id/review` | Audit Review | Review submitted audit findings, assign/close CAs, send distribution email | Assign CA, close CA, reopen audit, close audit, send distribution email, add recipients | `getAuditReview()`, CA APIs, distribution APIs | 6/10 — 1,151 lines with 4 unrelated workflows |
| `/audit-management/corrective-actions` | Corrective Actions | Track and resolve all CAs across all audits | Filter, bulk close/reassign/void, close individual CA, export Excel | `getCorrectiveActions()`, `closeCA()`, `bulkUpdateCAs()` | 7/10 — feature-rich; raw axios bypass is a code quality issue |
| `/audit-management/admin/templates` | Template Manager | Create, edit, and publish audit templates | Add/edit/remove questions, reorder sections, publish draft, manage logic rules | All template CRUD endpoints via adminStore | 7/10 — complex but functional |
| `/audit-management/admin/settings` | Email Routing | Configure which emails receive audit notifications per division | Add/edit/remove routing rules | `getEmailRouting()`, `updateEmailRouting()` | 7/10 — clean CRUD interface |
| `/audit-management/admin/users` | User Management | Manage user audit roles | Add user, assign role, disable/enable user, search | `getUsersWithAuditRoles()`, `setUserAuditRole()` | 7/10 — functional; stats row at top is useful |
| `/audit-management/admin/audit-log` | Admin Audit Log | Compliance audit trail and action log | Filter by type/action, expand rows, view by user/date | Backend audit trail endpoints | 4/10 — confirmed runtime error on Action tab row expansion |
| `/audit-management/reports/composer` | Report Composer | Build custom PDF reports with section selection | Select division/period, pick sections, save draft, generate/download PDF | `getReportDrafts()`, `generateReport()` | 5/10 — blank on load, raw HTML inputs |
| `/audit-management/reports/gallery` | Generate Report | Pick a report template and generate PDF | Select template type, generate, download | `generateReport()` | 6/10 — functional but limited guidance |
| `/audit-management/reports/scheduled` | Scheduled Reports | Configure automated report deliveries | Add/edit/delete schedule, toggle active | `getScheduledReports()`, `upsertScheduledReport()` | 7/10 — well implemented |
| `/audit-management/reports/by-employee` | Audits by Employee | Filter audit history by auditor | Select auditor, view their audit list and scores | `getAuditReport()` with auditor filter | 6/10 — thin wrapper, relies on report endpoint |
| `/audit-management/newsletter` | Compliance Newsletter | Quarterly newsletter print view | Select division/quarter/date range, generate AI narrative, print | `getAuditReport()` | 3/10 — completely different visual language, not responsive |
| `/audit-management/reports/quarterly-summary` | Quarterly Summary | Simplified quarterly compliance summary print | Select division/quarter, print | `getAuditReport()` | 4/10 — different visual language, not responsive |
| `/audit-management/print/:divisionId` | Printable Blank Form | Print-ready blank audit form | (Print only) | sessionStorage `print-blank-form-data` | 5/10 — functional but fragile sessionStorage dependency |
| `/audit-management/print-review/:auditId` | Printable Review | Print-ready audit review | (Print only) | sessionStorage `print-review-data` | 5/10 — same fragility |

---

## 2. TOP 10 UI/UX ISSUES

**Issue 1 — P0: NewsletterView and QuarterlySummaryView use a completely different visual language**

Evidence: `120-newsletter-base.png`, `121-quarterly-summary-base.png` from the QA sweep. These views have plain CSS classes (`.toolbar`, `.picker`, `.kpi-grid`, `.kpi-box`, `.kpi-value`, `.kpi-label`) that are not defined in Tailwind. They do not use `BasePageHeader`. They have a dark-blue top toolbar that is visually distinct from the rest of the app's `bg-slate-800` card system. A user navigating from the Reports dashboard to the Newsletter sees what appears to be a different application.

Root cause: These views were likely developed separately as print-first mockups and never ported to the app's design system.

Fix: Port both views to use `BasePageHeader` inside AppLayout (currently they are routed outside AppLayout — see `router/index.ts` lines 18–28), replace all custom CSS with Tailwind utility classes, and use `@media print` to handle print-specific layout rather than a separate page shell.

**Issue 2 — P0: ReportComposerView loads completely blank**

Evidence: `90-report-composer-base-empty.png`. The Composer page shows an empty editor state with no guidance on what to do. The top filter bar uses raw `<select>` elements (lines 12–16 of ReportComposerView.vue) and raw `<input type="date">` elements instead of PrimeVue components. The placeholder text ("Select division…") is the first instruction a user sees.

Fix: Add an `EmptyState` component instructing the user to "Select a division and date range to load data." Add a "Load Data" primary button. Replace raw HTML inputs with PrimeVue Dropdown and Calendar.

**Issue 3 — P1: Admin Audit Log has a runtime error on row expansion**

Evidence: `78-admin-audit-log-action-tab.png` and `80-admin-audit-log-expanded-row.png` from QA sweep. The Action tab row expansion causes a visible error. The expanded row template likely renders a property that is null or undefined on some log entries.

Root cause: AdminAuditLogView.vue — the `v-for` that renders expanded row details likely accesses `.detail` or a nested property without a null guard.

Fix: Add `?.` optional chaining or `v-if` guards on the expanded row template slots. Test with both action log and trail log entries.

**Issue 4 — P1: Mobile audit form has unreachable primary actions**

The "Save Draft" and "Submit for Review" buttons are in the `BasePageHeader` slot. On mobile, the header renders as a stack of buttons that may overflow or move off-screen when the audit form is long. A field auditor who has answered 30 questions cannot easily save or submit without scrolling back to the top.

Evidence: `130-mobile-audits.png` from QA sweep. The QA report notes "mobile/tablet UX" as a key risk.

Fix: Add `<StickyBottomActions>` that is hidden on `md:hidden` and shows Save/Submit buttons fixed at the bottom of the viewport. This is the standard pattern on every major mobile-first audit tool (SafetyCulture, GoAudits).

**Issue 5 — P1: "Reports" navigation is confusing**

The button labeled "Reports" in the `ReportsView.vue` header (line 28 of the template) opens a popup menu with items "Report Composer," "Generate PDF," "By Employee," "Quarterly Summary." Pressing "Reports" while on the Reports page navigates away from it. The breadcrumb says "Dashboard." The route is `/reports`. The page title in `router/index.ts` says "Dashboard." Three different names for the same thing.

Fix: Rename the page to "Analytics Dashboard" consistently across the title, breadcrumb, and route meta. Rename the header menu button to "Export / Print" to signal it is about output, not navigation.

**Issue 6 — P1: Filter bars are not a consistent component**

Every page with filters (ReportsView, AuditDashboardView, CorrectiveActionsView, AdminUsersView, AdminAuditLogView) implements its own filter bar layout inline in the template. On some pages filters are in a bordered div. On others they are in a flex-wrap gap. Label styling varies: some use `text-[10px] font-semibold text-slate-500 uppercase tracking-wider`, others use `text-xs text-slate-400 font-medium`. The filter bar is the second thing a user sees on every page.

Fix: Create `webapp/src/components/layout/FilterBar.vue` with a named slot for filter controls. Standardize label size and spacing. Used on all 5+ pages that have filters.

**Issue 7 — P2: Empty states are visually inconsistent**

ReportsView shows plain text ("No report data available." at line 800–802). CorrectiveActionsView shows a centered icon + 2-line text (lines 157–163). AuditDashboardView shows a similar pattern. These are three different implementations of the same UX pattern.

Fix: Create `webapp/src/components/feedback/EmptyState.vue`. Props: `icon` (string, default "pi pi-inbox"), `title` (string), `description` (string), `action` (slot). Use on all pages.

**Issue 8 — P2: Score ring in AuditReviewView is an inline SVG with hardcoded dimensions**

Lines 145–163 of AuditReviewView.vue contain an inline `<svg width="100" height="100">` score ring. The `ringCircumference`, `ringDashoffset`, and `ringColor` computeds are defined in the view's script (lines 1094–1105). This is good-quality code, but it belongs in a `ScoreRing.vue` component, not in a 1,151-line view file.

Fix: Extract to `webapp/src/components/data/ScoreRing.vue` — props: `scorePercent`, `size`. The same ring pattern could also be used on the Reports dashboard and the AuditFormView post-submit summary.

**Issue 9 — P2: Division Health cards in ReportsView have complex inline class bindings**

Lines 319–349 of ReportsView.vue contain a division health card with 4-way `:class` bindings using `div.complianceStatus` ('OnTrack', 'DueSoon', 'Overdue', 'NeverAudited') mapped to border colors. This same pattern repeats at lines 332–348 for the status badge. The result is 30 lines of inline class logic that is hard to read and cannot be tested.

Fix: Extract to `webapp/src/modules/audit-management/features/reports/components/DivisionHealthCard.vue` — props: `division` (DivisionHealthDto). The card owns the status color mapping logic internally.

**Issue 10 — P3: Printable views depend on sessionStorage as a data transfer mechanism**

`AuditDashboardView.vue` line 235: `sessionStorage.setItem('print-blank-form-data', JSON.stringify(template))`. `AuditReviewView.vue` line 1146: `sessionStorage.setItem('print-review-data', JSON.stringify(review.value))`. The print views (`PrintableAuditFormView.vue`, `PrintableAuditReviewView.vue`) read from sessionStorage on mount.

This is fragile: if the print tab is opened without the originating tab setting the sessionStorage key (e.g., direct URL navigation, browser back, private window), the print view silently fails or attempts an API call without an auth token.

Fix: The print views should call the authenticated API directly using the auth token that is available if the user is logged in. sessionStorage should be a fallback, not the primary path. Or replace with a server-rendered print endpoint (`/api/v1/audits/:id/print-html` returning HTML that the browser can print directly).

---

## 3. TOP 3 PAGES NEEDING URGENT REDESIGN

**1. NewsletterView (`/audit-management/newsletter`)**

Rationale: This is visually and structurally the most inconsistent page in the application. It is routed outside AppLayout, uses a custom CSS toolbar that looks like a different product, and produces output (a print-ready newsletter) that is one of the most visible externally-facing deliverables. The AI narrative generation button exists but the output cannot be emailed by the scheduler because the view is client-rendered. It is a showcase feature that is half-built.

Required changes: Route inside AppLayout, use BasePageHeader, replace all custom CSS with Tailwind, add `@media print` for print-specific styles, port data loading to `useAuditReportData.ts` composable, add scheduler integration support.

**2. ReportComposerView (`/audit-management/reports/composer`)**

Rationale: This is the most powerful reporting tool in the application (draft-based PDF generation, section selection, custom titles, newsletter mode) but it is completely unusable on first contact. The blank load state has no guidance. The raw `<select>` and `<input>` elements break the visual contract of the app. The top bar is a dense horizontal strip with no visual hierarchy. A non-technical user would not know what to do.

Required changes: Add empty state with guidance, replace raw HTML inputs with PrimeVue components, use consistent label styles, add a "Quick Start" panel that pre-selects the user's last-used division, add `loading` skeleton states for when drafts are loading.

**3. AdminAuditLogView (`/audit-management/admin/audit-log`)**

Rationale: This is a broken page. The row expansion on the Action tab throws a runtime error (confirmed in QA sweep screenshots `78` and `80`). The admin audit log is a compliance-critical feature — regulators and auditors need to see who changed what and when. A broken compliance log undermines the entire audit application's credibility.

Required changes: Fix the null reference in the row expansion template, add null guards on all DTO properties rendered in the expanded row, add an `ErrorBanner` for load failures, add proper empty state for the no-data case.

---

## 4. SHARED UI PATTERNS THAT SHOULD BECOME COMPONENTS

| Pattern | Currently In | Proposed Component |
|---|---|---|
| KPI card (value + label + optional delta) | ReportsView (6 cards), CorrectiveActionsView (5 cards), AdminUsersView (3 cards) | `KpiCard.vue` — props: `value`, `label`, `delta`, `severity` |
| Status severity mapper (Conforming→success, NC→danger, etc.) | AuditReviewView, AuditDashboardView, ReportsView — same logic duplicated | `useStatusSeverity.ts` — composable/util |
| Count-up animation | ReportsView (4 count-ups), CorrectiveActionsView (5 count-ups) — identical `useCountUp` functions copy-pasted | `useCountUp.ts` composable in `webapp/src/composables/` |
| Score ring (SVG animated score circle) | AuditReviewView (inline SVG), implied in post-submit modal in AuditFormView | `ScoreRing.vue` — props: `scorePercent`, `size` |
| Division health card | ReportsView (inside divisionHealthCards computed) | `DivisionHealthCard.vue` — props: `division: DivisionHealthDto` |
| Empty state (icon + title + description) | ReportsView, CorrectiveActionsView, AuditDashboardView | `EmptyState.vue` |
| Filter bar row (label + control stack) | ReportsView, AuditDashboardView, CorrectiveActionsView, AdminUsersView, AdminAuditLogView | `FilterBar.vue` |

---

## 5. VISUAL CONSISTENCY GAPS

1. **Tailwind config has zero custom tokens** — every color, radius, and shadow is hardcoded per-component. `bg-slate-800/60`, `border-red-700/50`, `text-emerald-400` are repeated dozens of times. One brand color change requires grep-and-replace.

2. **Two parallel styling systems** — PrimeVue + Tailwind (primary) vs. raw CSS (NewsletterView, QuarterlySummaryView). These must be unified. No new feature should use raw CSS.

3. **PrimeVue Dropdown vs native `<select>`** — ReportComposerView uses native `<select>` throughout. The template is explicitly uses raw `class="bg-slate-700 border border-slate-600 rounded px-3 py-1.5..."` on native selects. Every other page uses PrimeVue Dropdown. Pick one and enforce it.

4. **Border treatment varies** — Some cards use `border border-slate-700`, others `border border-slate-700/50`, others `border border-slate-600`. These should be one standard: `border border-surface-border`.

5. **Button sizing inconsistency** — Header action buttons sometimes use `size="small"` and sometimes don't. The "Reports" menu button in ReportsView is size default. Corrective Actions header uses `size="small"` on all buttons. Standardize on `size="small"` for all header action buttons.

---

## 6. EMPTY / ERROR / LOADING STATE GAPS

| Page | Loading State | Empty State | Error State |
|---|---|---|---|
| ReportsView | ProgressSpinner (correct) | "No report data available." — plain text only, no icon | None — silent failure |
| AuditDashboardView | `loading` passed to BaseDataTable (correct) | Icon + 2-line text (correct) | None |
| CorrectiveActionsView | DataTable `:loading` prop (correct) | Icon + 2-line text in `#empty` slot (correct) | Toast only |
| AuditReviewView | ProgressSpinner (correct) | "Review not available." — plain text only | None |
| NewsletterView | "Loading newsletter data..." — plain text | None | None |
| QuarterlySummaryView | "Loading…" — plain text | None | None |
| ReportComposerView | No visible loading state on draft load | No empty state or guidance | None |
| AdminAuditLogView | Unknown (broken page) | Unknown | Runtime error on row expand |
| AdminUsersView | `:loading` on refresh button | None visible | None |
| ScheduledReportsView | Unknown | Unknown | Unknown |

All loading states should use `ProgressSpinner` in `<div class="flex justify-center py-16">`. All empty states should use the shared `EmptyState` component. All error states should show an `ErrorBanner` component above the content, not only a toast (toasts disappear).

---

## 7. MOBILE USABILITY GAPS

| Gap | Pages Affected | Severity |
|---|---|---|
| Primary actions unreachable mid-scroll | AuditFormView — Save/Submit in header | P1 |
| No responsive layout | NewsletterView, QuarterlySummaryView | P1 |
| Filter bars wrap awkwardly | CorrectiveActionsView (5 dropdowns), ReportsView (4 controls), AdminUsersView | P2 |
| Touch target size unknown | AuditQuestionRow response buttons | P2 |
| No offline autosave recovery UI | AuditFormView — network failure during save | P2 |
| sessionStorage print dependency | AuditDashboardView, AuditReviewView — direct print tab navigation fails | P2 |
| Dense template manager on tablet | TemplateManagerView — drag-and-drop reorder is not touch-friendly | P3 |
| No camera-direct integration | AuditAttachments — file picker only | P3 |

The strongest mobile-ready page is `AuditDashboardView` — simple table layout, responsive filter wrap, and BaseDataTable handles mobile gracefully. This should be the pattern all list views follow.
