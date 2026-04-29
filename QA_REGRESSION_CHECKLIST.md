# QA Regression Checklist — Stronghold Audit App
_Last updated: 2026-04-27 | Aligned to master audit sweep + full-stack benchmark backlog_

## Instructions
Run this checklist before every release and after every feature touching more than one page.
Check = executed and evidenced. Do not mark pass without screenshot or test output.
Date patterns updated to reflect current UI (dropdown picker, not division cards).

---

## 1. API Health (Pre-flight)
- [ ] `GET /v1/divisions` → 200
- [ ] `GET /v1/audits` → 200
- [ ] `GET /v1/admin/templates` → 200
- [ ] `GET /v1/reports/compliance-status` → 200

**Command:** `npm --prefix webapp run qa:baseline`

---

## 2. Core Audit Workflow (P1 — must pass every release)

### 2.1 Audit Dashboard
- [ ] `/audit-management/audits` loads without blank screen
- [ ] Audit list renders rows with tracking numbers (not "—")
- [ ] Filters work (division, status, date range)
- [ ] Filter placeholders are not truncated (`All Statuses`, `Auditor...` readable)
- [ ] Clicking a row navigates to Audit Form and form body renders (not blank) ← **DEF-0002 / P1-007**
- [ ] Navigating back and clicking a DIFFERENT audit also renders form body ← **P1-007 regression**

### 2.2 New Audit (Dropdown UI — cards removed)
- [ ] `/audit-management/audits/new` loads division **dropdown** (not cards)
- [ ] Dropdown shows all division names correctly (not blank)
- [ ] Job prefix / site code fields appear for selected division
- [ ] Submitting creates audit and redirects to Audit Form
- [ ] New audit appears in dashboard list
- [ ] Tracking number is assigned in correct format (e.g. `H26-001-VCC`)
- [ ] **NOTE:** QA docs referencing "division card selection" are now stale — `button-coverage-matrix.md` needs update ← **P1-009**

### 2.3 Audit Form
- [ ] Questions load from active template version
- [ ] Can save responses (Conforming / Non-Conforming / Warning / N/A)
- [ ] Non-conforming response triggers corrective action creation
- [ ] Score summary bar updates on response change
- [ ] Attachments can be uploaded
- [ ] Submit button transitions audit from Draft → Submitted

### 2.4 Skip Logic (P0-001 regression — must verify after fix)
- [ ] Create template rule: Q-A = NonConforming → hide Section B
- [ ] Set Q-A to NonConforming — verify Section B disappears
- [ ] Clear Q-A — verify Section B reappears
- [ ] Clone template version — verify rules still fire correctly on new version
- [ ] Dynamically shown question accepts input and saves correct response ID (not 0)

### 2.5 Audit Review
- [ ] `/audit-management/audits/:id/review` loads review summary
- [ ] CAs listed with correct status
- [ ] Close/Reopen audit controls present for correct roles

---

## 3. Corrective Actions

### 3.1 Single CA lifecycle
- [ ] `/audit-management/corrective-actions` loads CA list
- [ ] Can assign owner to CA
- [ ] Can close CA with closure notes
- [ ] If `requireClosurePhoto = true`: close button disabled until photo uploaded
- [ ] Status updates reflected in Audit Form

### 3.2 Bulk close (P1-006 regression — must verify after fix)
- [ ] Select multiple CAs — if any require closure photo: bulk close is **blocked** with explanation
- [ ] Select only CAs that do NOT require photo: bulk close proceeds normally
- [ ] Bulk close does not bypass any policy ← **P1-006**

---

## 4. Navigation Stability (P1-007 regression)
- [ ] Navigate Audits → Reports → CAs → Audits (×5 cycles) — no blank page
- [ ] Open audit form `/audits/5` → navigate back → open `/audits/8` → form body renders
- [ ] Back-button after audit form does not blank the page
- [ ] **Command:** `npm --prefix webapp run test:e2e:audit:live-guard`

---

## 5. Template Manager (P2)
- [ ] `/audit-management/admin/templates` loads with templates list
- [ ] Template version selector shows valid name (not "undefined")
- [ ] Clone version button present and functional
- [ ] Add question/section controls present and functional
- [ ] Drag-and-drop reorder works and persists
- [ ] Publish button present and transitions version to Active
- [ ] After publish: new audit creation binds to newly published version
- [ ] **Command:** `PW_AUDIT_TEMPLATE_GATE=true npm --prefix webapp run test:e2e:audit:template-gate`

---

