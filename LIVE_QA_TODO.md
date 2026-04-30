# LIVE QA TODO — Stronghold Audit App
_Last updated: 2026-04-27 | 11 code fixes applied (P1-002/003/005/006/007/008/010/012/014, P2-001/002) | full-stack benchmark audit charter added_

## How to use this file
- **OPEN** = verified defect, needs fixing, awaiting Joseph approval
- **VERIFY** = candidate issue, needs live test or deeper read to confirm
- **ACCEPTED AS-IS** = reviewed, intentional, no change needed
- **APPROVED** = Joseph said fix it
- **DONE** = fixed and regression-tested
- No code is changed without Joseph's explicit approval.

---

## PLANNED AUDIT SWEEP — Full-Stack Benchmark + Experience Review

### AUDIT-20260427-FULLSTACK-BENCHMARK: Full Compliance Audit App Review
- **Status:** OPEN
- **Severity:** P1 planning item; promote individual defects to P0/P1/P2 only after evidence.
- **Scope:** Stronghold compliance audit app only. Do not import estimating, staffing, bid, manpower, revenue-estimate, or customer-approval assumptions unless the repository code actually contains them.
- **Goal:** Run a full-stack, evidence-backed audit covering code defects, logic defects, workflow defects, data integrity risk, UI/UX, visual/layout polish, discoverability, reporting, template engine, corrective actions, draft/autosave safety, route guards, print/export/report composer, testing/doc drift, and external market benchmark opportunities.
- **Required benchmark research:** Current web research on modern compliance audit / inspection / EHS / field-audit products. Produce a benchmark matrix, top market patterns worth adapting, Stronghold gaps, and differentiation opportunities. Use competitors as evidence, not as blind requirements.
- **Required read-only subagents:** Exactly 3, in this order: UI / End-User Agent, Logic / Workflow Agent, Code / Architecture Agent. Agents must read this live TODO first, must not change app code, and must return evidence-backed findings only.
- **Must-review routes:** `/audit-management/reports`, `/audit-management/audits`, `/audit-management/audits/new`, `/audit-management/audits/:id`, `/audit-management/audits/:id/review`, `/audit-management/corrective-actions`, `/audit-management/admin/templates`, `/audit-management/admin/settings`, `/audit-management/newsletter/template-editor`, `/audit-management/reports/composer`, `/audit-management/reports/gallery`, `/audit-management/reports/scheduled`, `/audit-management/reports/by-employee`.
- **Must-review files:** `webapp/src/modules/audit-management/router/index.ts`, audit dashboard/new-audit/audit-form/review/corrective-action/template-manager/report views, `useReportDraft.ts`, `auditStore.ts`, `adminStore.ts`, `webapp/src/apiclient/auditClient.ts`, `Api/Api.csproj`, and QA docs under `docs/qa/`.
- **UI candidates to verify:** U-001 dashboard action cluster, U-002 audit list filters, U-003 new audit progression/dead space, U-004 corrective action table density/actions, U-005 template manager workflow clarity, U-006 report composer empty state, U-007 dashboard hide controls, U-008 date input consistency.
- **Logic/technical candidates to verify:** B-001 skip logic identifier mismatch, B-002 duplicate audit load, B-003 composer active draft double-delete, B-004 misleading newsletter AI label, B-005 reporting count semantics, B-006 corrective-action bulk close photo policy, B-007 dead delete-selected state, B-008 print dialog loading/error state, B-009 QA new-audit contract drift, B-010 division normalization ownership, B-011 handwritten client contract risk, B-012 route guard string fragility, B-013 blank-form print fragility, B-014 reporting period drift, B-015 prior new-audit navigation regression.
- **Deliverables:** Executive summary, external benchmark summary, verified bugs, verified visual/UX improvements, verified functional/workflow improvements, verified code/architecture improvements, items still needing verification, recommended fix order, live TODO summary, regression checklist summary, subagent workflow summary, evidence packet paths, and top items Joseph should approve first.
- **Evidence required:** Screenshots under `docs/qa-evidence/<sweep-id>/`, test output/logs where possible, exact repro steps, exact likely files, likely root cause, recommended fix, and regression protection for every verified finding.
- **No-code-change gate:** No application code changes until Joseph explicitly approves a specific fix. QA docs, evidence, and temporary test harnesses may be updated for audit coordination.
- **Regression linkage:** See `QA_REGRESSION_CHECKLIST.md` section "15. Full-Stack Benchmark Audit Backlog".

## STATUS CORRECTIONS (from 2026-04-27 audit sweep)

| Old ID | Was | Now | Reason |
|---|---|---|---|
| DEF-0001 | Open | **ACCEPTED AS-IS** | Card UI was replaced with a working dropdown. Defect no longer exists. |
| DEF-0003 | Open | **ACCEPTED AS-IS** | Template Manager is fully implemented (1003 lines). Not a placeholder. |
| DEF-0004 | Open | **ACCEPTED AS-IS** | Reports/KPI is fully implemented (800+ lines, filters, KPI, export). Not a placeholder. |
| DEF-0009 | Open | **ACCEPTED AS-IS** | 404 is handled silently; frontend uses defaults. No visible breakage. |
| DEF-0010 | Open | **ACCEPTED AS-IS** | Tiptap import is correct for v3.22.3. Not a defect. |
| DEF-0011 | Open | **ACCEPTED AS-IS** | Incident delete is correctly wired and reactive. Not a defect. |

---

## LIVE SCREEN SWEEP RESULTS (2026-04-27/28)

Evidence root: `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/`

