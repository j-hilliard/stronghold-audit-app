## Auto-Trigger: Agents Run Automatically on Code Changes

The `.claude/settings.json` PostToolUse hook fires every time a `.cs`, `.vue`, `.ts`, or `.tsx` file is edited. When it fires, Claude must run the full quality loop below — without waiting to be asked.

**You do not need to instruct Claude to run these agents. They trigger automatically when code changes.**

---

## Continuous Improvement Loop

This loop is mandatory after every feature, bug fix, or UI change. It runs automatically via hook — Claude must not skip or defer it.

### Step 1 - Run All QA Agents in Parallel
Run these four agents simultaneously:
- `tester` — full E2E behavior/logic/data-flow/button coverage; verifies DB writes
- `ui-agent` — before/after screenshot audit across all pages, dark+light, 3 viewports
- `db-agent` — schema quality, migration safety, FK indexes, soft-delete filters
- `ef-agent` — EF Code First compliance: no raw DDL, no EnsureCreated, migrations generated not hand-written, MigrateAsync only in Local

### Step 2 - Run Improver
`improver` consumes all four reports and:
- fixes every confirmed defect (logic, UI, schema, EF violations)
- revalidates all impacted tests after fixes
- researches enterprise competitors (SafetyCulture, Cority, Intelex, VelocityEHS) and reports gaps
- audits Azure + EF readiness
- prepares enhancement options for user decision — never self-approves product direction changes

### Step 3 - Re-Run Core QA in Parallel
Re-run `tester`, `ui-agent`, `db-agent`, and `ef-agent` to verify all fixes worked.

### Step 4 - Repeat Until Clean
Do not report complete until:
- all critical defects are resolved or explicitly accepted by user
- E2E/visual/schema/EF checks pass for the changed scope
- every unresolved item is documented with severity and owner
- user has seen and acknowledged any enhancement options requiring their decision

---

## EF Core Code First — Non-Negotiable Rules

1. **Models are the source of truth.** Database schema is always derived from `Data/Models/` classes via EF migrations. Never the reverse.
2. **Every schema change needs a migration.** No direct DB edits. No `EnsureCreated`. No raw DDL.
3. **Migrations are generated, not hand-written.** Use `dotnet ef migrations add <Name>`, then review the output.
4. **`Database.Migrate()` only in Local startup.** CI/CD pipeline applies migrations in Development and Production.
5. **`DbInitializer` seeds data only.** It is not a schema tool and must never call `Migrate()` or DDL.
6. **To add a migration:** use the `/migrate <MigrationName>` skill.

---

## General Non-Negotiable Rules

1. No assumed passes — only executed and evidenced checks count.
2. No deployment sign-off if any migration/data-loss risk is unresolved.
3. No enhancement implementation without user decision when product direction is affected.
4. UI fixes require before/after Playwright screenshots.
5. Data-write flows require DB verification — not UI-only confirmation.
6. Never commit without explicit user instruction.

## Hard QA Contract (Before and After Every Code Change)

This is mandatory. No exceptions.

1. Pre-Change Baseline (before first code edit in a task):
   - API health must be 200 for:
     - `/v1/divisions`
     - `/v1/audits`
     - `/v1/admin/templates`
     - `/v1/reports/compliance-status`
   - Capture visual baseline on audit routes (desktop/tablet/mobile, dark/light) and store artifact paths in the report.

2. Post-Change Verification (after edits):
   - Re-run API health checks above.
   - Run:
     - `npm --prefix webapp run qa:baseline`
     - `npm --prefix webapp run test:e2e:audit:live-guard`
     - `npm --prefix webapp run test:e2e:audit:visual:all`
   - Capture post-change visuals and list diff/failure artifacts.

3. Required Report Output (every change):
   - Exact commands executed.
   - Pass/fail counts.
   - Before/after screenshot artifact paths.
   - All failures with root cause and fix.

4. Fail Conditions:
   - If API health fails, stop and fix runtime first.
   - If visuals are missing, task is not complete.
   - If any claim is not backed by executed evidence, task is not complete.
