# QA_AUDIT_20260427_SCREEN_SWEEP

Date finalized: 2026-04-28

Scope: Stronghold compliance audit app only. Surfaces covered include audit dashboard, audit list, new audit, audit form, review, corrective actions, template manager, admin settings/users/audit log, reports dashboard, newsletter, quarterly summary, report composer, gallery, scheduled reports, by-employee reports, print routes, tablet, and mobile views.

Policy: no application code was changed during this audit package. QA artifacts, screenshots, and this report were updated.

## Executive Summary

The live screenshot sweep is complete and valid. The first attempt would have produced false evidence because the API was not listening on the frontend-expected port. After confirming and starting the API on `http://localhost:7221`, Playwright captured 62 live screenshots against real app data.

The app has strong bones: audit creation, audit forms, corrective actions, templates, reporting, composer, newsletter, and admin surfaces all exist and load. The biggest risks are not "missing pages"; they are trust and workflow issues: section N/A parity, legacy skip logic drift, route guard fragility, misleading newsletter/reporting labels, admin audit log runtime errors, mobile/table UX, and no-data/empty-state weakness.

## Evidence Packet

- Screenshot root: `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/screenshots/`
- Sweep log: `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/sweep-log.json`
- Capture harness: `docs/qa-evidence/QA_AUDIT_20260427_SCREEN_SWEEP/screenshot-sweep.spec.ts`
- Screenshot count: 62 PNGs
- Playwright result: 1 passed in 53.4s
- API base: `http://localhost:7221`
- Selected evidence:
  - `01-reports-dashboard-base.png`
  - `10-audits-list-base.png`
  - `21-new-audit-division-selected.png`
  - `30-audit-form-base.png`
  - `50-corrective-actions-base.png`
  - `61-template-manager-division-selected.png`
  - `78-admin-audit-log-action-tab.png`
  - `80-admin-audit-log-expanded-row.png`
  - `90-report-composer-base-empty.png`
  - `120-newsletter-base.png`
  - `121-quarterly-summary-base.png`
  - `130-mobile-audits.png`
  - `130-mobile-reports.png`

## External Benchmark Summary

Sources used:
- SafetyCulture inspections/reporting help and product pages: https://help.safetyculture.com/en-US/000009/ and https://safetyculture.com/inspections-and-reports/
- SafetyCulture report layouts: https://help.safetyculture.com/en-US/003189/
- GoAudits app, reports, workflows, analytics pages: https://goaudits.com/, https://goaudits.com/app/electronic-forms/, https://goaudits.com/workflows/, https://goaudits.com/reports/
- Cority audit and inspection software: https://www.cority.com/corityone/audit-inspections/
- Intelex audit/CAPA/mobile pages: https://www.intelex.com/resources/product-demo/audit-management-software, https://www.intelex.com/products/applications/capa-software-corrective-and-preventive-action, https://www.intelex.com/products/mobile

Benchmark matrix:

| Pattern | Market evidence | Stronghold status | Gap/opportunity |
|---|---|---|---|
| Mobile/offline inspections | SafetyCulture and GoAudits emphasize mobile/offline capture and sync. | Desktop web is usable; mobile audit list/form need work. | P1/P2 opportunity for mobile-first field audit mode and autosave. |
| Required fields and guided completion | SafetyCulture marks required questions and reviews flagged responses before completion. | Submit blockers exist, but long-form navigation is weak. | Add section progress, unanswered/NC jump list, and clear submit blockers. |
| Instant reports/export | SafetyCulture/GoAudits generate reports after inspection. | Reports, quarterly, newsletter, composer exist. | Improve no-data states, period consistency, PDF/photo confidence. |
| Corrective action workflows | GoAudits and Intelex emphasize assignment, due dates, reminders, root cause, CAPA history. | CAs exist with bulk actions and photo policy work. | Add escalation chain, root cause/effectiveness verification, clearer scanability. |
| Template builders | SafetyCulture and GoAudits highlight drag/drop, response types, logic, AI/template import. | Stronghold has template manager and versioning. | Make active/draft/publish workflow more obvious; resolve skip logic or remove it. |
| Dashboards and drilldown | Cority/GoAudits emphasize dashboards, trends, actions, BI. | Stronghold dashboard is data rich. | Rework dashboard action hierarchy and surface repeat findings/section score drivers. |
| Report customization | SafetyCulture report layouts and email templates are first-class. | Stronghold composer is powerful but blank on entry. | Add first-use guidance and safer draft/autosave confidence. |
| Audit-ready history | Intelex/CAPA pages emphasize complete history/audit trail. | Stronghold has admin audit log and change trail. | Fix audit log tabs/expansion runtime bug. |
| Scheduling/reminders | Cority and GoAudits emphasize schedules, reminders, escalations. | Scheduled reports exist; audit scheduling/escalation is limited. | Add CA escalation and audit planning calendar later. |
| Evidence quality | SafetyCulture/GoAudits mention photos, annotation, e-signatures, GPS. | Stronghold supports photos but annotation/mobile evidence is weaker. | Add photo annotation/evidence thumbnails after trust fixes. |

