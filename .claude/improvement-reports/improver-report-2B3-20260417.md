# Improver Report — Phase 2B-1 + 2B-3
**Date:** 2026-04-17

---

## Task 1 — Competitor Research (WebSearch Results)

### SafetyCulture iAuditor
**Sources:** [GetApp 2026](https://www.getapp.com/operations-management-software/a/iauditor/) | [Capterra Reviews 2026](https://www.capterra.com/p/141080/iAuditor/reviews/) | [Workyard Review](https://www.workyard.com/compare/iauditor-review) | [Taqtics Review 2025](https://taqtics.co/reviews/safetyculture/)

**What they do:** Assign CAs from inspection items to anyone including non-account-holders (guest email link). Centralized dashboard. Mobile-first with offline capture. Automated reminders: "due soon," 1h before due, 24h overdue alert. Zapier for external system hooks.

**What users hate (253 Capterra reviews, 4.6/5):**
- Cap of 3 mobile devices per user — must log out to switch
- Report export is restrictive — users rebuild scoring in Excel afterward (most consistent complaint)
- Navigation resets CA list to page 1 on back — UX annoyance cited by multiple users
- Platform has grown bloated (IoT, training, asset mgmt) — core audit teams find it too heavy

---

### Cority EHS
**Sources:** [Cority Audit & Inspections](https://www.cority.com/corityone/audit-inspections/) | [Cority Cortex AI](https://www.cority.com/cortex-ai/) | [Cority How to Choose EHS](https://www.cority.com/blog/how-to-choose-an-ehs-audits-and-inspections-solution/)

**What they do:** Full CA lifecycle (schedule → checklist → finding → CA → resolution → closure). Automated notification on assignment. Configurable escalation workflows (supervisor notified when overdue). AI (Cortex AI) pairs findings with recommended CAs based on past audit history and surfaces recurring hotspots. Configurable SLA rules by CA type/severity.

---

### VelocityEHS
**Sources:** [VelocityEHS Action Management](https://www.ehs.com/solutions/safety/action-management/) | [VelocityEHS Demo](https://www.ehs.com/videos/corrective-action-demo/)

**What they do:** Centralize CAs from all modules. External users can update tasks without a licensed seat. Automated email on assignment and overdue. Mobile access. CAPA with root cause analysis.

---

### Intelex
**Source:** [Intelex CA Demo](https://www.intelex.com/resources/product-demo/corrective-action-reporting-software/)

Full CAPA with root cause analysis. Regulatory linkage. SAP, Power BI, Procore integration. External collaboration links without full seat license.

---

## Feature Parity Matrix

| Feature | SafetyCulture | Cority | Intelex | VelocityEHS | Stronghold |
|---|:---:|:---:|:---:|:---:|:---:|
| CA tracking dashboard | ✓ | ✓ | ✓ | ✓ | **✓ (2B-1)** |
| Closure photo requirement | Partial | Configurable | ? | ? | **✓ (2B-3)** |
| Email on CA assignment | ✓ | ✓ | ✓ | ✓ | **✗** |
| Overdue escalation to supervisor | Partial | ✓ | ✓ | ✓ | **✗** |
| SLA rules by severity | ✗ | ✓ | ✓ | Partial | **✗** |
| Native mobile app | ✓ | ✓ | ✓ | ✓ | **✗** |
| Offline audit capture | ✓ | ✓ | Partial | Partial | **✗** |
| AI-suggested corrective actions | ✗ | ✓ | Partial | ✗ | **✗** |
| External assignee (no license) | ✓ | Partial | Partial | ✓ | **✗** |
| Root cause analysis | ✗ | Partial | ✓ | ✓ | **✗** |
| Power BI / SAP integration | Partial | ✓ | ✓ | Partial | **✗** |
| Scheduled report delivery | Partial | ✓ | ✓ | ✓ | **✓** |
| Pagination on CA list | ✗ (bug) | ✓ | ✓ | ✓ | **✗** |

---

## Code Issues Found

### GetCorrectiveActions.cs

**Finding 1 — Unbounded Query (No Pagination) — MEDIUM**
Line 113: `ToListAsync()` with no `Take()`/`Skip()`. All CAs load into memory on every page view. Will degrade at scale.

**Finding 2 — `SectionName` always empty string — LOW**
Line 137: `SectionName = ""` is never populated. Manual CAs have a Section via Finding navigation that could be included in the projection.

**Finding 3 — `AsNoTracking()` present — PASS** (Line 77, correctly applied)

**Finding 4 — N+1 photo count — PASS** (batch-load pattern at lines 144-158 is correct)

---

### BulkUpdateCorrectiveActions.cs

**Finding 5 — `CorrectiveActionOwner` excluded from Bulk — MEDIUM**
The role list (lines 10-12) excludes `CorrectiveActionOwner`. But that role can see all CAs via `GetCorrectiveActions` and can individually close via `CloseCorrectiveAction`. If the Vue UI shows the bulk action panel to this role, every bulk attempt will silently 403.

**Finding 6 — No upper bound on ID list — MEDIUM**
Line 55 only checks `Any()`. No max. Submitting 10,000 IDs generates a SQL `IN (...)` clause with 10,000 parameters — SQL Server struggles above ~2,100. Add: `if (request.CorrectiveActionIds.Count > 500) throw new ArgumentException(...)`.

**Finding 7 — Resolution notes appended to Description — MEDIUM (design)**
Line 126: `ca.Description += $"\n\n[Resolved] {request.ClosureNotes}"`. Resolution notes are mutated into the description string, making them unextractable for reporting. A dedicated `ResolutionNotes` column is needed.

**Finding 8 — No audit log on bulk status change — MEDIUM**
`IProcessLogService` is injected but never called. Bulk status changes leave no compliance trace.

---

### CloseCorrectiveAction.cs

**Finding 9 — No division-scope check — HIGH (Security / IDOR)**
CA loaded by `Id` only with no check against `IAuditUserContext.AllowedDivisionIds`. A `CorrectiveActionOwner` scoped to Division A can close any CA in any division by guessing an integer ID.
**Fix:** Inject `IAuditUserContext`. Check `ca.Audit?.DivisionId` against `AllowedDivisionIds` after load.

**Finding 10 — Voided CA can be closed — LOW**
Line 40 only blocks `"Closed"`, not `"Voided"`. Change to: `if (ca.Status is "Closed" or "Voided")`.

---

### UpdateCorrectiveAction.cs

**Finding 11 — No division-scope check — HIGH (Security / IDOR)**
Same IDOR pattern as Finding 9. No `IAuditUserContext` injected or used.

---

### UploadCaClosurePhoto.cs

**Finding 12 — No division-scope check on photo upload — HIGH (Security / IDOR)**
CA existence checked but division scope is not. Any authenticated user can upload a photo to any CA by ID.

**Finding 13 — Local filesystem storage — MEDIUM (Azure readiness)**
Photos written to local disk path. Incompatible with Azure App Service scale-out. Azure Blob Storage is required for production.

---

## Enhancement Options for User Decision

### Enhancement 1: CA Assignee Email on Assignment + Overdue Digest
**Problem:** Assignees are not notified. All major competitors send emails on assignment.
**Option A (Recommended):** Use existing `IEmailService` to email assignee on create/reassign. Add daily background digest for overdue CAs. — **2-3 days**
**Option B:** Azure Function / Hangfire scheduled scanner for overdue batch emails. More robust, higher setup cost. — **1 week**
**Source:** [SafetyCulture Notifications](https://help.safetyculture.com/en-US/000032/) | [VelocityEHS Action Management](https://www.ehs.com/solutions/safety/action-management/)

---

### Enhancement 2: SLA Rules + Supervisor Escalation Per Division
**Problem:** No enforcement tier. Life-Critical CAs can sit overdue for weeks. Cority and VelocityEHS both support configurable SLA windows with escalation.
**Option A (Recommended):** Per-division SLA tiers in Admin Settings (LifeCritical=24h, Standard=14d, Advisory=30d). Breach triggers manager email. — **3-4 days**
**Option B:** Simple "escalate after X days" toggle per division. No tiers. — **1-2 days**
**Source:** [Cority Audit Inspections](https://www.cority.com/corityone/audit-inspections/)

---

### Enhancement 3: Server-Side Pagination on CA List
**Problem:** All CAs load unbounded. Will degrade at 500+ items. This is a known pain point for iAuditor users too.
**Option A (Recommended):** `PageNumber` + `PageSize` on `GetCorrectiveActions`, return `{ items, totalCount }`, add paginator in Vue. — **1-2 days**
**Option B:** Infinite scroll frontend-only — does not fix the unbounded backend query. Not recommended alone.
**Source:** Code audit Finding 1 | [iAuditor Capterra Reviews](https://www.capterra.com/p/141080/iAuditor/reviews/)

---

### Enhancement 4: External Guest CA Assignee (Tokenized Link — No Login Required)
**Problem:** Subcontractors assigned CAs must have an account. SafetyCulture and VelocityEHS allow external users to act on CAs without a license seat.
**Option A (Recommended):** Signed token (SHA-256 hashed, 30-day expiry, revocable). Emailed to external assignee. Minimal public view showing just that CA. — **3-5 days**
**Option B:** Read-only public CA view only. Lower value. — **1 day**
**Source:** [VelocityEHS Action Management](https://www.ehs.com/solutions/safety/action-management/)

---

### Enhancement 5: AI-Suggested Corrective Action Descriptions
**Problem:** Cority's Cortex AI recommends CAs from audit history. Stronghold already uses Claude for summaries — same infrastructure applies.
**Option A (Recommended):** When CA is auto-generated, call Claude API with question text + section + past CAs for similar questions. Pre-populate editable suggested description. — **2-3 days**
**Option B:** Rule-based suggestion library: admin-authored remediation text per question. No AI cost, but requires manual maintenance. — **3 days setup + ongoing**
**Source:** [Cority Cortex AI](https://www.cority.com/cortex-ai/)

---

## Security Defects — Must Fix Before Next Release

| # | File | Issue | Severity | Fix |
|---|---|---|---|---|
| 9 | `CloseCorrectiveAction.cs` | IDOR — no division scope check | **HIGH** | Inject `IAuditUserContext`, filter by `AllowedDivisionIds` |
| 11 | `UpdateCorrectiveAction.cs` | IDOR — no division scope check | **HIGH** | Same fix |
| 12 | `UploadCaClosurePhoto.cs` | IDOR — no division scope on photo upload | **HIGH** | Load CA with Audit, check division |
| 5 | `BulkUpdateCorrectiveActions.cs` | `CorrectiveActionOwner` 403 asymmetry | MEDIUM | Add role or suppress UI |
| 10 | `CloseCorrectiveAction.cs` | Voided CA re-closeable | LOW | Add `or "Voided"` to guard |
| 6 | `BulkUpdateCorrectiveActions.cs` | No ID list cap (SQL param overflow risk) | MEDIUM | Add max 500 check |

---

## Azure / EF Readiness

- Local filesystem photo storage (`Attachments:BasePath`) is a **blocking issue** for Azure scale-out. Azure Blob Storage abstraction needed before production.
- If `ResolutionNotes` column is approved, a new EF migration is required.
- Bulk CA count cap (max 500) should be added before release.

---

## Sources

- [SafetyCulture GetApp 2026](https://www.getapp.com/operations-management-software/a/iauditor/)
- [SafetyCulture Capterra Reviews 2026](https://www.capterra.com/p/141080/iAuditor/reviews/)
- [Workyard iAuditor Review](https://www.workyard.com/compare/iauditor-review)
- [Taqtics SafetyCulture Review 2025](https://taqtics.co/reviews/safetyculture/)
- [Cority Audit & Inspections](https://www.cority.com/corityone/audit-inspections/)
- [Cority Cortex AI](https://www.cority.com/cortex-ai/)
- [Cority How to Choose EHS Audits Solution](https://www.cority.com/blog/how-to-choose-an-ehs-audits-and-inspections-solution/)
- [VelocityEHS Action Management](https://www.ehs.com/solutions/safety/action-management/)
- [VelocityEHS Corrective Action Demo](https://www.ehs.com/videos/corrective-action-demo/)
- [Intelex Corrective Action Demo](https://www.intelex.com/resources/product-demo/corrective-action-reporting-software/)
- [SafetyCulture Notifications Help](https://help.safetyculture.com/en-US/000032/)
