# QA_AUDIT_20260428_CLAUDE_FIX_AUDIT

Date: 2026-04-28

Scope: audit Claude's claimed fixes for `P1-015`, `P1-016`, `P1-017`, `P1-018`, and `P1-019`.

Policy: no application code changed by this audit. QA evidence only.

Evidence root: `docs/qa-evidence/QA_AUDIT_20260428_CLAUDE_FIX_AUDIT/`

## Summary

| ID | Result | Notes |
|---|---|---|
| P1-015 | ACCEPTED AS-IS | Already closed in `LIVE_QA_TODO.md`; no code change needed. |
| P1-016 | DONE | `launchSettings.json` now uses `http://localhost:7221`; API health returned 200 on 7221. |
| P1-017 | PARTIAL / STILL OPEN | `expandedRows` crash is fixed, but tab rendering is still broken: Action Log and Change Trail tables both render together. |
| P1-018 | DONE | New Audit no longer calls `/v1/admin/*`; it uses `/v1/templates/active?divisionId=8`. |
| P1-019 | DONE | Auditor stats now count every row via `entry.count++` and display `auditCount: s.count`. |

## P1-016: API Port Mismatch

- **Status:** DONE
- **What Claude changed:** `Api/Properties/launchSettings.json` profile application URL changed from `http://localhost:5221` to `http://localhost:7221`.
- **Verification performed:** `GET http://localhost:7221/v1/divisions` returned 200.
- **Build check:** `dotnet build --no-restore` passed.
- **Remaining note:** Keep the QA preflight rule: no live screenshots until `/v1/divisions` returns 200 from the frontend-configured API base.

## P1-017: Admin Audit Log Tabs / Expansion

- **Status:** PARTIAL / STILL OPEN
- **What Claude fixed:** The row expansion crash is gone. The live probe recorded no console errors after clicking expansion.
- **What is still broken:** The page still renders both the Action Log table and the Change Trail table at the same time. This means the tab/panel behavior is still not correct.
- **Evidence:**
  - `docs/qa-evidence/QA_AUDIT_20260428_CLAUDE_FIX_AUDIT/admin-audit-log-regression-result.json`
  - `docs/qa-evidence/QA_AUDIT_20260428_CLAUDE_FIX_AUDIT/admin-audit-log-actions-after-fix.png`
  - `docs/qa-evidence/QA_AUDIT_20260428_CLAUDE_FIX_AUDIT/admin-audit-log-expanded-after-fix.png`
- **Likely cause:** `AdminAuditLogView.vue` still uses `<Tabs>`, `<TabList>`, `<TabPanels>`, and `<TabPanel>` while the app has `primevue@3.26.1`. The DataTable `expandedRows` contract was fixed, but the tab container is still not controlling panel visibility.
- **Tell Claude exactly this:** Replace the tab implementation with PrimeVue 3-compatible `TabView`/`TabPanel`, or remove dependency on PrimeVue tabs and render the two DataTables with explicit `v-if="activeTab === 'actions'"` and `v-if="activeTab === 'trail'"`. Re-run the same live probe and confirm only one table renders at a time.
- **Regression needed:** Open `/audit-management/admin/audit-log`, verify Action Log shows only action columns, switch to Change Trail, verify only change columns, expand a row, and assert no console errors.

## P1-018: New Audit Non-Admin API Contract

- **Status:** DONE
- **What Claude changed:** Removed `adminStore` from `NewAuditView.vue`; selected division now loads active template via `client.getActiveTemplate(divId)` and job prefixes via `client.getDivisionJobPrefixes(divId)`.
- **Verification performed:** Live browser/network probe selected division `CSL (Job Site)`.
- **Observed API calls:**
  - `http://localhost:7221/v1/divisions`
  - `http://localhost:7221/v1/divisions/8/job-prefixes`
  - `http://localhost:7221/v1/templates/active?divisionId=8`
- **Admin calls:** none.
- **Evidence:** `docs/qa-evidence/QA_AUDIT_20260428_CLAUDE_FIX_AUDIT/new-audit-network-result.json`
- **Small follow-up:** `getActiveTemplate(divId).catch(() => null)` will make 403/500/network failures look like "No active template found." That is acceptable for closing P1-018, but should be improved later with visible error handling.

## P1-019: Auditor Audit Count

- **Status:** DONE
- **What Claude changed:** `ReportsView.vue` auditor stats map now tracks `count`; every row increments `entry.count++`; UI outputs `auditCount: s.count`.
- **Verification performed:** Static/diff audit and `npm.cmd --prefix webapp run build:dev`.
- **Result:** The UI no longer uses `scores.length` for auditor audit count.
- **Regression needed:** Add a fixture with one scored audit and one unscored/all-N/A audit for the same auditor; verify `Audits` count includes both and `Avg Score` only averages scored rows.

## Commands / Checks

- `npm.cmd --prefix webapp run build:dev` — passed.
- `dotnet build --no-restore` — passed with existing NuGet vulnerability warnings.
- Live API health: `http://localhost:7221/v1/divisions` — 200.
- Live New Audit probe — passed, no `/v1/admin/*`.
- Live Admin Audit Log probe — partial, no console error but mixed tab content remains.