- **Status:** COMPLETED
- **Live API:** `http://localhost:7221`
- **Playwright result:** 1 passed in 53.4s
- **Screenshots:** 62 live screenshots under `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/screenshots/`
- **Sweep log:** `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/sweep-log.json`
- **Important note:** Do not accept mock-only screenshots for this sweep. The first API check failed until the API was started on the frontend-expected 7221 port.

### P0-002: Section N/A Override Is Not Yet End-to-End Enforced
- **Status:** OPEN
- **Severity:** P0
- **Area:** Audit Form / Data Integrity / Reporting
- **Problem:** The frontend score path excludes section-N/A questions, but save/review/report backend paths can still persist and count prior responses from a section later marked N/A.
- **Evidence:** Logic agent review of `auditStore.ts`, `SaveAuditResponses.cs`, `GetAuditReview.cs`, `GetAuditReport.cs`; screenshot `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/screenshots/33-audit-form-section-na-dialog-or-state.png`.
- **Likely files:** `webapp/src/modules/audit-management/stores/auditStore.ts`, `Api/Domain/Audit/Audits/SaveAuditResponses.cs`, `Api/Domain/Audit/Audits/GetAuditReview.cs`, `Api/Domain/Audit/Audits/GetAuditReport.cs`.
- **Expected behavior:** N/A section responses must be excluded from score denominator, unanswered count, review findings, report findings, and CA workload.
- **Verification required:** Answer a section, save, mark section N/A with reason, save/reload/submit, then verify review/report/score exclude that section.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.1.

### P0-003: Legacy Skip Logic Still Has Identifier Drift Or Must Be Fully Removed
- **Status:** NEEDS JOSEPH REVIEW
- **Severity:** P0
- **Area:** Template Engine / Logic / Data Integrity
- **Problem:** Existing code still carries skip-logic rule paths keyed by `triggerVersionQuestionId` while runtime responses are keyed by `questionId`; dynamically shown rows can fall through to a fake response with `questionId = 0`. Clone logic also does not preserve rules.
- **Evidence:** Logic agent review of `auditStore.ts`, `AuditSection.vue`, `AuditQuestionRow.vue`, `QuestionLogicRule.cs`, `CloneTemplateVersion.cs`.
- **Likely files:** `webapp/src/modules/audit-management/stores/auditStore.ts`, `webapp/src/modules/audit-management/features/audit-form/components/AuditSection.vue`, `webapp/src/modules/audit-management/features/audit-form/components/AuditQuestionRow.vue`, `Api/Domain/Audit/Admin/CloneTemplateVersion.cs`.
- **Expected behavior:** Either remove/deactivate skip logic completely per Joseph's section-N/A direction, or fix it so rules use one canonical question identifier and clone/publish keeps rule behavior intact.
- **Verification required:** Confirm product decision; if skip logic stays, create rule, publish, audit, clone, publish again, and verify no saved response has `questionId = 0`.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.2.

### P1-016: Local API Port Mismatch Breaks Live QA And Causes False Screenshot Evidence
- **Status:** DONE — verified 2026-04-28 after `Api/Properties/launchSettings.json` changed from `http://localhost:5221` to `http://localhost:7221`.
- **Severity:** P1
- **Area:** Dev Experience / QA / API
- **Problem:** The frontend expected the audit API on `http://localhost:7221`, but the API was initially listening on `http://localhost:5221`. Live pages then showed failures such as "Failed to load audit" and any screenshot capture would be invalid.
- **Evidence:** User screenshot showing `Failed to load audit`; local health check failed on 7221 until API was explicitly started with `ASPNETCORE_URLS=http://localhost:7221`; successful sweep log shows `apiBase: http://localhost:7221`; fix audit verified `GET http://localhost:7221/v1/divisions` returned 200 and `launchSettings.json` now matches `dev-start.bat`.
- **Likely files:** `dev-start.bat`, `Api/Properties/launchSettings.json`, webapp environment/API base configuration.
- **Expected behavior:** One dev start path should bring up API and frontend on the same expected ports, with a preflight health check before QA screenshot capture.
- **Verification:** `dotnet build --no-restore` passed; API health returned 200 on `http://localhost:7221/v1/divisions`. Keep the preflight in every future screenshot sweep.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 1 and 16.3.

### P1-017: Admin Audit Log Tabs And Row Expansion Are Broken
- **Status:** OPEN — partial fix verified 2026-04-28. The `expandedRows is not iterable` console crash is fixed, but both Action Log and Change Trail tables still render together, so the tab/panel bug remains.
- **Severity:** P1
- **Area:** UI / Admin / Runtime
- **Problem:** Admin Audit Log originally showed mixed Action Log and Change Trail content and threw `this.expandedRows is not iterable` when expanding rows. Claude's DataTable change removed the console crash, but the live page still shows mixed tab content.
- **Evidence:** Original failure: `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/screenshots/78-admin-audit-log-action-tab.png`, `79-admin-audit-log-change-trail-tab.png`, `80-admin-audit-log-expanded-row.png`; original `sweep-log.json` console error. Partial-fix audit: `docs/qa-evidence/QA_AUDIT_20260428_CLAUDE_FIX_AUDIT/admin-audit-log-regression-result.json` has no console errors, but `admin-audit-log-actions-after-fix.png` and `admin-audit-log-expanded-after-fix.png` still show both tables rendered.
- **Likely files:** `webapp/src/modules/audit-management/features/admin-audit-log/views/AdminAuditLogView.vue`; PrimeVue 3 DataTable/Tabs usage.
- **Claude note:** The `expandedRows` part is fixed with `dataKey="id"` and `v-model:expandedRows`, but the remaining failure is likely the `<Tabs>/<TabList>/<TabPanels>` pattern in a `primevue@3.26.1` app. Replace with PrimeVue 3 `TabView`/`TabPanel`, or render each table with explicit `v-if="activeTab === 'actions'"` / `v-if="activeTab === 'trail'"`.
- **Expected behavior:** Only the selected tab panel renders, row expansion works, and the page emits no console errors.
- **Verification required:** Open `/audit-management/admin/audit-log`, switch both tabs, expand one row in each tab, assert no console errors and no mixed tab content.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.4.

