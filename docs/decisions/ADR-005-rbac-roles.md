# ADR-005: Role-Based Access Control — Five Roles

**Status:** Accepted  
**Date:** 2026-03-31  
**Note:** Auth is bypassed in Phase 1. RBAC is enforced starting Phase 2 when Azure AD SSO is added.

## Decision

The application uses five roles. Roles are eventually sourced from Azure AD groups.
For Phase 1, all users are treated as having full access (auth bypass mode).

## Roles

| Role | Can Do |
|---|---|
| `SystemAdmin` | Everything: user management, template publishing, email routing config, all divisions, reopen submitted audits |
| `AuditManager` | View all audits across all divisions, run all reports, manage corrective actions, reopen audits |
| `DivisionManager` | View audits for assigned division(s) only, run division-scoped reports |
| `Auditor` | Create and submit audits for assigned division(s), view their own submissions |
| `ReadOnly` | View submitted audits for their division — no create, no edit, no delete |

## Reasoning

- Five roles covers the realistic user population without over-engineering permission granularity
- Division scoping at the DivisionManager and Auditor levels prevents cross-division data leakage (HR sensitivity)
- SystemAdmin and AuditManager are the only roles that can reopen a submitted audit — preventing silent after-the-fact edits to findings
- ReadOnly is important for HR personnel or leadership who need to view but must not modify records

## Consequences

- Every API endpoint declares a minimum required role using `[Authorization(...)]`
- EF Core global query filters enforce division scoping at the data layer for non-admin roles
- The `userStore` exposes role-checking computed properties (`isAdmin`, `isAuditManager`, etc.)
- Phase 2 work: Azure AD group → role mapping configured in `appsettings.json`
