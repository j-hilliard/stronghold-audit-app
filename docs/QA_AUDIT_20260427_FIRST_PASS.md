# QA_AUDIT_20260427_FIRST_PASS

Date: 2026-04-27

Scope: Stronghold Audit App audit-management module, template engine, reporting, newsletter, report composer, corrective actions, print/PDF-adjacent flows, route guards, and QA gates.

Code-change policy: no application code was changed in this sweep. Evidence artifacts only were created under `docs/qa-evidence/QA_AUDIT_20260427_FIRST_PASS/`.

## Executive Summary

This first-pass audit found four demo/trust/data-integrity issues that should be reviewed before authorizing fixes: incomplete scope enforcement, mutable template versions, incomplete response snapshots, and skip-logic key mismatch. These directly affect access control, historical defensibility, template governance, and audit form behavior.

Several old TODO defects appear stale or ready for Joseph review rather than still being simple missing-UI defects. The composer Tiptap issue is ready for closure review because the composer gate passed 5/5. New Audit and navigation mocked core gates also passed 4/4. Template Manager and Reports exist now, but their contract gates fail against stale labels/selectors.

UI Agent status: BLOCKED/TIMED OUT. Improver Agent and Tester Agent completed read-only reports. Mocked screenshots were captured for key surfaces.

## Evidence Packet

Root: `docs/qa-evidence/QA_AUDIT_20260427_FIRST_PASS/`

- `screenshots/reports-dashboard-mocked.png`
- `screenshots/newsletter-mocked.png`
- `screenshots/report-composer-mocked.png`
- `screenshots/template-manager-draft-mocked.png`
- `capture-screenshots.spec.ts`
- Playwright error contexts:
  - `webapp/test-results/artifacts/audit-template-admin-contr-201c5-op-reorder-and-publish-flow-chromium/error-context.md`
  - `webapp/test-results/artifacts/audit-kpi-reporting-contra-fd513--supports-section-drilldown-chromium/error-context.md`
  - `webapp/test-results/artifacts/audit-kpi-reporting-contra-ed1b8-ble-and-keep-uniform-widths-chromium/error-context.md`
  - `webapp/test-results/artifacts/audit-corrective-actions-c-b43b7-isabled-until-notes-entered-chromium/error-context.md`
  - `webapp/test-results/artifacts/audit-corrective-actions-c-5725a-og-submits-reassign-request-chromium/error-context.md`

## Commands Run

- PASS: `dotnet build --no-restore`
  - 0 errors, 5 NuGet vulnerability warnings.
- PASS: `npm.cmd --prefix webapp run build:dev`
  - Required escalation because sandbox blocked Vite/esbuild child process.
- PASS: `npm.cmd --prefix webapp run test:e2e:audit:core -- --workers=1`
  - 4/4 passed.
- FAIL: `$env:PW_AUDIT_TEMPLATE_GATE='true'; npm.cmd --prefix webapp run test:e2e:audit:template-gate -- --workers=1`
  - 0/1 passed. Timeout waiting for `Create New Version`.
- PASS: `$env:PW_AUDIT_COMPOSER_GATE='true'; npm.cmd --prefix webapp run test:e2e:audit:composer-gate -- --workers=1`
  - 5/5 passed.
- FAIL: `$env:PW_AUDIT_REPORTING_GATE='true'; npm.cmd --prefix webapp run test:e2e:audit:reporting-gate -- --workers=1`
  - 6/8 passed. KPI reporting tests failed waiting for heading `Reports`.
- FAIL: `npm.cmd --prefix webapp run test:e2e:audit:corrective-actions -- --workers=1`
  - 29/34 passed. Failures are close-dialog selector drift and bulk reassign payload.
- PASS: temporary mocked screenshot capture spec
  - 1/1 passed. Temporary copy under `webapp/tests/e2e` was removed after capture.

## Live TODO Status