Top market patterns worth adapting:
- Mobile-first audit execution with autosave/offline confidence.
- Section progress and submit-readiness guidance.
- Corrective action escalation, reminders, root cause, and effectiveness verification.
- Instant report generation with clear PDF/photo evidence.
- Template workflow clarity: active, draft, publish, archive.
- Report/layout customization with starter templates.
- No-data states that explain what period was checked.
- Dashboard drilldowns from KPI to exact audits/CAs.
- Strong permission/scoping audit trail.
- Scheduled audits and recurring compliance calendar.

Where Stronghold can differentiate:
- Division-specific audit governance and scoring targets.
- Integrated newsletter/report composer instead of only raw exports.
- Section N/A reason trail for legal defensibility.
- Focused audit management for Stronghold workflows instead of broad generic EHS bloat.
- Admin-controlled email routing and score thresholds.
- Repeat finding intelligence across audit history.
- Evidence-backed QA discipline with screenshot sweep artifacts.
- Tight CA-only portal for normal users.
- Weighted scoring and life-critical overrides.
- Custom executive reporting if period logic is made trustworthy.

## Verified Bugs

### P0-002: Section N/A Override Is Not Yet End-to-End Enforced
- **Status:** OPEN
- **Severity:** P0
- **Area:** Data Integrity
- **Problem:** Frontend scoring can exclude section-N/A questions, but save/review/report paths can still persist and count old responses from that section.
- **Why it matters:** N/A must be a defensible audit decision. If old NC answers keep affecting review/report output, score and CA workload are not trustworthy.
- **User impact:** Auditors may mark a section N/A and still see findings or score changes from hidden responses.
- **Likely root cause:** N/A handling is split across frontend score state and backend response/report readers.
- **Likely files:** `auditStore.ts`, `SaveAuditResponses.cs`, `GetAuditReview.cs`, `GetAuditReport.cs`.
- **Recommended fix:** Enforce section N/A on the backend as the source of truth and exclude those section responses from save counts, submit validation, review findings, report findings, and scoring.
- **Regression test needed:** Answer section, mark N/A, save/reload/submit; assert score, unanswered count, review, report, and CAs exclude that section.
- **Evidence collected:** `33-audit-form-section-na-dialog-or-state.png`; logic/code agent file review.

### P0-003: Legacy Skip Logic Still Has Identifier Drift Or Must Be Fully Removed
- **Status:** VERIFY
- **Severity:** P0
- **Area:** Template Admin
- **Problem:** Rule triggers use version-question IDs while runtime responses are keyed by question IDs; dynamic fallback can write `questionId = 0`. Clone logic does not preserve rules.
- **Why it matters:** Conditional sections can silently hide/show incorrectly, which affects audit scope and score.
- **User impact:** Auditors may answer the wrong visible form state or lose rule behavior after template clone/publish.
- **Likely root cause:** Two identifier systems are mixed in template/runtime state.
- **Likely files:** `auditStore.ts`, `AuditSection.vue`, `AuditQuestionRow.vue`, `CloneTemplateVersion.cs`, `QuestionLogicRule.cs`.
- **Recommended fix:** Either fully remove/deactivate skip logic per Joseph's section-N/A direction, or standardize on one canonical runtime identifier and clone rules safely.
- **Regression test needed:** Publish rule, trigger it, answer newly shown section, clone/publish, verify rule still fires and no saved response ID is 0.
- **Evidence collected:** Logic/code agent file review.

### P1-016: Local API Port Mismatch Breaks Live QA And Causes False Screenshots
- **Status:** OPEN
- **Severity:** P1
- **Area:** Testing
- **Problem:** Frontend expected API on 7221 while API was initially listening on 5221; live pages showed failed loads.
- **Why it matters:** Screenshot evidence is invalid if the API is down or on the wrong port.
- **User impact:** QA sees broken screens and could report visual defects caused by environment failure.
- **Likely root cause:** Dev launch settings and frontend/API base are not aligned.
- **Likely files:** `dev-start.bat`, `Api/Properties/launchSettings.json`, webapp env/API config.
- **Recommended fix:** Align dev start profile/ports and add a mandatory API health preflight before live screenshot capture.
- **Regression test needed:** Fresh dev start, then `GET /v1/divisions` from configured API base returns 200 before Playwright begins.
- **Evidence collected:** User screenshot with "Failed to load audit"; sweep log `apiBase: http://localhost:7221`.