## 6. Reports / KPI Dashboard (P2)
- [ ] `/audit-management/reports` loads KPI cards
- [ ] Division/status/date filters present and functional
- [ ] Auditor row count matches actual audit count (not just scored rows) ← **P1-005 regression**
- [ ] Compliance trend chart renders
- [ ] Section trend data is scoped to the same date window as the report ← **P1-014 regression**
- [ ] Export action present and produces correct output
- [ ] **Command:** `PW_AUDIT_REPORTING_GATE=true npm --prefix webapp run test:e2e:audit:reporting-gate`

---

## 7. Newsletter (P1-008 regression)
- [ ] Template editor save calls API (not localStorage)
- [ ] Opening newsletter after saving template shows saved customizations
- [ ] "Generate with AI" button behavior matches label (either real AI or renamed)
- [ ] Section trends in newsletter are scoped to selected date window ← **P1-014**

---

## 8. Report Composer (P1-003 regression)
- [ ] `/audit-management/reports/composer` loads without Tiptap error
- [ ] Narrative editor block mounts and accepts text input
- [ ] Draft saves and reloads correctly
- [ ] **Bulk delete with active draft selected:** only one DELETE per draft, no console 404 ← **P1-003**
- [ ] Print / export flow completes
- [ ] **Command:** `PW_AUDIT_COMPOSER_GATE=true npm --prefix webapp run test:e2e:audit:composer-gate`

---

## 9. Print Blank Form (P2-002 / P1-013 regression)
- [ ] Print dialog opens from dashboard
- [ ] If no active template: error message is shown in dialog (not silent) ← **P2-002**
- [ ] Print button shows loading state while fetching
- [ ] Reloading the print tab does not blank or crash it ← **P1-013**
- [ ] Printing from two tabs simultaneously doesn't corrupt template data

---

## 10. Admin Pages (P2)
- [ ] `/audit-management/admin/users` loads user list
- [ ] Can set/remove audit role for user
- [ ] `/audit-management/admin/settings` loads email routing config
- [ ] `/audit-management/admin/audit-log` loads log entries

---

## 11. Route Guards (P1-012 regression)
- [ ] Non-admin user cannot access `/admin/templates` or `/admin/users`
- [ ] Adding a new route at `/audit-management/admin-summary` does NOT accidentally match `/admin/` guard
- [ ] New-audit access enforced by role, not path string

---

## 12. Visual Regression (all pages)
- [ ] Desktop (1280×720), Tablet (768×1024), Mobile (390×844)
- [ ] Dark mode and Light mode
- [ ] No layout overflow, missing icons, or broken cards
- [ ] KPI card hide/show controls are understandable ← **U-007**
- [ ] Date input patterns consistent across pages ← **U-008**
- [ ] **Command:** `npm --prefix webapp run test:e2e:audit:visual:all`

---

## 13. DB Write Verification
- [ ] Audit create → row in `audit.Audit` with non-null `TrackingNumber`
- [ ] CA close → `audit.CorrectiveAction` updated with `ClosedAt` and `Status='Closed'`
- [ ] Response save → row in `audit.AuditResponse` with correct `QuestionId` and `AuditId` (not 0) ← **P0-001**
- [ ] Template publish → `Status='Active'` on correct version row
- [ ] Newsletter template save → row in `audit.NewsletterTemplate` (not localStorage) ← **P1-008**

---

## 14. QA Doc Freshness Check
- [ ] `docs/qa/button-coverage-matrix.md` — no entries referencing "division card selection" ← **P1-009**
- [ ] `docs/qa/defect-log.md` — DEF-0001 marked closed with reason ← **P1-015**
- [ ] All e2e specs targeting new-audit flow use dropdown selectors, not card selectors

## 15. Full-Stack Benchmark Audit Backlog
These items are not pass/fail release gates until the next authorized sweep verifies them. They define the regression coverage that must be added or updated when a matching finding is confirmed and Joseph approves a fix.

### 15.1 External Benchmark Coverage
- [ ] Build benchmark matrix from current compliance audit / inspection / EHS / field-audit products.
- [ ] Compare Stronghold against market patterns for audit creation, template/versioning, mobile evidence, corrective actions, overdue management, dashboards, reports/exports, PDF/print, filters, admin usability, score transparency, onboarding, autosave/drafts, navigation, and discoverability.
- [ ] Record market-gap recommendations as `VERIFY`, not `OPEN`, until tied to Stronghold evidence.