| Item | Status | Notes |
|---|---|---|
| DEF-0001 New Audit blank division cards/all selected | ACCEPTED AS-IS CANDIDATE | Current New Audit uses a dropdown, not cards. Mocked core gate passed 4/4. |
| DEF-0002 Sidebar navigation blank pages | DONE CANDIDATE | Mocked navigation stability passed. Needs live nav stress before closure. |
| DEF-0003 Template Manager missing controls | NEEDS JOSEPH REVIEW | Template Manager exists with Create Draft/Publish/add/reorder controls; contract expects old label and no selected division. |
| DEF-0004 Reports/KPI missing controls | NEEDS JOSEPH REVIEW | Reports page has KPI cards, filters, export, section cards; contract expects heading `Reports` while UI says `Audit Dashboard`. |
| DEF-0005 Template selector undefined | READY FOR REVIEW | Current selector renders version number/status; no current code evidence of undefined label. |
| DEF-0007 API ECONNREFUSED 7221 | BLOCKED | Live API guard not rerun against a confirmed running API. |
| DEF-0009 Newsletter template 404 | NEEDS JOSEPH REVIEW | API still returns 404 for missing default template; UI catches and defaults. Decide whether normal-load 404 is acceptable. |
| DEF-0010 Composer Tiptap import error | READY FOR REVIEW | Current named import and composer gate passed 5/5. |

## Verified Findings

### P0-SHAA-001: Audit Scope Enforcement Is Incomplete

- **Status:** OPEN
- **Area:** Security | Permissions | Data Integrity
- **Problem:** Several audit detail and mutation handlers load records directly by ID without enforcing the scoped audit user context. List/report handlers only filter when `AllowedDivisionIds` has entries, so a non-global user with an empty scoped list can fall through to all rows.
- **Why it matters:** Requirements R-007 and R-009 require role plus scope enforcement. Direct URL/API access can expose or mutate out-of-scope audits.
- **Impact:** Trust, security, data exposure, unauthorized edit/delete risk.
- **Evidence:** `docs/requirements/audit-template-engine-requirements.md:54`; `Api/Domain/Audit/Audits/GetAudit.cs:38`; `SaveAuditResponses.cs:41`; `SubmitAudit.cs:60`; `GetAuditReview.cs:35`; `DeleteAudit.cs:36`; `CreateAudit.cs:45`; `GetAuditList.cs:47`; `GetAuditReport.cs:51`; `Api/Authorization/AuditUserContext.cs:33-36`.
- **Likely files:** `Api/Domain/Audit/Audits/*.cs`, `Api/Domain/Audit/Export/*.cs`, `Api/Authorization/AuditUserContext.cs`, `Api/Authorization/AuthorizationBehavior.cs`.
- **How to fix:** Add a shared audit access guard that applies division/site/company/audit-type scope to every direct-ID read/mutation/export. Treat non-global users with empty allowed scope as no access, not all access. Use the same guard for division lists and template/admin paths.
- **Regression test:** Add negative API tests for scoped users: cannot list, get, save, submit, review, close, reopen, delete, export, upload, or fetch divisions/templates outside assigned scope.
- **Owner lane:** Shared
- **Verification needed:** API returns 403/404 for out-of-scope operations and UI hides out-of-scope divisions/routes.

### P0-SHAA-002: Draft Template Edits Can Mutate Active Templates

