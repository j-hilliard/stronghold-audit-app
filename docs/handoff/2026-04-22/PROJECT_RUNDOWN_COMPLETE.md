# Stronghold Audit App - Full Project Rundown
**Snapshot Date:** April 22, 2026  
**Prepared For:** Cross-PC handoff + execution continuity  
**Scope:** Current state, architecture, implemented work, remaining work, workflow maps, QA/test status, blockers, and next actions

> Updated handoff packet files were added in this same folder for new-PC migration:
> - `README_PC_MOVE_PACKET.md`
> - `MASTER_GRANULAR_RUNDOWN.md`
> - `01_CURRENT_STATE_AND_INVENTORY.md`
> - `02_FEATURE_STATUS_AND_GAPS.md`
> - `03_END_TO_END_WORKFLOW_FLOWS.md`
> - `04_NEW_PC_BOOTSTRAP_AND_OPERATIONS.md`
> - `05_QA_STATUS_AND_EXECUTION_PLAN.md`
> - `06_PRIORITIZED_NEXT_WORK.md`
> - `07_REQUIREMENTS_COVERAGE_MATRIX.md`

---

## 1) Current Snapshot (Where We Are Right Now)

### 1.1 Overall status
- Project is **actively in-flight** with significant implementation complete.
- Core audit product is operational (audit creation, response saving, review, templates, reporting, corrective actions).
- Significant enhancement work has been added recently (distribution preview, root-cause support, scoring/admin improvements, report composer enhancements).
- Repo is **highly active / dirty** (many changed files and ongoing edits), so QA outcomes are sensitive to concurrent code changes.

### 1.2 What is currently running/validated today
- API recovered and reachable on `http://localhost:7221`.
- Verified endpoints returning `200` during this session:
  - `/v1/divisions`
  - `/v1/audits?take=1`
  - `/v1/admin/templates`
- EF code-first guard run result:
  - `PASS`
  - Report: `qa-runtime/reports/ef-guard-latest.md`

### 1.3 Important current caveat
- A Node/Vite process can accidentally occupy `:7221`, causing API startup conflicts and "Failed to load ..." behavior in UI.
- This happened today and was corrected by clearing the process and relaunching API on the intended port.

---

## 2) Repository Layout and Key Modules

### 2.1 Top-level directories
- `Api/` - ASP.NET Core API (CQRS + controllers + auth + services)
- `Data/` - EF Core DbContext, models, migrations, SQL seed scripts
- `webapp/` - Vue 3 + TypeScript frontend
- `Scripts/qa/` - QA gate runners, watcher scripts, EF guard
- `.agents/` - Codex-owned QA agent definitions
- `.claude/agents/` - Claude-side agent definitions (separate system)
- `qa-runtime/` - watcher runtime state + cycle reports
- `docs/` - requirements, guidance, and now this handoff package

### 2.2 Audit frontend module routes (implemented)
- `/audit-management/audits`
- `/audit-management/audits/new`
- `/audit-management/audits/:id`
- `/audit-management/audits/:id/review`
- `/audit-management/reports`
- `/audit-management/corrective-actions`
- `/audit-management/admin/templates`
- `/audit-management/admin/settings`
- `/audit-management/admin/users`
- `/audit-management/admin/audit-log`
- `/audit-management/newsletter/template-editor`
- `/audit-management/reports/composer`
- `/audit-management/reports/gallery`
- `/audit-management/reports/scheduled`
- `/audit-management/reports/by-employee`

### 2.3 API controller footprint
- `AuditController.cs` currently exposes **87 HTTP endpoints** across:
  - divisions
  - audits lifecycle
  - review/distribution
  - corrective actions
  - templates/admin
  - newsletter/report drafts
  - attachments/finding photos
  - exports/scheduled reports
  - score targets / SLA / benchmarks / audit logs

### 2.4 Data model status
- `Data/Models/Audit/` currently contains **33 audit-domain model files**, including:
  - `Audit`, `AuditHeader`, `AuditResponse`, `AuditQuestion`, `AuditSection`
  - `CorrectiveAction`, `CorrectiveActionPhoto`, `CaNotificationLog`
  - `AuditTemplate`, `AuditTemplateVersion`, `AuditVersionQuestion`
  - `ReportDraft`, `NewsletterTemplate`, `ScheduledReport`
  - `Division`, `DivisionJobPrefix`, `EmailRoutingRule`, `ReviewGroupMember`
  - `AuditActionLog`, `AuditTrailLog` (recently added)

### 2.5 Migration status (code-first)
- EF migrations present and tracked in repo.
- Non-designer migration files in `Data/Migrations/`: **31**
- Latest migration file present:
  - `20260423113525_add_audit_trail_and_action_logs.cs`

---

## 3) Feature Status by Workflow

## 3.1 Template workflow
**Status:** Mostly implemented  
**Built:**
- Versioned template model (`AuditTemplateVersion`)
- Draft/publish structure
- Section/question CRUD and reorder endpoints
- Logic rules endpoint set
- Template admin UI and drag/drop mechanics

