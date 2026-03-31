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
| `EnsureCreated — audit schema` | 2026-03-31 | j.hilliard | All 14 audit tables created via `EnsureCreated()` in Local environment. Tables in `audit` SQL schema: Division, AuditTemplate, AuditTemplateVersion, AuditSection, AuditQuestion, AuditVersionQuestion, Audit, AuditHeader, AuditResponse, AuditFinding, CorrectiveAction, AuditAttachment, EmailRoutingRule, TemplateChangeLog |
| `DBInitializer.Audit seed` | 2026-03-31 | j.hilliard | Seed data: 9 divisions (TKIE, STS, STG, SHI, SHI_RT, SHI_RA, ETS, CSL, FACILITY), 487 questions across all division templates, 46 email routing rules migrated from SHC_Compliance_Audit_Tool.html prototype |

---

## How to Add an Entry

When you need a schema change:

1. Create a new Draft PR
2. Add `[SCHEMA CHANGE AUTHORIZED]` to the PR title or description
3. Add a row to the table above with the migration name, date, who authorized it, and what it changes
4. Run `dotnet ef migrations add {MigrationName} --project Data --startup-project Api`
5. Verify the generated migration file matches what you intended
6. Get reviewer sign-off before merging