- **Status:** OPEN
- **Area:** Template Admin | Data Integrity | Logic
- **Problem:** Cloning a template version reuses `QuestionId`, and editing a draft mutates the shared `AuditQuestion` row. Active template reads question text/flags from that shared row.
- **Why it matters:** R-001 and R-004 say published versions must not be directly edited and new audits must bind to the active version. Draft edits can change live audit wording before publish.
- **Impact:** Template governance, audit defensibility, demo trust.
- **Evidence:** `docs/requirements/audit-template-engine-requirements.md:15`; `CloneTemplateVersion.cs:88`; `UpdateQuestion.cs:57-60`; `GetActiveTemplate.cs:66`.
- **Likely files:** `Api/Domain/Audit/Admin/CloneTemplateVersion.cs`, `Api/Domain/Audit/Admin/UpdateQuestion.cs`, `Api/Domain/Audit/Templates/GetActiveTemplate.cs`, audit template/version models and migrations.
- **How to fix:** Store version-local question snapshots/config on `AuditVersionQuestion`, or clone master questions when a draft is edited. Active/published versions must read immutable version-local values.
- **Regression test:** Clone active version, edit draft wording/flags, load active template and start new audit before publish; active wording must remain unchanged until publish.
- **Owner lane:** Improver Agent | Tester Agent
- **Verification needed:** API and DB show draft and active version wording can diverge safely.

### P0-SHAA-003: Response Snapshot Integrity Is Incomplete

- **Status:** OPEN
- **Area:** Reporting | Data Integrity | Review Workflow
- **Problem:** `AuditResponse` has snapshot columns for section/category/order, but save writes only question text and weights. Review/report code later groups and sorts by snapshots that can be null.
- **Why it matters:** R-003 and R-006 require stable reporting taxonomy and completed audit snapshots. Missing snapshots can collapse review sections to `General`, break ordering, and destabilize historical reporting.
- **Impact:** Reporting trust, legal/QA defensibility, historical trend accuracy.
- **Evidence:** `docs/requirements/audit-template-engine-requirements.md:25`; `AuditResponse.cs:20,26,32,35`; `SaveAuditResponses.cs:143,159`; `GetAuditReview.cs:188-212`; `GetAuditReport.cs:133-134,191`.
- **Likely files:** `Api/Domain/Audit/Audits/SaveAuditResponses.cs`, `SubmitAudit.cs`, `GetAuditReview.cs`, `GetAuditReport.cs`, `Data/Models/Audit/AuditResponse.cs`.
- **How to fix:** Populate `SectionNameSnapshot`, `ReportingCategorySnapshot`, and `SortOrderSnapshot` from the bound template version at save/submit. Use `ReportingCategorySnapshot` for analytics rollups and section label only for display.
- **Regression test:** Save/submit an audit, rename/move sections in a new template, verify old review/report grouping and ordering remain unchanged.
- **Owner lane:** Shared
- **Verification needed:** DB rows for answered questions have non-null section/category/order snapshots.

### P0-SHAA-004: Skip Logic Uses Version Question IDs Against A Question-ID Map

- **Status:** OPEN
- **Area:** Logic | Template Admin | Audit Form
- **Problem:** Logic rules store `triggerVersionQuestionId`, but audit form response state is keyed by `questionId`; rule lookup uses the version-question ID against that map.
- **Why it matters:** Conditional hide/show sections can silently fail or target the wrong response after clone/version divergence.
- **Impact:** Broken audit workflow, invalid visible sections, template rule trust.
- **Evidence:** `auditStore.ts:96`; `auditStore.ts:146-163`; `auditStore.ts:235`; template manager saves `triggerVersionQuestionId` at `TemplateManagerView.vue:971-976`.
- **Likely files:** `webapp/src/modules/audit-management/stores/auditStore.ts`, `TemplateManagerView.vue`, `Api/Domain/Audit/Admin/SaveLogicRule.cs`, template DTOs.
- **How to fix:** Key response state by `versionQuestionId`, or maintain a reliable version-question-to-question mapping for logic evaluation. Validate trigger question and target section belong to the same template version.
- **Regression test:** Create hide/show rule in draft, publish, start audit, answer trigger, verify target section visibility before and after clone/publish.
- **Owner lane:** Improver Agent | Tester Agent
- **Verification needed:** Rule behavior works for cloned/published templates and does not affect unrelated versions.

### P1-SHAA-005: Reporting Dates And Scores Are Not Single-Source