### P1-017: Admin Audit Log Tabs And Expansion Are Broken
- **Status:** OPEN
- **Severity:** P1
- **Area:** UI
- **Problem:** Admin Audit Log renders mixed tab content and row expansion throws `this.expandedRows is not iterable`.
- **Why it matters:** The audit log is the trust surface for non-repudiation. It cannot have runtime errors.
- **User impact:** Admins cannot reliably inspect action/change history.
- **Likely root cause:** PrimeVue 3 app appears to use a tab/expanded-row pattern that does not match the installed component contract.
- **Likely files:** `AdminAuditLogView.vue`, PrimeVue DataTable/Tabs usage.
- **Recommended fix:** Use PrimeVue 3-compatible tab components or explicit conditional panels; make `expandedRows` match DataTable's expected type and `dataKey` behavior.
- **Regression test needed:** Switch tabs and expand a row in each tab with no console errors and only the selected panel visible.
- **Evidence collected:** `78-admin-audit-log-action-tab.png`, `79-admin-audit-log-change-trail-tab.png`, `80-admin-audit-log-expanded-row.png`, `sweep-log.json`.

### P1-018: Auditor Create-Audit Permission Can Route Into Admin-Only Template APIs
- **Status:** OPEN
- **Severity:** P1
- **Area:** Permissions
- **Problem:** Auditor users can be allowed to create audits, but New Audit loads admin-only template/draft endpoints.
- **Why it matters:** The route permission model and API contract disagree.
- **User impact:** Auditors may reach New Audit but fail to load required division/template data.
- **Likely root cause:** New Audit reuses admin template APIs instead of non-admin active-template lookup.
- **Likely files:** `NewAuditView.vue`, `userStore.ts`, `GetTemplates.cs`, `GetDraftVersionDetail.cs`.
- **Recommended fix:** Provide auditor-safe template lookup for active templates, or block/hide create audit unless API access matches.
- **Regression test needed:** Auditor role opens New Audit and creates an audit without `/v1/admin/*` calls.
- **Evidence collected:** Logic/code agent file review.

### P1-019: Reports Auditor Count Still Appears To Count Scored Rows
- **Status:** OPEN
- **Severity:** P1
- **Area:** Reporting
- **Problem:** Reports auditor count appears to use scored rows rather than audit rows.
- **Why it matters:** Auditor productivity can be understated when audits are all-N/A or otherwise unscored.
- **User impact:** Managers get misleading volume/performance data.
- **Likely root cause:** UI aggregates `scores.length` as count in one report surface.
- **Likely files:** `ReportsView.vue`.
- **Recommended fix:** Separate "audit count" from "scored audit count" and label both accurately.
- **Regression test needed:** Mix scored and unscored audits and verify counts in dashboard/table/export.
- **Evidence collected:** Logic agent review; `01-reports-dashboard-base.png`, `05-reports-tab-performance.png`.

### P1-020: Route Guards Still Have String-Fragile Checks
- **Status:** OPEN
- **Severity:** P1
- **Area:** Routing
- **Problem:** Router still uses `path.includes('/reports')` and `path.includes('/corrective-actions')`; standalone report routes lack explicit meta.
- **Why it matters:** Route renames or similar path fragments can silently change permissions.
- **User impact:** Users can be blocked or allowed incorrectly.
- **Likely root cause:** Partial migration from path-based checks to route-meta checks.
- **Likely files:** `webapp/src/router/index.ts`, audit module router, `userStore.ts`.
- **Recommended fix:** Move all audit permission checks to route meta and capability checks.
- **Regression test needed:** Permission matrix for admin, auditor, CA-only, and no-role users across all audit routes.
- **Evidence collected:** Code review of `webapp/src/router/index.ts`.

### P1-021: Newsletter "Generate With AI" Is Misleading
- **Status:** OPEN
- **Severity:** P1
- **Area:** UX
- **Problem:** Button copy says AI, but the implementation builds a local deterministic draft unless a separate AI endpoint is intentionally wired.
- **Why it matters:** AI labeling is a trust issue.
- **User impact:** Users may believe a smarter AI-backed summary was produced when it was not.
- **Likely root cause:** UI label outran implementation.
- **Likely files:** `NewsletterView.vue`, `auditClient.ts`.
- **Recommended fix:** Rename to "Auto-Draft Summary" or wire the real AI generation endpoint and expose failure/loading states.
- **Regression test needed:** Button click either calls the AI endpoint or displays non-AI wording.
- **Evidence collected:** Logic agent review; `120-newsletter-base.png`.

