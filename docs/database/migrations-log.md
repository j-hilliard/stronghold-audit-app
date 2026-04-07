# Migrations Log

All schema changes are recorded here. No migration is merged without an entry in this file
and an `[SCHEMA CHANGE AUTHORIZED]` marker in the PR description.

---

## Schema Governance Rule

> **No schema migration merges to main without:**
> 1. An entry in this file
> 2. `[SCHEMA CHANGE AUTHORIZED]` in the PR description
> 3. Reviewer sign-off on the migration

---

## Migration History

### Baseline — Template Migrations (inherited from enterprise template)

| Migration | Date | Description |
|---|---|---|
| `20241111194342_InitialMigration` | 2024-11-11 | Initial schema: Roles, Users, UserRoles, IncidentReports, RefTables, ProcessLog |
| `20241203193549_AddAppOrders` | 2024-12-03 | Added app ordering to user roles |
| `20241216153312_AddAppOrdersIsAdmin` | 2024-12-16 | Added IsAdmin flag to app orders |

These migrations are from the enterprise template and are not modified.

---

### Audit Module Migrations

| Migration | Date | Authorized By | Description |
|---|---|---|---|
| `20260331234240_AuditModule` | 2026-03-31 | j.hilliard | All 14 audit tables in `audit` SQL schema: Division, AuditTemplate, AuditTemplateVersion, AuditSection, AuditQuestion, AuditVersionQuestion, Audit, AuditHeader, AuditResponse, AuditFinding, CorrectiveAction, AuditAttachment, EmailRoutingRule, TemplateChangeLog. Includes unique indexes, filtered indexes (one Active version per template), and check constraints on all Status/AuditType fields. Replaces prior `EnsureCreated()` approach — startup now uses `Database.Migrate()`. |
| `DBInitializer.Audit seed` | 2026-03-31 | j.hilliard | Seed data: 9 divisions, 487 questions, 46 email routing rules migrated from prototype. Seeder wrapped in a single DB transaction; batch inserts per section instead of per-row SaveChanges. Idempotent guard: returns immediately if Divisions table already has rows. |

---

## How to Add an Entry

When you need a schema change:

1. Create a new Draft PR
2. Add `[SCHEMA CHANGE AUTHORIZED]` to the PR title or description
3. Add a row to the table above with the migration name, date, who authorized it, and what it changes
4. Build first: `dotnet build Api/Api.csproj -p:skipNswagClientGeneration=true`
   Then: `dotnet ef migrations add {MigrationName} --project Data --startup-project Api --no-build`
   To apply: `dotnet ef database update --project Data --startup-project Api --no-build --connection "Server=localhost,14332;Database=StrongholdAuditDb;User Id=sa;Password=...;TrustServerCertificate=True;"`
   *(NSwag post-build step breaks `dotnet ef` directly — always build with `skipNswagClientGeneration=true` first)*
5. Verify the generated migration file matches what you intended
6. Get reviewer sign-off before merging