- **Status:** OPEN
- **Area:** Reporting | Data Integrity | UX
- **Problem:** Report date filtering uses OR logic between `SubmittedAt` and `CreatedAt`; exports use a different effective date expression. Form/email score uses simple conformance ratio while review/report use weighted two-level scoring.
- **Why it matters:** A quarter or custom date range can include/exclude different audits depending on screen/export. The same audit can show different scores in the form, email, review, and dashboard.
- **Impact:** Reporting trust, executive summary accuracy, demo risk.
- **Evidence:** `GetAuditReport.cs:63-67`; `ExportQuarterlySummary.cs:49-52`; `ReportsView.vue:1049-1056`; `QuarterlySummaryView.vue:254-259`; `auditStore.ts:55-67`; `SubmitAudit.cs:242-244,304-306`; `GetAuditReport.cs:182-209`; `GetAuditReview.cs:49`.
- **Likely files:** `Api/Domain/Audit/Audits/GetAuditReport.cs`, `Api/Domain/Audit/Export/ExportQuarterlySummary.cs`, `Api/Domain/Audit/Audits/SubmitAudit.cs`, `webapp/src/modules/audit-management/stores/auditStore.ts`, reporting views.
- **How to fix:** Define one effective date (`SubmittedAt ?? CreatedAt`) and inclusive end-of-day boundaries. Centralize scoring in one backend service/API contract and have form/email/review/report consume the same result.
- **Regression test:** Boundary-date audits at quarter start/end, created-before/submitted-inside, created-inside/submitted-after, plus uneven weights; verify dashboard/export/email/review match.
- **Owner lane:** Shared
- **Verification needed:** Dashboard, quarterly page, CSV/Excel, review, and submission email show identical audit set and score.

### P1-SHAA-006: Newsletter Behavior And Copy Are Misleading

- **Status:** OPEN
- **Area:** UX | Reporting | API
- **Problem:** Newsletter UI says `Generate with AI (Draft)` but calls a local deterministic builder. Newsletter send is dry-run only. Missing newsletter template returns 404 in normal composer/editor flows unless caught by the UI.
- **Why it matters:** Users may believe AI or email sending happened when it did not. Normal-load 404s can fail strict live gates and erode trust.
- **Impact:** User trust, reporting workflow reliability, demo risk.
- **Evidence:** `NewsletterView.vue:46`; `NewsletterView.vue:459-472`; `SendNewsletter.cs:45-56`; `EmailService.cs:52-57`; `AuditController.cs:1118-1128`; `ReportComposerView.vue:366-391`; `NewsletterTemplateEditorView.vue:303-318,353-368`.
- **Likely files:** `NewsletterView.vue`, `NewsletterTemplateEditorView.vue`, `ReportComposerView.vue`, `Api/Domain/Audit/Newsletter/SendNewsletter.cs`, `Api/Controllers/AuditController.cs`, newsletter template handlers.
- **How to fix:** Rename local generation or wire it to a real AI summary endpoint. Return a default newsletter template as 200 for normal missing-template state. Wire send through `IEmailService` or label it visibly as dry-run/preview.
- **Regression test:** Save a template, reload in a fresh browser, open composer/newsletter, and verify persistence. Send action must visibly report real send/dry-run status and no normal-load 404.
- **Owner lane:** Shared
- **Verification needed:** No normal composer/editor 4xx; UI labels match actual behavior.

### P1-SHAA-007: Rich Text HTML Is Rendered Without Sanitization