### P1-018: Auditor Create-Audit Permission Can Route Into Admin-Only Template APIs
- **Status:** DONE — verified 2026-04-28 with live browser/network probe.
- **Severity:** P1
- **Area:** Workflow / Permissions / API Contract
- **Problem:** The New Audit route can be available to Auditor users, but the page loads admin template/draft APIs that are admin-guarded.
- **Evidence:** Logic/code agent review of `userStore.ts`, `NewAuditView.vue`, `GetTemplates.cs`, `GetDraftVersionDetail.cs`; fix audit `docs/qa-evidence/QA_AUDIT_20260428_CLAUDE_FIX_AUDIT/new-audit-network-result.json` shows New Audit selected division CSL and called only `/v1/divisions`, `/v1/divisions/8/job-prefixes`, and `/v1/templates/active?divisionId=8`; `adminUrls` is empty.
- **Likely files:** `webapp/src/modules/audit-management/features/new-audit/views/NewAuditView.vue`, `webapp/src/stores/userStore.ts`, `Api/Domain/Audit/Admin/GetTemplates.cs`, `Api/Domain/Audit/Admin/GetDraftVersionDetail.cs`.
- **Expected behavior:** Auditors can create audits using non-admin template lookup endpoints, or the route is hidden/blocked until APIs match the permission model.
- **Verification:** Live New Audit network probe showed no `/v1/admin/*` calls after selecting a division. Remaining hardening: do not silently turn `getActiveTemplate()` 403/500/network errors into "No active template found."
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.5.

### P1-019: Reports Auditor Count Still Appears To Count Scored Rows Instead Of Audits
- **Status:** DONE — verified 2026-04-28 by diff/build/static audit.
- **Severity:** P1
- **Area:** Reporting / Data Integrity
- **Problem:** Newsletter and quarterly count logic was partially addressed, but the Reports auditor table still appears to derive audit count from scored rows, which can undercount all-N/A or unscored audits.
- **Evidence:** Logic agent review of `ReportsView.vue`; live reports screenshots `01-reports-dashboard-base.png`, `05-reports-tab-performance.png`; fix audit confirmed `auditorStats` now initializes `count: 0`, increments `entry.count++` for every row, and outputs `auditCount: s.count`.
- **Likely files:** `webapp/src/modules/audit-management/features/reports/views/ReportsView.vue`.
- **Expected behavior:** Audit volume count and scored-response count must be separately named and mathematically correct.
- **Verification:** `npm.cmd --prefix webapp run build:dev` passed. Static audit confirms the UI no longer derives auditor audit count from `scores.length`. Keep a fixture-based regression with scored + unscored audits for future protection.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.6.

### P1-020: Route Guards Still Have String-Fragile Permission Checks
- **Status:** REOPENED
- **Severity:** P1
- **Area:** Routing / Permissions / Maintainability
- **Problem:** Route meta exists for some routes, but global router still uses `path.includes('/reports')` and `path.includes('/corrective-actions')`; standalone newsletter and quarterly routes lack explicit permission meta.
- **Evidence:** Code review of `webapp/src/router/index.ts` around the remaining `path.includes(...)` checks.
- **Likely files:** `webapp/src/router/index.ts`, `webapp/src/modules/audit-management/router/index.ts`, `webapp/src/stores/userStore.ts`.
- **Expected behavior:** All audit route permission checks are driven by explicit route meta and role capabilities, not path substrings.
- **Verification required:** Route permission matrix for admin, auditor, normal CA user, and no-role user across reports, CAs, newsletter, quarterly, print, admin.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 11 and 16.7.

### U-001: Dashboard Action Cluster And Hide Controls Need Rework
- **Status:** OPEN
- **Severity:** P2
- **Area:** UI / UX / Dashboard
- **Problem:** Top-right actions are visually cramped and similar in weight; KPI cards and banners include low-context hide/close affordances that add noise and may imply data deletion or permanent hiding.
- **Evidence:** `01-reports-dashboard-base.png`, `02-reports-customize-panel.png`, `03-reports-menu-open.png`, `04-reports-export-menu-open.png`, `130-mobile-reports.png`.
- **Likely files:** `webapp/src/modules/audit-management/features/reports/views/ReportsView.vue`.
- **Expected behavior:** Primary actions, exports, report navigation, and dashboard customization are grouped and labeled clearly; hide controls have a clear restore model.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.8.

### U-002: Audit List Is Not Mobile-Friendly And Filters Truncate
- **Status:** OPEN
- **Severity:** P2
- **Area:** UI / UX / Responsive
- **Problem:** Desktop table and filter layout compress poorly on mobile; status placeholder truncates and key row actions/columns are hard to use.
- **Evidence:** `10-audits-list-base.png`, `11-audits-list-filter-dropdown-open.png`, `130-mobile-audits.png`.
- **Likely files:** `webapp/src/modules/audit-management/features/audit-dashboard/views/AuditDashboardView.vue`, shared table/filter components.
- **Expected behavior:** Mobile auditors can identify status/tracking/division and use primary row actions without desktop-table squeezing.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.9.