**Open gaps / risk:**
- More UX polish needed on template manager usability and admin ergonomics.
- Additional coverage needed for every edge path when section/question rules change.

## 3.2 Audit execution workflow
**Status:** Implemented (core path works)  
**Built:**
- Create audit
- Save responses
- Submit/reopen/close endpoints
- Per-question behavior and scoring-related data
- Prior prefill endpoint

**Open gaps / risk:**
- Strong need for hardening around all branch behaviors (navigation stress, partial drafts, review handoff).
- Role-scoped behavior still needs full end-to-end contract verification in live mode.

## 3.3 Audit review + distribution workflow
**Status:** Implemented but unstable in tests right now  
**Built:**
- Review summary save
- Add/remove distribution recipients
- Distribution preview endpoint
- Send distribution endpoint

**Current issue cluster:**
- Distribution contract suite currently has many failing assertions (dialog render/interaction paths and summary/recipient flows).

## 3.4 Corrective actions workflow
**Status:** Implemented with major enhancements in progress  
**Built:**
- Create/update/close CA flows
- Bulk actions
- Priority/filtering support path
- Closure photo support
- Root-cause migration/model updates added in current change set

**Current issue cluster:**
- Several CA dialog behaviors failing in current Phase2C suite (close gating, photo requirement path, bulk reassign path).

## 3.5 Reporting + dashboard + composer workflow
**Status:** Functionally present, targeted UX simplification still needed  
**Built:**
- Reports dashboard
- KPI and section-level views
- Newsletter view/template editor
- Report composer with block model and print/export pipeline
- Draft persistence and manage-drafts flow

**Validated today:**
- Composer gate passed all tests.
- Reporting gate passed (with KPI contract tests currently skipped via feature-gate conditions).

**Open needs:**
- Dashboard simplification and information hierarchy still needs improvement (user feedback: too dense/overwhelming).
- KPI drilldown labeling and card consistency still need UX tuning and additional contracts.

---

## 4) Requirements Coverage (Business-Driven)

## 4.1 Confirmed requirement direction
From `docs/requirements/audit-template-engine-requirements.md` and user direction:
- Template engine (not static form)
- Versioning + reporting stability
- Role + scope security
- Drag/drop admin controls
- Report composer with sticky side rails
- Rich text authoring controls
- Layout customization engine
- QA gates with visual + behavior coverage

## 4.2 User-prioritized backlog themes still active
- Access controls by role and scope
- Weighted scoring (topic and group)
- Optional/toggled sections (job-specific)
- Attachments and evidence workflows
- Corrective action notifications
- Submission notifications
- Mobile usability
- Auto-generated reports
- Excel exports parity with legacy outputs
- Report composer custom layout freedom (banner/side regions/news sections)

## 4.3 Deferred / blocked by user decision
- **E-Charts integration:** explicitly deferred until access is available.

---

## 5) QA and Testing Status (Latest Session)

## 5.1 Core QA infrastructure
- QA scripts exist under `Scripts/qa/`.
- Watcher framework exists (`Start-CodexAgentWatch.ps1`, `Run-CodexAgentWatch.ps1`, status in `qa-runtime/agent-watch/`).
- Agent definitions exist in `.agents/`:
  - `tester.md`
  - `ui-agent.md`
  - `improver.md`
  - `db-agent.md`
  - `benchmark-agent.md`
  - `azure-ef-agent.md`
  - `ef-codefirst-guard-agent.md`

## 5.2 Watcher state at snapshot
- `Get-CodexAgentWatchStatus` currently reports stale/non-running state:
  - `Running: False`
  - `State: CYCLE_RUNNING` (stuck)
  - Heartbeat timestamp is outdated

**Meaning:** watcher process management must be reset/restarted cleanly on new machine.

## 5.3 Test runs in this session
- `test:e2e:audit:phase2c`
  - **61 passed / 29 failed / 1 skipped**
  - Primary failures concentrated in:
    - Audit review distribution interactions/dialog paths
    - Corrective action close/photo/bulk-reassign behaviors
    - SLA/priority API contract expectations
- `test:e2e:audit:composer-gate`
  - **5 passed / 0 failed**
- `test:e2e:audit:reporting-gate`
  - **6 passed / 0 failed / 2 skipped**
- `test:e2e:audit:live-guard`
  - Not executed successfully due local Playwright process spawn issue (`spawn EPERM`) during this snapshot.

## 5.4 Additional QA findings
- Vite warning observed repeatedly:
  - duplicate class member `createUser` in:
  - `webapp/src/apiclient/client.ts`
- During one failing Phase2C run, HMR updates occurred mid-run, which can invalidate deterministic assertions.

---

## 6) Current Known Defects / Instability Areas

## 6.1 Environment/process defects
1. Port collision risk on `7221` (API port) from stray Node/Vite process.
2. QA watcher can become stale/stuck (`CYCLE_RUNNING` while not alive).
3. Live guard suites can fail with Playwright `spawn EPERM` in current environment.