- **Status:** OPEN
- **Area:** Security | Print/PDF | Reporting
- **Problem:** Rich text read-only output renders raw `modelValue` through `v-html`; backend draft validator only checks JSON validity/size.
- **Why it matters:** R-014 requires sanitized rich text storage and rendering. Unsafe HTML can persist into preview/print/PDF surfaces.
- **Impact:** XSS/trust risk, print/PDF integrity risk.
- **Evidence:** `docs/requirements/audit-template-engine-requirements.md:108-118`; `RichTextEditor.vue:87,126-127`; `Api/Domain/Audit/ReportDrafts/BlocksJsonValidator.cs:14-28`.
- **Likely files:** `webapp/src/modules/audit-management/features/reports/components/RichTextEditor.vue`, report block components, `Api/Domain/Audit/ReportDrafts/BlocksJsonValidator.cs`, create/update draft handlers.
- **How to fix:** Sanitize Tiptap HTML with an allowlist on write and render; reject scripts, event handlers, dangerous links/styles, and unsupported tags server-side.
- **Regression test:** Save malicious HTML in narrative/commentary, reload, preview, and print; stored JSON and rendered DOM must contain only allowed markup.
- **Owner lane:** Improver Agent | Tester Agent
- **Verification needed:** Payload and DOM inspection prove scripts/event attributes are stripped.

### P1-SHAA-008: Corrective Action Contract Has Close/Reassign Regressions Or Stale Selectors

- **Status:** OPEN
- **Area:** Corrective Actions | Testing | UX
- **Problem:** Mocked corrective-actions contract suite failed 5/34. Four close-dialog tests fail because the dialog now has two textareas and tests target `textarea` ambiguously. Bulk reassign submits `newAssignee: null` after typed input in the contract flow.
- **Why it matters:** The close dialog may be acceptable UX, but the contract is now too brittle to verify closure notes/photo rules. Bulk reassign payload mismatch is a real workflow risk if users can submit without a selected assignee.
- **Impact:** Corrective action workflow reliability, regression coverage, assignment trust.
- **Evidence:** Playwright output: 29 passed, 5 failed. `CorrectiveActionsView.vue:347-352`; `CorrectiveActionsView.vue:430-440`; `CorrectiveActionsView.vue:820-827`; error contexts under `webapp/test-results/artifacts/audit-corrective-actions-*`.
- **Likely files:** `webapp/src/modules/audit-management/features/corrective-actions/views/CorrectiveActionsView.vue`, `webapp/tests/e2e/audit-corrective-actions-contract.spec.ts`, bulk update handler/API DTO.
- **How to fix:** Give close-dialog fields stable labels/test IDs and update tests to fill resolution notes specifically. Decide whether bulk reassign must require selecting a known user; disable submit until valid, or preserve typed assignee string intentionally.
- **Regression test:** Corrective action close with optional root cause and required notes; photo-required close; bulk reassign by selected user and free text if supported.
- **Owner lane:** Tester Agent | Shared
- **Verification needed:** Corrective-actions contract suite passes 34/34 or documented accepted-as-is with updated tests.

### P1-SHAA-009: Dependency Vulnerability Warnings In Build

- **Status:** NEEDS JOSEPH REVIEW
- **Area:** Security | Maintainability
- **Problem:** Backend build passes but emits high/moderate NuGet vulnerability warnings.
- **Why it matters:** Security warnings in audit/admin/reporting dependencies should be triaged before release even when the build passes.
- **Impact:** Security posture, release readiness.
- **Evidence:** `dotnet build --no-restore` output: `AutoMapper 13.0.1` high severity, `Microsoft.Identity.Abstractions 7.1.0` moderate, `Microsoft.Identity.Web 3.3.0` moderate, `System.Security.Cryptography.Xml 9.0.0` high severity. Direct refs include `Api/Api.csproj:19` and `Api/Api.csproj:41`.
- **Likely files:** `Api/Api.csproj`, `Data/Data.csproj`, transitive package graph.
- **How to fix:** Run package audit/upgrade review, update vulnerable direct packages, identify transitive source for `System.Security.Cryptography.Xml`, rerun build and authentication flows.
- **Regression test:** Build with NU190x warnings resolved or explicitly suppressed with documented risk acceptance; run auth/login/report/export smoke.
- **Owner lane:** Shared
- **Verification needed:** Build output has no unreviewed high/moderate vulnerability warnings.