### U-003: New Audit Page Has Weak Progression And Excess Dead Space
- **Status:** OPEN
- **Severity:** P2
- **Area:** UX / New Audit
- **Problem:** The selected-division state leaves most of the page empty, and the primary CTA is visually disconnected from the required input path.
- **Evidence:** `20-new-audit-base.png`, `21-new-audit-division-selected.png`.
- **Likely files:** `webapp/src/modules/audit-management/features/new-audit/views/NewAuditView.vue`.
- **Expected behavior:** The page should read as a compact step flow with required fields, selected template/version context, and obvious next action.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.10.

### U-004: Long Audit Form Needs Field-Auditor Navigation Help
- **Status:** OPEN
- **Severity:** P2
- **Area:** UI / UX / Audit Form
- **Problem:** A 60+ question form renders as a long dense page without a visible section jump/progress navigator or quick path to unanswered/nonconforming items.
- **Evidence:** `30-audit-form-base.png`, `31-audit-form-collapse-all.png`, `32-audit-form-expand-all.png`.
- **Likely files:** `AuditFormView.vue`, `AuditSection.vue`, `AuditQuestionRow.vue`.
- **Expected behavior:** Auditors can jump by section, find unanswered items, and review NC/warning items without linear scrolling through the entire form.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.11.

### U-005: Corrective Actions Table Is Dense And Overdue Styling Loses Signal
- **Status:** OPEN
- **Severity:** P2
- **Area:** UI / UX / Corrective Actions
- **Problem:** With many overdue/open items, almost every row is visually heavy, which reduces scan speed; row actions are icon-heavy/subtle.
- **Evidence:** `50-corrective-actions-base.png`, `51-corrective-actions-bulk-toolbar.png`, `52-corrective-actions-bulk-close-all.png`.
- **Likely files:** `CorrectiveActionsView.vue`.
- **Expected behavior:** Overdue severity, assignment, close eligibility, and action affordances are easy to scan at table density.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.12.

### U-006: Template Manager Active View Undersells Version Workflow
- **Status:** OPEN
- **Severity:** P2
- **Area:** UI / UX / Template Admin
- **Problem:** Active template view reads like a passive question list; create-draft/edit/publish workflow is not visually explained in-context.
- **Evidence:** `60-template-manager-base.png`, `61-template-manager-division-selected.png`.
- **Likely files:** `TemplateManagerView.vue`.
- **Expected behavior:** Admins can immediately tell what is active, what is draft, how to edit safely, and when publish affects new audits.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.13.

### U-007: Report Composer Empty State Is Too Blank
- **Status:** OPEN
- **Severity:** P2
- **Area:** UX / Report Composer
- **Problem:** Composer opens to a large blank canvas and a property panel that only says to select a block; first-time users get weak guidance about Generate, drafts, or manual blocks.
- **Evidence:** `90-report-composer-base-empty.png`, `91-report-composer-manage-drafts.png`, `93-report-composer-newsletter-settings-panel.png`.
- **Likely files:** `ReportComposerView.vue` and composer canvas/property components.
- **Expected behavior:** Empty state should present primary start paths and replace the property panel with contextual help until a block is selected.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.14.

### U-008: Newsletter And Quarterly No-Data States Look Like Finished Reports
- **Status:** OPEN
- **Severity:** P2
- **Area:** UX / Reporting
- **Problem:** Newsletter and quarterly pages can render zero-filled narratives/pages without clear "no audits in this period" guidance.
- **Evidence:** `120-newsletter-base.png`, `121-quarterly-summary-base.png`.
- **Likely files:** `NewsletterView.vue`, `QuarterlySummaryView.vue`.
- **Expected behavior:** No-data report states explain the selected period, what data was checked, and what action to take next.
- **Regression linkage:** `QA_REGRESSION_CHECKLIST.md` section 16.15.

## P0 DEFECTS — Fix immediately, need Joseph approval

### P0-001: Section-Level N/A Override — NEW FEATURE (approved)
- **Status:** IN PROGRESS — implementation started 2026-04-27
- **Severity:** P0 → Feature
- **Area:** Audit Form / Template Engine / Data Integrity
- **Decision:** Remove skip logic system entirely. Replace with section-level N/A toggle:
  - Auditor can mark any visible section as "N/A for this audit" during the audit
  - A written reason is required (creates a legal audit trail)
  - Section collapses and its questions are excluded from scoring
  - N/A + reason are persisted to the database and restored on reload
  - Questions in N/A sections are not counted as unanswered
- **Scope:** Full-stack: new DB table, API endpoint changes, store state, AuditSection.vue UI
- **Regression test needed:** Mark section N/A → enter reason → verify section collapses, score updates, saves to DB, reloads correctly. Remove N/A → verify section reopens and questions are scoreable again.

---

## P1 DEFECTS — Important, need Joseph approval before fixing

### P1-002: `loadAudit()` Fires Two Identical GET Requests Per Audit Open
- **Status:** DONE — fixed 2026-04-27, collapsed to single sequential fetch
- **Severity:** P1
- **Area:** Performance / Maintainability
- **Problem:** `auditStore.ts:331-335` — `getAudit(id)` called twice inside `Promise.all`. Second call only retrieves `divisionId` which is already available from the first call's result.
- **Likely files:** `auditStore.ts:331`
- **Fix:** Fetch once, use returned `divisionId` for template call. ~3 line change.
- **Regression test needed:** Verify single GET to `/v1/audits/:id` on audit form open.

