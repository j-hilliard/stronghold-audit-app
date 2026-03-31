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

### Audit Module Migrations (to be added)

| Migration | Date | Authorized By | Description |
|---|---|---|---|
| `AuditModule_Initial` | TBD | TBD | Divisions, AuditTemplates, AuditTemplateVersions, AuditSections, AuditQuestions, AuditVersionQuestions, Audits, AuditHeaders, AuditResponses, AuditFindings, CorrectiveActions, AuditAttachments, EmailRoutingRules, TemplateChangeLog |
| `AuditModule_SeedData` | TBD | TBD | Seed: 9 divisions, initial template versions with all questions from prototype, default email routing rules |

---

## How to Add an Entry

When you need a schema change:

1. Create a new Draft PR
2. Add `[SCHEMA CHANGE AUTHORIZED]` to the PR title or description
3. Add a row to the table above with the migration name, date, who authorized it, and what it changes
4. Run `dotnet ef migrations add {MigrationName} --project Data --startup-project Api`
5. Verify the generated migration file matches what you intended
6. Get reviewer sign-off before merging