## Verified Visual / UX Improvements

### U-001: Dashboard Action Cluster And Hide Controls Need Rework
- **Status:** OPEN
- **Severity:** P2
- **Area:** UI
- **Problem:** Refresh, filter/settings, export, and reports actions are tightly grouped with similar weight; card hide controls are noisy.
- **Why it matters:** Dashboard actions should be fast to scan in demos and daily use.
- **User impact:** Users may not know which action is primary or what hide/customize does.
- **Likely root cause:** Many controls were added without a clear action hierarchy.
- **Likely files:** `ReportsView.vue`.
- **Recommended fix:** Group actions by purpose, label primary actions, clarify customize/hide/restore.
- **Regression test needed:** Desktop/tablet/mobile screenshots with action group and restore path.
- **Evidence collected:** `01-reports-dashboard-base.png`, `02-reports-customize-panel.png`, `130-mobile-reports.png`.

### U-002: Audit List Is Not Mobile-Friendly And Filters Truncate
- **Status:** OPEN
- **Severity:** P2
- **Area:** UI
- **Problem:** Mobile uses a squeezed desktop table; filter labels truncate.
- **Why it matters:** Field auditors are likely to use smaller screens.
- **User impact:** Tracking/status/action discovery is poor on mobile.
- **Likely root cause:** No responsive audit-list layout.
- **Likely files:** `AuditDashboardView.vue`, shared table/filter components.
- **Recommended fix:** Add mobile card/list mode or deliberate horizontal table affordance with frozen key data.
- **Regression test needed:** Mobile screenshot must show tracking, status, division, date, and action affordance.
- **Evidence collected:** `10-audits-list-base.png`, `130-mobile-audits.png`.

### U-003: New Audit Page Has Weak Progression
- **Status:** OPEN
- **Severity:** P2
- **Area:** UX
- **Problem:** Selected-division state leaves dead space and CTA is disconnected from inputs.
- **Why it matters:** Audit creation should feel deliberate and guided.
- **User impact:** Users may not know what is required before creating the audit.
- **Likely root cause:** Single-column form layout underuses page context.
- **Likely files:** `NewAuditView.vue`.
- **Recommended fix:** Convert to compact step flow with required fields, active template/version summary, and nearby primary CTA.
- **Regression test needed:** Visual check of base/selected states.
- **Evidence collected:** `20-new-audit-base.png`, `21-new-audit-division-selected.png`.

### U-004: Long Audit Form Needs Section Navigation
- **Status:** OPEN
- **Severity:** P2
- **Area:** UX
- **Problem:** A 60+ question audit form lacks visible section jump/progress navigation.
- **Why it matters:** Long field audits need fast orientation.
- **User impact:** Auditors scroll linearly and can miss unanswered or NC items.
- **Likely root cause:** Form optimized for section rendering, not field navigation.
- **Likely files:** `AuditFormView.vue`, `AuditSection.vue`, `AuditQuestionRow.vue`.
- **Recommended fix:** Add sticky section index/progress, unanswered/NC filters, and jump-to-next-blocker behavior.
- **Regression test needed:** Long audit form screenshot and E2E next-unanswered action.
- **Evidence collected:** `30-audit-form-base.png`, `32-audit-form-expand-all.png`.

### U-005: Corrective Actions Table Is Dense
- **Status:** OPEN
- **Severity:** P2
- **Area:** UI
- **Problem:** Overdue styling is so common that every row feels urgent; actions are subtle.
- **Why it matters:** CA triage needs scan speed.
- **User impact:** Users struggle to tell what needs attention first.
- **Likely root cause:** Table density plus uniform danger styling.
- **Likely files:** `CorrectiveActionsView.vue`.
- **Recommended fix:** Use severity/age grouping, persistent key actions, and better overdue hierarchy.
- **Regression test needed:** Visual check with many overdue rows and action discovery.
- **Evidence collected:** `50-corrective-actions-base.png`, `51-corrective-actions-bulk-toolbar.png`.