### P2-SHAA-010: QA Contracts And TODO Language Drifted From Current UI

- **Status:** OPEN
- **Area:** Testing | Maintainability | UX
- **Problem:** QA contracts and live TODO still describe older UI labels/structures: New Audit cards, Template Manager `Create New Version`, Reports heading `Reports`. Current UI uses dropdown, `Create Draft`, and `Audit Dashboard`.
- **Why it matters:** Stale gates fail for the wrong reason and make the live TODO scoreboard less useful.
- **Impact:** QA confidence, release readiness, wasted triage.
- **Evidence:** `LIVE_QA_TODO.md:13-21`; `QA_REGRESSION_CHECKLIST.md:28-91`; `audit-template-admin-contract.spec.ts:89`; `audit-kpi-reporting-contract.spec.ts:97,145`; error contexts show current UI loaded.
- **Likely files:** `LIVE_QA_TODO.md`, `QA_REGRESSION_CHECKLIST.md`, `docs/qa/button-coverage-matrix.md`, `audit-template-admin-contract.spec.ts`, `audit-kpi-reporting-contract.spec.ts`.
- **How to fix:** Update contracts to current labels/accessible names and keep user-facing headings intentional. Move stale TODOs to accepted-as-is or Joseph review sections after approval.
- **Regression test:** Re-run template/reporting gates with no stale-selector failures and no hidden skips for delivered audit/report/composer contracts.
- **Owner lane:** Tester Agent | Shared
- **Verification needed:** Template/reporting/composer gates pass or fail only on real product behavior.

## Accepted / Ready For Joseph Review

- DEF-0001: New Audit dropdown behavior is acceptable candidate; core gate passed.
- DEF-0002: Navigation stability done candidate; mocked navigation passed, live stress still needed.
- DEF-0003: Template Manager implementation exists; gate/test language needs review.
- DEF-0004: Reports dashboard implementation exists; gate/test language needs review.
- DEF-0005: Template version selector undefined issue ready for review.
- DEF-0010: Composer Tiptap issue ready for review; composer gate passed 5/5.
- Report draft optimistic concurrency appears acceptable as-is: `UpdateReportDraft.cs:41-42`, `useReportDraft.ts:207-208`.
- Close audit blocks open/non-terminal CAs; still needs shared scope guard.

## Blocked Items

- UI Agent did not return within this sweep window; lane marked BLOCKED/TIMED OUT.
- Live API availability on `localhost:7221` was not retested against a confirmed running API. DEF-0007 remains BLOCKED.
- Live visual/light/dark responsiveness was not completed; mocked screenshots only.

## Regression Checklist Summary

Add or update checklist coverage when fixes are authorized:

- Scoped user negative API suite across audit list/detail/save/submit/review/delete/export/divisions.
- Template immutability suite: draft edits do not affect active version before publish.
- Snapshot DB validation: section/reporting category/order snapshots persist on all answered responses.
- Logic rule E2E: hide/show section rules after clone/publish.
- Reporting consistency: date boundaries and weighted score equality across form/email/review/report/export.
- Newsletter persistence/send semantics: no normal-load 404; AI/dry-run labels match behavior.
- Rich text sanitizer suite for narrative/commentary preview and print.
- Corrective action close/reassign contract: unambiguous fields, photo gate, bulk reassign payload.
- QA contract cleanup: template/reporting gates updated to current UI labels.

## Recommended Fix Order

1. P0-SHAA-001 Scope enforcement.
2. P0-SHAA-002 Template immutability.
3. P0-SHAA-003 Snapshot integrity.
4. P0-SHAA-004 Skip logic key mismatch.
5. P1-SHAA-005 Reporting date/scoring consistency.
6. P1-SHAA-006 Newsletter behavior and template 404 semantics.
7. P1-SHAA-007 Rich text sanitization.
8. P1-SHAA-008 Corrective action close/reassign contract.
9. P2-SHAA-010 QA contract/TODO cleanup.
