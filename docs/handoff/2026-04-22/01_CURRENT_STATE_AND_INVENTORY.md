# 01 - Current State and Inventory
**Snapshot date:** April 22, 2026 (local repo state)

## Branch and Repo Condition
- Branch: `feat/phase-2c-enhancements`
- Working tree status:
  - Modified files: `148`
  - Added/untracked files: `51`
  - Deleted files: `13`
  - Total status lines: `212`
- Implication: treat this as an in-flight branch with active work, not a clean release branch.

## Top-Level Structure
- `Api/` - ASP.NET Core API (.NET 10, MediatR CQRS)
- `Data/` - EF Core models, context, migrations, seeders
- `webapp/` - Vue 3 + TypeScript + PrimeVue frontend
- `Scripts/qa/` - QA gates, watcher, EF guard scripts
- `.agents/` - Codex-owned agent profiles
- `.claude/` - Claude-side agent config and commands
- `docs/` - handbook, requirements, workflows, QA assets, handoffs

## Frontend Audit Module Footprint
- Audit module source files under `webapp/src/modules/audit-management`: `58` files
- Audit E2E spec files under `webapp/tests/e2e`: `32` specs
- Key audit routes registered:
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
  - `/audit-management/reports/composer`
  - `/audit-management/reports/gallery`
  - `/audit-management/reports/scheduled`
  - `/audit-management/reports/by-employee`
  - `/audit-management/newsletter/template-editor`

## API Footprint
- `Api/Controllers/AuditController.cs` contains `87` HTTP endpoints.
- Current endpoint surface includes:
  - audit lifecycle (create/save/submit/reopen/close/delete)
  - review/distribution (preview, recipient management, send)
  - corrective action CRUD + bulk operations + close-photo path
  - templates and admin controls
  - reporting, trends, newsletter, scheduled reports
  - exports (quarterly, NCR, corrective actions)

## Data Layer Footprint
- Audit domain models in `Data/Models/Audit`: `33` files
- Non-designer EF migrations in `Data/Migrations`: `31`
- Latest migration present:
  - `20260423113525_add_audit_trail_and_action_logs.cs`

## Platform Architecture Decisions (Observed)
- EF code-first pattern in use (`Data/Models` + migrations)
- Local startup uses `Database.Migrate()` then seeders in `Api/Program.cs`
- Development/Production assume pipeline-applied migrations, not runtime auto-migrate
- Audit trail interceptor (`Api/Infrastructure/AuditTrailInterceptor.cs`) is active
- CQRS role guards use `[AllowedAuthorizationRole(...)]` with `AuthorizationBehavior`

## Current Known Operational Risks
1. API port conflict risk on `7221` if another process binds the port.
2. Frontend can show mass 500s when API is down or pointed at wrong DB.
3. QA watcher status has shown stale state (`CYCLE_RUNNING` while not healthy).
4. Watcher logs show repeated failures from a frontend compile issue:
   - duplicate class member `createUser` in `webapp/src/apiclient/client.ts`

## Immediate Interpretation
- The project is feature-rich and structurally strong.
- Current project risk is execution consistency: environment health, test gating discipline, and regression control across rapid parallel edits.