### P1-003: Bulk Draft Delete Double-Deletes the Active Draft
- **Status:** DONE — fixed 2026-04-27, removed redundant draft.deleteDraft() call
- **Severity:** P1
- **Area:** Report Composer / Draft Safety
- **Problem:** `ReportComposerView.vue:568-571` bulk-deletes via `Promise.all`, then calls `draft.deleteDraft()` again for the same ID if it was the active draft. Second DELETE hits a 404 and produces an unhandled rejection.
- **Likely files:** `ReportComposerView.vue:568`, `useReportDraft.ts:227`
- **Fix:** Remove the redundant `draft.deleteDraft()` call; reset meta state inline instead.
- **Regression test needed:** Bulk delete with active draft selected — verify one DELETE per draft, no console rejection, UI clears correctly.

### P1-004: "Generate with AI" Button Does Not Call Any AI
- **Status:** OPEN
- **Severity:** P1
- **Area:** UX / Trust / Newsletter
- **Problem:** `NewsletterView.vue:46` labels button "Generate with AI (Draft)". `NewsletterView.vue:459-468` calls `buildNarrativeDraft()` — a pure local string template, zero API calls. The real AI endpoint `generateNewsletterSummary()` exists at `auditClient.ts:1171` but is never called.
- **Decision needed from Joseph:** Wire the real AI endpoint, or rename button to "Auto-Draft Summary".
- **Regression test needed:** Button click verifies correct implementation (AI call or relabeled local).

### P1-005: Auditor Audit Count Understated in Newsletter + Quarterly Summary
- **Status:** DONE — fixed 2026-04-27, added total counter separate from scores array
- **Severity:** P1
- **Area:** Reporting / Data Accuracy
- **Problem:** `NewsletterView.vue:434-457` and `QuarterlySummaryView.vue:309-326` count audits as `stats.scores.length` — only rows where `scorePercent != null`. Audits with all-N/A responses are silently excluded. Auditor performance is misrepresented.
- **Likely files:** `NewsletterView.vue:434`, `QuarterlySummaryView.vue:309`
- **Fix:** Count rows unconditionally; keep separate `scores[]` array for average calculation.
- **Regression test needed:** Mix of scored and all-NA audits; verify count matches total rows not just scored rows.

### P1-006: Bulk Close of Corrective Actions Bypasses Closure Photo Policy
- **Status:** DONE — fixed 2026-04-27, openBulkCloseDialog() blocks and toasts when any selected item requiresClosurePhoto
- **Severity:** P1
- **Area:** Corrective Actions / Workflow Integrity / Policy Enforcement
- **Problem:** `CorrectiveActionsView.vue:365` — single close correctly gates on `requireClosurePhoto`. `CorrectiveActionsView.vue:790-808` — bulk close has zero check for `requireClosurePhoto`. Division policy can be violated at scale silently.
- **Likely files:** `CorrectiveActionsView.vue:790`
- **Fix:** Before opening bulk close dialog, check `selectedItems.value.some(i => i.requireClosurePhoto)` and block or require individual closure.
- **Regression test needed:** Select CAs including one requiring photo — verify bulk close is blocked or split.

### P1-007: DEF-0002 Navigation Blank Page — Still Open, No Closure Evidence
- **Status:** DONE — fixed 2026-04-27, added onBeforeRouteUpdate to AuditFormView.vue
- **Severity:** P1
- **Area:** Navigation / Route Stability
- **Problem:** `AuditFormView.vue:276` uses `onMounted` to load audit. Vue Router re-uses the component instance when navigating between `/audits/1` → `/audits/2`. `onMounted` never fires again. Form body renders blank indefinitely. `onBeforeRouteUpdate` is not imported or used.
- **Likely files:** `AuditFormView.vue:276`, `router/index.ts:32`
- **Fix:** Import `onBeforeRouteUpdate` from `vue-router`, add handler to call `store.loadAudit(Number(to.params.id))` on `:id` change.
- **Regression test needed:** Navigate from audit/1 to audit/2 — verify form body renders both times.

### P1-008: Newsletter Template Editor Saves to localStorage — API Never Called
- **Status:** DONE — fixed 2026-04-27, replaced localStorage with saveNewsletterTemplate/getNewsletterTemplate API calls
- **Severity:** P1
- **Area:** Newsletter / Data Integrity
- **Problem:** `NewsletterTemplateEditorView.vue:341` saves template to `localStorage`. Newsletter view reads from the API. The two systems never communicate. Every template customization saved in the editor is silently discarded when the newsletter is previewed.
- **Likely files:** `NewsletterTemplateEditorView.vue:341`, `auditClient.ts:1281`
- **Fix:** Replace localStorage read/write with `saveNewsletterTemplate()` / `getNewsletterTemplate()` API calls.
- **Regression test needed:** Save template → open newsletter → verify customizations appear.

### P1-009: QA Button Coverage Matrix Describes Removed UI
- **Status:** REOPENED — `docs/qa/button-coverage-matrix.md` exists and still carried the removed division-card contract on 2026-04-27. Matrix language was corrected to dropdown, but related specs still need verification.
- **Severity:** P1
- **Area:** Testing / Documentation Drift
- **Problem:** `docs/qa/button-coverage-matrix.md:23` — still documents "Division card — Exactly one selected card at a time." `NewAuditView.vue:17-27` — actual UI is a `<select>` dropdown. Cards do not exist.
- **Likely files:** `docs/qa/button-coverage-matrix.md`, e2e new-audit specs
- **Fix:** Update matrix and specs to reflect dropdown-based selection.