### U-006: Template Manager Undersells Version Workflow
- **Status:** OPEN
- **Severity:** P2
- **Area:** Template Admin
- **Problem:** Active view reads like a passive list; draft/publish next steps are not obvious.
- **Why it matters:** Template governance is a core admin workflow.
- **User impact:** Admins may fear editing or misunderstand publish impact.
- **Likely root cause:** Version controls are visually separated from the workflow explanation.
- **Likely files:** `TemplateManagerView.vue`.
- **Recommended fix:** Add active/draft state banner, publish impact copy, and in-context create/edit/publish controls.
- **Regression test needed:** Active and draft screenshots with clear next step.
- **Evidence collected:** `60-template-manager-base.png`, `61-template-manager-division-selected.png`.

### U-007: Report Composer Empty State Is Too Blank
- **Status:** OPEN
- **Severity:** P2
- **Area:** UX
- **Problem:** Empty canvas and dead property panel do not guide first use.
- **Why it matters:** Composer is powerful but feels unfinished on first load.
- **User impact:** Users may not know whether to generate, add blocks, load drafts, or choose a mode.
- **Likely root cause:** Property panel only supports selected-block state.
- **Likely files:** `ReportComposerView.vue`, composer canvas/property components.
- **Recommended fix:** Add start options on the canvas and contextual help in the property panel until a block is selected.
- **Regression test needed:** Empty composer screenshot and block-selected screenshot.
- **Evidence collected:** `90-report-composer-base-empty.png`, `93-report-composer-newsletter-settings-panel.png`.

### U-008: Newsletter And Quarterly No-Data States Look Like Finished Reports
- **Status:** OPEN
- **Severity:** P2
- **Area:** Reporting
- **Problem:** Empty periods render as zero-filled reports without clear no-data explanation.
- **Why it matters:** Reporting trust depends on knowing whether zero means good performance or no data.
- **User impact:** Managers can misread empty quarters/newsletters.
- **Likely root cause:** No dedicated no-data report state.
- **Likely files:** `NewsletterView.vue`, `QuarterlySummaryView.vue`.
- **Recommended fix:** Show selected period, data checked, and "no audits found" messaging before narrative/export.
- **Regression test needed:** No-data fixture screenshots for newsletter and quarterly summary.
- **Evidence collected:** `120-newsletter-base.png`, `121-quarterly-summary-base.png`.

## Items Still Needing Verification

- `B-002 loadAudit()` duplicate fetch: logic agent says likely fixed; add network spy to prove one audit GET.
- `B-003 composer active draft double-delete`: logic agent says fixed, but metadata-only edits may not autosave.
- `B-006 CA bulk close photo policy`: logic agent says likely fixed in UI/backend; needs live role/data repro.
- `B-007 Delete Selected (0)`: logic agent says likely fixed; verify with draft/non-draft selections.
- `B-008 print blank form loading/error`: likely fixed; keep screenshot regression.
- `B-013 blank form print`: fallback exists, but DOM relocation/print timing still needs reload/multi-tab testing.
- External benchmark opportunities such as offline mode, photo annotation, CA escalation, root cause/effectiveness, audit scheduling, and section score breakdown are recommendations until Joseph approves product direction.

## Recommended Fix Order

1. P1-016 API/dev-start port alignment and live QA preflight.
2. P1-017 Admin Audit Log tabs/row expansion runtime bug.
3. P0-002 Section N/A backend parity.
4. P0-003 Decide remove vs repair skip logic, then finish that path.
5. P1-018 Auditor new-audit API permission contract.
6. P1-020 Route guard meta completion.
7. P1-019 Reporting count semantics.
8. P1-021 Newsletter AI label or real AI wiring.
9. U-002 Mobile audit list usability.
10. U-004 Long audit form navigation.
11. U-007 Composer empty state.
12. U-008 Newsletter/quarterly no-data state.
13. U-001 Dashboard action hierarchy.
14. U-005 Corrective action scanability.
15. U-006 Template manager workflow clarity.
16. U-003 New Audit layout/progression.

## Updated QA Artifacts

- `LIVE_QA_TODO.md`: added live screen sweep results, new P0/P1/P2 findings, and reopened route guard issue.
- `QA_REGRESSION_CHECKLIST.md`: added section 16 with regression targets for every confirmed live sweep finding.
- `docs/qa/qa-operating-model.md`: added API-health-first screenshot rule and evidence metadata requirements.
- `docs/qa/button-coverage-matrix.md`: already updated to reflect dropdown-based New Audit flow, not removed division-card UI.

## Top Items Joseph Should Approve First

Approve these first because they protect trust and keep future QA from lying:

1. Align dev-start/API port and add live API preflight.
2. Fix Admin Audit Log tabs/expansion.
3. Finish Section N/A backend/report/review parity.
4. Decide whether skip logic is removed or repaired.
5. Fix Auditor New Audit API contract.
6. Finish route guard meta conversion.