## 6.2 Application behavior defects (from latest contract suite)
1. Review distribution add/remove recipient flows failing.
2. Send distribution dialog open/render path failing in multiple tests.
3. Review summary save contract tests failing.
4. Corrective action close dialog requirement behaviors failing in several paths.
5. Bulk reassign contract failing.
6. SLA priority contract assertions failing in API-based checks.

## 6.3 Technical debt/high-risk warnings
1. Duplicate method warning in generated/maintained API client file (`createUser`).
2. Background services and safety-schema queries can log noise/errors when safety tables are absent in local DB shape.

---

## 7) Project Flow Maps (Operational)

## 7.1 Audit lifecycle flow
1. User opens `New Audit`
2. Select division/template context
3. Start audit -> create draft record
4. Fill responses (save/update response sets)
5. Submit audit
6. Review page validates findings/actions
7. Corrective actions assigned/updated/closed
8. Distribution recipients + summary managed
9. Distribution send action executed
10. Audit closed/reopened as needed

## 7.2 Template lifecycle flow
1. Admin opens template manager
2. Clone/create draft version
3. Add/edit/reorder sections/questions
4. Configure rules/weights/logic
5. Publish draft version
6. New audits bind to published version; old audits remain snapshot-bound

## 7.3 Corrective action lifecycle flow
1. Finding identified
2. CA created (manual or auto-create path)
3. Priority/SLA fields set
4. Assignment + due date managed
5. Optional closure photo evidence collected
6. CA status transitions to closed/voided/in-progress
7. Exports/reporting consume CA status and aging metrics

## 7.4 Reporting/composer flow
1. User opens reports dashboard for KPI/trend context
2. Optionally launches report composer
3. Builds report from blocks/templates
4. Saves/reloads report drafts
5. Prints/exports PDF (print-root path)
6. Newsletter/report send workflows consume configured routing

---

## 8) What Is Left (Execution-Ready Backlog)

## 8.1 Immediate (must-fix before demo confidence)
1. Fix Phase2C failures (review distribution + CA contract paths).
2. Stabilize deterministic test environment (disable in-run HMR contamination during critical gate runs).
3. Resolve duplicate `createUser` client member warning.
4. Normalize watcher startup/heartbeat/recovery behavior.

## 8.2 Next (high value)
1. Dashboard simplification and KPI drilldown UX alignment.
2. Full visual sweep with consistent page-level snapshots (light/dark where applicable).
3. End-to-end role/view/access contract pass for all audit roles and scoped visibility cases.

## 8.3 Mid-term strategic
1. Advanced report-composer layout customization (regions/page templates).
2. Mobile usability hardening.
3. Notification and escalation behavior verification.
4. Export parity checks vs legacy artifacts.

## 8.4 Deferred by decision
1. E-Charts integration (explicitly deferred until access is available).

---

## 9) Code-First / EF and Environment Strategy

## 9.1 Current posture
- EF Core migrations are in place and actively used (`Database.Migrate()` in Local).
- Guard script exists and currently passes.
- Migration history is extensive and includes recent audit-centric changes.

## 9.2 Recommended discipline for dev -> test -> prod
1. Keep **single source of truth** in `Data/Models` + migrations.
2. Never hand-edit DB in higher environments without corresponding migration.
3. Require migration checks in CI for every schema-affecting PR.
4. Use environment-specific seeding controls to avoid local-only data bleed.
5. Validate generated SQL and migration order before promotion.

## 9.3 Local caveat
- Local startup can produce safety-schema related errors when those tables are not present; these should not be ignored if endpoints rely on them.

---

## 10) New Machine Bring-Up Checklist

1. Clone repo and install dependencies:
   - `npm install` in `webapp`
   - `dotnet restore`
2. Ensure local SQL container/instance is reachable at local connection string values.
3. Start API in Local environment on `7221`.
4. Start webapp dev server (default `7220`).
5. Verify:
   - `/v1/divisions` returns 200
   - `/v1/audits?take=1` returns 200
   - `/v1/admin/templates` returns 200
6. Restart QA watcher cleanly:
   - stop stale watcher
   - start new watcher
   - verify heartbeat + status transitions
7. Run hard gates in order:
   - composer gate
   - reporting gate
   - phase2c
   - visual/live guards

---

## 11) Immediate Handoff Recommendation

Use this order when you get on the new PC:
1. Bring API + UI up and verify the three core health endpoints.
2. Run Phase2C first and fix red paths in review distribution/CA flows.
3. Run composer/reporting gates to ensure no regressions there.
4. Re-enable and verify watcher automation only after the above is stable.
5. Then start broader UI simplification and role-scope full-sweep testing.

---

## 12) Related Files to Read Next
- `docs/requirements/audit-template-engine-requirements.md`
- `AUDIT_GAP_ANALYSIS.md`
- `ACCURATE_GAP_ANALYSIS.md`
- `Scripts/qa/README.md`
- `qa-runtime/reports/ef-guard-latest.md`
- `Api/Controllers/AuditController.cs`
- `webapp/src/modules/audit-management/router/index.ts`