### P1-010: Division Normalization Runs Twice in Different Ways
- **Status:** DONE — fixed 2026-04-27, removed redundant normalization from auditStore.loadDivisions(); client already returns clean DivisionDto[]
- **Severity:** P1
- **Area:** Maintainability / Data Integrity
- **Problem:** `auditClient.ts:813-865` normalizes and deduplicates divisions by `code`. `auditStore.ts:279-293` normalizes and deduplicates again by `id`. Two passes, different keys.
- **Fix:** Remove normalization from `auditStore.loadDivisions()`, trust client output.

### P1-011: Handwritten Audit Client Carries Contract-Drift Risk
- **Status:** OPEN
- **Severity:** P1
- **Area:** Maintainability / API Contract
- **Problem:** `auditClient.ts:1-7` — explicit comment: NSwag cannot run due to comma in OneDrive path. 1,525-line manually maintained client. Any backend change risks silent drift.
- **Decision needed from Joseph:** Move/symlink project to path without special characters, or accept manual maintenance with contract tests added.

### P1-012: Route Guards Use Brittle String Path Fragment Matching
- **Status:** REOPENED — 2026-04-28 live/code sweep found reports and corrective-actions still use `path.includes(...)`; standalone report routes still need explicit meta.
- **Severity:** P1
- **Area:** Routing / Security / Maintainability
- **Problem:** `router/index.ts:82-108` — six consecutive `path.includes(...)` checks for admin, new audit, reports, corrective-actions. Any future route containing a matched substring will get wrong permissions.
- **Fix:** Add `meta.requiresRole` to route definitions; replace `path.includes` checks with `to.meta` checks in `beforeEach`.

### P1-013: Print Blank Form Uses sessionStorage + DOM Relocation
- **Status:** PARTIALLY MITIGATED — `PrintableAuditFormView.vue:170-177` already has an API fallback that fires when sessionStorage is empty (reload) or on a second simultaneous tab. Remaining risk is DOM relocation hack at line 193-198. Lower priority than originally assessed.
- **Severity:** P2 (downgraded)
- **Area:** Print/PDF / Reliability
- **Problem:** `AuditDashboardView.vue:231` stashes template in sessionStorage; `PrintableAuditFormView.vue:193` relocates DOM with `document.body.appendChild`. Race condition if two tabs open simultaneously. Fragile on reload.
- **Fix (optional):** Remove sessionStorage optimization; always fetch in print tab. DOM relocation can stay as a print isolation technique.

### P1-014: Section Trend Charts Ignore the Date Filter
- **Status:** DONE — fixed 2026-04-27, getSectionTrends in NewsletterView now passes dateFrom/dateTo; useReportEngine intentionally uses null,null (full history for composer trend lines)
- **Severity:** P1
- **Area:** Reporting / Logic / Trust
- **Problem:** `NewsletterView.vue:476-491` — `getSectionTrends` is called with `null, null` (no date filter) while report data is date-windowed. Q2 newsletter shows Q1/Q3/Q4 trends too. Also: `quarterDateRange()` is independently copied three times across QuarterlySummaryView, NewsletterView, ReportComposerView.
- **Fix:** Pass `dateFrom`/`dateTo` to `getSectionTrends`; extract shared `useQuarterDateRange` composable.

### P1-015: DEF-0001 (Old Card UI) Should Be Formally Closed
- **Status:** ACCEPTED AS-IS — docs/qa/defect-log.md does not exist on disk. DEF-0001 is closed in LIVE_QA_TODO.md STATUS CORRECTIONS table above.
- **Severity:** P1 (QA hygiene)
- **Area:** Testing / Documentation
- **Problem:** `docs/qa/defect-log.md:5` — DEF-0001 is Open but the card UI it references was replaced months ago. This pollutes the defect backlog.
- **Fix:** Add closure note to DEF-0001: "Card UI replaced by dropdown — defect no longer applicable."

---

## P2 DEFECTS — Polish / Lower risk

### P2-001: Delete Button Renders as "Cannot Delete" for Non-Draft Selection
- **Status:** DONE — fixed 2026-04-27, v-if now uses deleteSelection.length > 0 so button only appears when deletable items are selected
- **Severity:** P2
- **Area:** UX / Dashboard
- **Problem:** `AuditDashboardView.vue:9-17` — button shows "Cannot Delete" and is disabled when no drafts are selected, but it is still rendered and visible, creating confusion.
- **Fix:** `v-if="deleteSelection.length > 0"` or hide when no deletable items exist.

### P2-002: Print Dialog Error/Loading State Is Set But Never Rendered
- **Status:** DONE — fixed 2026-04-27, wired printError message and printLoading spinner into print dialog template
- **Severity:** P2
- **Area:** UX / Print/PDF
- **Problem:** `AuditDashboardView.vue:212-238` — `printError` and `printLoading` set in script but not bound to anything in the template. Failed template lookup is silently swallowed.
- **Fix:** Add `<p v-if="printError">{{ printError }}</p>` and `:disabled="printLoading"` to the print dialog.

### P2-003: Double `getAudit()` Also Creates Race Condition Risk
- **Status:** OPEN (sub-issue of P1-002)
- **Severity:** P2
- **Area:** Performance / Maintainability
- **Problem:** If the two parallel `getAudit(id)` calls return in different orders or fail independently, the store can end up with mismatched audit/template state.