### 15.2 Visual and UX Candidate Coverage
- [ ] U-001 Dashboard action cluster: desktop/tablet/mobile screenshots, hover/tooltips, split-button clarity, export vs reports affordance.
- [ ] U-002 Audit list filters: placeholder truncation, date-field readability, search/apply/clear layout parity.
- [ ] U-003 New audit page: required-field clarity, workflow progression, CTA placement, empty/dead-space evidence.
- [ ] U-004 Corrective actions: scanability, overdue emphasis, icon-only action discoverability, KPI/table relationship.
- [ ] U-005 Template manager: create draft/edit/publish discoverability and active-vs-draft version confidence.
- [ ] U-006 Report composer: empty-state onboarding, property panel behavior with no selection, left-toolbar density.
- [ ] U-007 Dashboard hide controls: accidental-hide behavior, restore path, persistence semantics.
- [ ] U-008 Date inputs: consistent wording/formatting across dashboard, audit list, reports, newsletter, composer.

### 15.3 Logic and Workflow Candidate Coverage
- [ ] B-001 Skip logic: trigger IDs remain correct after template clone/publish and dynamically shown questions save real response IDs.
- [ ] B-002 Audit form load: exactly one `GET /v1/audits/:id` per form load, no duplicate request fallback.
- [ ] B-003 Composer draft bulk delete: active selected draft deleted once, no duplicate 404, UI state clears.
- [ ] B-004 Newsletter generation: button label matches implementation, whether local auto-draft or real AI call.
- [ ] B-005 Reporting counts: audit counts and scored response counts are separately named and mathematically correct.
- [ ] B-006 Corrective-action bulk close: closure-photo policy cannot be bypassed and does not dead-end.
- [ ] B-007 Audit dashboard delete-selected: no visible `Delete Selected (0)` or equivalent dead action.
- [ ] B-008 Print blank form dialog: loading and error states are visible and actionable.
- [ ] B-009 New audit QA contract: docs/specs use dropdown workflow, not removed division cards.
- [ ] B-010 Division normalization: one owner and one dedupe rule verified against backend/source data.
- [ ] B-011 Audit API client contract: generated-client path restored or contract tests cover handwritten DTO drift.
- [ ] B-012 Route guards: permissions derive from route metadata/roles, not fragile path fragments.
- [ ] B-013 Blank-form print: direct navigation, reload, multi-tab print, API failure, and browser print timing covered.
- [ ] B-014 Reporting periods: custom range, quarter mode, trend labels, exports, and summaries share one canonical period.
- [ ] B-015 New-audit navigation: historical regression reproduced or closed with live evidence.

### 15.4 Evidence Requirements
- [ ] Evidence stored under `docs/qa-evidence/<sweep-id>/`.
- [ ] Markdown handoff stored under `docs/` with exact screenshot paths and file paths.
- [ ] Each confirmed item has status `OPEN`, `BLOCKED`, or `ACCEPTED AS-IS`; unproven items stay `VERIFY`.
- [ ] Regression additions identify automated, manual-only, or both.

---

## 15. Section N/A Override

### 15.1 Basic flow
- [ ] Section header shows N/A toggle button (not shown on submitted audits)
- [ ] Clicking N/A toggle shows inline reason input
- [ ] Reason field is required — confirm button disabled until text entered
- [ ] Confirming N/A: section collapses, header shows "N/A — [reason]" badge, questions hidden
- [ ] Score bar updates immediately — N/A section questions excluded from denominator
- [ ] Save Draft → reload → section still shows N/A with correct reason
- [ ] Submit audit → review page shows section as N/A (not as 0/N unanswered)

### 15.2 Clear / undo
- [ ] Clicking N/A badge or toggle again prompts to clear it
- [ ] After clearing: section expands, questions are answerable, score recalculates

### 15.3 Permissions and states
- [ ] On a submitted/closed audit: N/A toggle is NOT shown (read-only)
- [ ] On a reopened audit: N/A toggle IS shown
- [ ] Print view: N/A sections print as a single line "SECTION NAME — Not Applicable: [reason]"

### 15.4 DB verification
- [ ] `audit.AuditSectionNaOverride` row created with correct AuditId, SectionId, Reason
- [ ] Clearing N/A deletes the row
- [ ] Score calculation excludes N/A section questions from denominator

## 16. Live Screen Sweep Regression Targets

Evidence source: `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/`

### 16.1 Section N/A backend parity
- [ ] Answer at least one question in a section, save, then mark that section N/A with a required reason.
- [ ] Save/reload/submit and verify score denominator, unanswered count, review findings, report findings, and CA workload exclude that section.
- [ ] Required evidence: audit form screenshot, review screenshot, report screenshot, and DB/API response proving excluded responses are not counted.
- [ ] Coverage type: automated API/integration plus manual UI.