---

## UI / UX CANDIDATES — Verified in live screen sweep

### U-001: Dashboard Action Cluster Visually Unclear
- **Status:** OPEN — live screen sweep confirmed; see U-001 in `LIVE SCREEN SWEEP RESULTS`
- **Severity:** P2
- **Area:** UI / UX
- **Problem:** Top-right dashboard actions (Refresh, filter icon, export split-button, Reports button) grouped tightly with weak visual hierarchy. Unclear which is primary or what each cluster owns.
- **What to check:** Discoverability, spacing, icon clarity, split-button clarity, redundancy between export and reports.

### U-002: Audit List Filter Placeholders Truncated
- **Status:** OPEN — live screen sweep confirmed; see U-002 in `LIVE SCREEN SWEEP RESULTS`
- **Severity:** P2
- **Area:** UI / UX
- **Problem:** `All Stat...` and `Auditor...` visible in filter bar. Filter widths may be too narrow, date fields may be hard to read, search/clear affordances compressed.
- **What to check:** Placeholder truncation, filter widths, date input readability, search/clear UX.

### U-003: New Audit Page Has Dead Space and Weak Workflow Progression
- **Status:** OPEN — live screen sweep confirmed; see U-003 in `LIVE SCREEN SWEEP RESULTS`
- **Severity:** P2
- **Area:** UI / UX
- **Problem:** Page appears sparse with large unused area. Primary CTA placement may feel disconnected from data entry. Users may not understand required vs optional fields.
- **What to check:** Wasted space, alignment of summary card + optional fields + CTA buttons, onboarding clarity.

### U-004: Corrective Actions Table Visually Heavy, Icon Actions Too Subtle
- **Status:** OPEN — live screen sweep confirmed; see U-005 in `LIVE SCREEN SWEEP RESULTS`
- **Severity:** P2
- **Area:** UI / UX
- **Problem:** Data-rich page, overdue rows may not stand out enough. Icon-only action buttons may be too subtle for non-power users.
- **What to check:** Scanability, contrast, row emphasis for overdue items, icon-only discoverability.

### U-005: Template Manager Undersells Admin Power and Next Steps
- **Status:** OPEN — live screen sweep confirmed; see U-006 in `LIVE SCREEN SWEEP RESULTS`
- **Severity:** P2
- **Area:** UI / UX / Template Admin
- **Problem:** Active version view may read like a passive list. Path to creating a draft/editing may be too hidden.
- **What to check:** Discoverability of create-draft / edit / publish actions, version workflow clarity.

### U-006: Report Composer Empty State Is Too Empty — No Onboarding
- **Status:** OPEN — live screen sweep confirmed; see U-007 in `LIVE SCREEN SWEEP RESULTS`
- **Severity:** P2
- **Area:** UI / UX / Report Composer
- **Problem:** Composer opens to a large empty canvas with a dead-looking property panel. First-time users may not know where to start.
- **What to check:** Empty-state guidance, onboarding text, action priority, property panel vs help panel when nothing selected.

### U-007: Dashboard KPI Card Hide Controls May Confuse Users
- **Status:** OPEN — live screen sweep confirmed; see U-001 in `LIVE SCREEN SWEEP RESULTS`
- **Severity:** P2
- **Area:** UI / UX / Dashboard
- **Problem:** KPI card close/hide affordances add clutter. Users may not understand if they are hiding temporarily, customizing, or permanently dismissing data.
- **What to check:** Clarity of "x" actions, customization model, accidental-hide risk, restore visibility.

### U-008: Inconsistent Date Input Patterns Across Pages
- **Status:** VERIFY — date input parity still needs a dedicated pass; no-data/reporting state is OPEN as U-008 above.
- **Severity:** P2
- **Area:** UI / UX
- **Problem:** Some pages use `From date` / `To date` text placeholders, others use `mm/dd/yyyy` formatted placeholders. Date entry behavior feels inconsistent across dashboard, audit list, reports, and composer.
- **What to check:** Date pattern consistency, locale clarity, typing vs picker affordance, filter parity.

---

## Routes Under Test

| Route | Page | Priority |
|---|---|---|
| `/audit-management/reports` | Dashboard / KPI | P1 |
| `/audit-management/audits` | Audit Dashboard (list) | P1 |
| `/audit-management/audits/new` | New Audit (dropdown picker) | P1 |
| `/audit-management/audits/:id` | Audit Form (question scoring) | P1 |
| `/audit-management/audits/:id/review` | Audit Review | P1 |
| `/audit-management/corrective-actions` | Corrective Actions | P1 |
| `/audit-management/admin/templates` | Template Manager | P2 |
| `/audit-management/admin/settings` | Email Routing | P2 |
| `/audit-management/admin/users` | User Management | P2 |
| `/audit-management/admin/audit-log` | Audit Log | P2 |
| `/audit-management/reports/composer` | Report Composer | P2 |
| `/audit-management/reports/gallery` | Generate Report | P2 |
| `/audit-management/reports/scheduled` | Scheduled Reports | P3 |
| `/audit-management/reports/by-employee` | Audits by Employee | P3 |
| `/audit-management/newsletter/template-editor` | Newsletter Template Editor | P3 |

---

## Critical User Journeys

1. **Create new audit** → pick division from dropdown → fill form → submit → verify tracking number assigned
2. **Corrective action lifecycle** → open CA from audit → assign owner → close with photo → verify status
3. **Template admin** → clone version → add question → reorder → publish → verify new audit binds new version
4. **Report Composer** → open composer → load draft → author narrative block → save → print/export
5. **Distribution** → complete audit → generate distribution preview → send email
6. **Skip logic** → set trigger response → verify section shows/hides correctly (P0-001 test)
7. **Bulk close CAs with photo requirement** → verify policy is enforced (P1-006 test)

---

## Decisions Joseph Must Make Before Coding Starts

| Item | Decision Required |
|---|---|
| P0-001 | Fix Map key on client (versionQuestionId) OR change server DTO to use stable questionId |
| P1-004 | Wire real AI endpoint OR rename button to "Auto-Draft Summary" |
| P1-011 | Move project folder off OneDrive special-char path to unblock NSwag, or accept manual client |

---

## Recommended Fix Order

| Priority | ID | Fix | Effort |
|---|---|---|---|
| 1 | P0-001 | Skip logic ID mismatch | Medium — needs decision |
| 2 | P1-006 | Bulk close photo policy bypass | Small |
| 3 | P1-007 | Nav blank page (onBeforeRouteUpdate) | Small |
| 4 | P1-008 | Newsletter editor → API save | Small |
| 5 | P1-002 | Remove double getAudit() call | Tiny |
| 6 | P1-003 | Remove double-delete in bulk draft delete | Tiny |
| 7 | P1-005 | Fix auditor count in reports | Small |
| 8 | P1-009 | Update QA docs for dropdown new-audit UI | Small |
| 9 | P1-012 | Replace string-fragment route guards | Medium |
| 10 | P1-014 | Pass date window to getSectionTrends; extract shared util | Small |
| 11 | P1-015 | Close DEF-0001 in defect log | Tiny |
| 12 | P2-001 | Hide dead delete button | Tiny |
| 13 | P2-002 | Wire printError/printLoading into dialog | Tiny |
| 14 | P1-013 | sessionStorage print flow | Medium |
| 15 | U-001–U-008 | UI/UX polish pass | After P1s |
| — | **AI FEATURES — DO LAST** | Requires Cloudflare pipeline + AI infrastructure before any wiring | — |
| LAST | P1-004 | "Generate with AI" button — rename or wire real endpoint | Blocked on pipeline |
| LAST | BM-AI-1 | CA remediation suggestions via AI | Blocked on pipeline |
| LAST | BM-AI-2 | AI repeat pattern detection across audits | Blocked on pipeline |
| LAST | BM-AI-3 | AI-drafted distribution email body | Blocked on pipeline |

---

## Benchmark Research
_Completed: 2026-04-27 | Sources: SafetyCulture, Cority, Intelex, VelocityEHS, Benchmark Gensuite, GoAudits, Pervidi, ETQ Reliance, Enablon, Opus iO_

### Where Stronghold Is Already Ahead of the Market
| Differentiator | Evidence |
|---|---|
| Native AI audit narrative on submission | Claude Haiku integration exists — no comparator has this as a default |
| Field-level change trail (before/after diffs) | `AuditTrailInterceptor` — most SMB competitors only log status changes |
| Per-division SLA, scoring targets, email routing, section enabling | Architecturally richer than GoAudits/SafetyCulture |
| Integrated newsletter + distribution engine | No competitor offers this as part of audit workflow |
| NormalUser scoped CA-only portal | Clean CA closing experience for non-auditors without full portal access |
| Weighted scoring + life-critical override | Matches mid-market enterprise (Cority/Intelex), not just pass/fail |

### Top 10 Market Gaps — Fix Order

| # | Gap | Effort | Impact |
|---|---|---|---|
| 1 | **No autosave** — user loses work on browser close or accidental navigation | Small (frontend only — `SaveAuditResponses` already supports partial saves) | Critical trust issue |
| 2 | **No CA escalation chain** — overdue CAs only remind assignee, never escalate to manager | Small-medium | Compliance control gap |
| 3 | **Score breakdown not visible** — users see final % but not which sections drove it | Small (extend `GetAuditReport` response) | Trust + transparency |
| 4 | **PDF photo embedding unconfirmed** — finding photos may not appear inline in generated PDF | Verify then fix | Evidence quality |
| 5 | **No repeat finding surface in dashboard** — `GetRepeatFindings` endpoint exists but no UI widget | Small (endpoint exists) | Compliance value |
| 6 | **No CA root cause fields** — no `RootCauseCategory`, `CorrectiveMeasure`, `EffectivenessVerifiedAt` | Medium (migration + UI) | ISO 45001 readiness |
| 7 | **No section jump nav on long audit forms** — auditors scroll linearly through 60+ questions | Small | Field usability |
| 8 | **No photo annotation** — photos upload but cannot be marked up with arrows/circles/text | Medium | Evidence quality |
| 9 | **No audit scheduling / compliance calendar** | Large | Planning & regulatory |
| 10 | **No onboarding / empty states** — new division gets blank dashboard with no direction | Small-medium | First-use experience |

### Top 3 "Do This Now" from Benchmark
1. **Autosave** — debounced auto-call to `SaveAuditResponses` on every response change. Frontend only, no migration, no backend changes needed. 1-2 days.
2. **Overdue CA escalation to manager** — add `EscalationAfterDays` to Division config; extend `CaReminderService`. 2-3 days.
3. **Per-section score subtotals** — extend `GetAuditReport` to include section-level breakdown; surface in `AuditReviewView` and as read-only in `AuditFormView`. 2 days.

---

## Approval Gate
No code changes are made until Joseph explicitly approves a specific item.
Evidence packet path: `docs/qa-evidence/2026-04-27-sweep/`