### 16.2 Legacy skip logic decision
- [ ] If skip logic is removed: verify no visible admin/runtime rule controls remain and no runtime form path writes `questionId = 0`.
- [ ] If skip logic stays: create rule, publish, start audit, answer trigger, clone/publish again, and verify rule behavior survives.
- [ ] Required evidence: template admin screenshot, audit form screenshot before/after trigger, and saved response payload.
- [ ] Coverage type: automated E2E plus API/DB check.

### 16.3 Dev API port preflight
- [ ] Fresh `dev-start.bat` run starts the API on the same port used by the frontend API base.
- [ ] `GET /v1/divisions` returns 200 before screenshots or live E2E begins.
- [ ] Required evidence: command output or health log plus screenshot sweep log showing `apiBase`.
- [ ] Coverage type: automated preflight.

### 16.4 Admin Audit Log tabs and expansion
- [ ] `/audit-management/admin/audit-log` renders one selected tab panel at a time.
- [ ] Action Log row expansion works without console error.
- [ ] Change Trail row expansion works without console error.
- [ ] Required evidence: screenshots for each tab and expanded row; console log must not include `expandedRows`.
- [ ] Coverage type: automated E2E plus visual.

### 16.5 Auditor new-audit permission contract
- [ ] Auditor role can access `/audit-management/audits/new` only if all required APIs are auditor-accessible.
- [ ] New Audit page does not require `/v1/admin/*` endpoints for non-admin audit creation.
- [ ] Required evidence: network log and created audit screenshot.
- [ ] Coverage type: automated permission E2E.

### 16.6 Reporting count semantics
- [ ] Audit volume count includes scored and all-N/A/unscored audits.
- [ ] Average score count excludes unscored rows and is labeled separately.
- [ ] Required evidence: fixture data, reports screenshot, and exported data comparison.
- [ ] Coverage type: API/integration plus E2E.

### 16.7 Route guard metadata
- [ ] Reports, corrective actions, newsletter, quarterly summary, print, and admin routes are guarded by route meta/capabilities.
- [ ] Adding a route with a similar path substring does not inherit unrelated permissions.
- [ ] Required evidence: route matrix for admin, auditor, CA-only user, and no-role user.
- [ ] Coverage type: unit/router plus E2E.

### 16.8 Dashboard action cluster
- [ ] Dashboard actions are grouped by purpose and have clear labels/tooltips.
- [ ] Hide/customize controls explain restore behavior.
- [ ] Desktop/tablet/mobile screenshots show no cramped action cluster.
- [ ] Coverage type: visual/manual.

### 16.9 Audit list mobile and filters
- [ ] Mobile audit list exposes tracking number, status, division, date, and primary action without desktop-table squeezing.
- [ ] Filter placeholders render fully or use labels at desktop/tablet/mobile.
- [ ] Coverage type: visual/manual plus E2E screenshot.

### 16.10 New Audit progression
- [ ] Selected division state shows required fields, active template/version context, and a nearby primary action.
- [ ] Empty/dead space is reduced without hiding optional fields.
- [ ] Coverage type: visual/manual.

### 16.11 Audit form navigation
- [ ] Long forms provide section jump/progress and quick access to unanswered/NC/warning items.
- [ ] Collapse/expand does not hide validation state or submit blockers.
- [ ] Coverage type: visual/manual plus E2E.

### 16.12 Corrective action scanability
- [ ] Overdue state remains visible without every row becoming visually identical.
- [ ] Row actions are discoverable by non-power users at desktop and mobile sizes.
- [ ] Coverage type: visual/manual.

### 16.13 Template manager workflow clarity
- [ ] Active/draft/published states and create-draft/edit/publish flow are visible in context.
- [ ] Admin can tell when a change affects future audits versus existing audits.
- [ ] Coverage type: visual/manual plus template contract E2E.

### 16.14 Report composer empty state
- [ ] Blank composer canvas offers clear primary start options.
- [ ] Property panel shows contextual help until a block is selected.
- [ ] Coverage type: visual/manual plus composer E2E.

### 16.15 Reporting no-data states
- [ ] Newsletter and quarterly summary show explicit no-data messaging for empty periods.
- [ ] Zero-filled reports do not read like successful performance summaries.
- [ ] Coverage type: visual/manual plus reporting E2E.

---

## Sign-off
| Check | Owner | Date |
|---|---|---|
| All P1 items pass | | |
| P0-001 skip logic verified | | |
| No new High defects | | |
| Visual baselines captured | | |
| DB writes verified | | |
| QA docs aligned to current UI | | |
| Joseph approved all enhancement options | | |
