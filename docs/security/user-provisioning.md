# User and Role Provisioning

**Last updated:** 2026-05-05

---

## How Users Are Created

### Production / Azure AD
Users are **not pre-created** in the database. On first login via Azure AD (MSAL):

1. The user authenticates against the Stronghold Azure AD tenant.
2. `AuthorizationBehavior` resolves the user by email from the `Users` table.
3. If no matching user exists, a new `Users` record is inserted automatically (email, display name from the Azure AD token claims).
4. The user has **no roles assigned** by default — they land in a "pending access" state.
5. An IT Admin must navigate to **Admin → Users** and assign at least one role before the user can access any audit functionality.

### Role Assignment
Roles are stored in `UserRoles` (UserId, RoleName). Assignment is done via the Admin Users UI:
- IT Admin opens **Admin → Users**.
- Selects a user, then assigns one or more roles from the dropdown.
- Changes are effective immediately (no restart required — role cache TTL is 60 minutes per user session).

---

## Seeded Reference Data (All Environments)

`DbInitializer.Initialize()` runs on startup in all environments. It seeds:

| Data | Method | Idempotent? |
|------|--------|-------------|
| User role definitions | `SeedUserRoles()` | ✅ — existence check per role |
| Audit role definitions | `SeedAuditRoles()` | ✅ — existence check per role |
| Divisions, templates, categories, response types | `AuditDbInitializer.SeedAuditData()` | ✅ — upsert by code/name |

---

## Local Development Seeding

When `ASPNETCORE_ENVIRONMENT=Local`, additional seed data is injected:

| Data | Method | Notes |
|------|--------|-------|
| Local admin user | `SeedDefaultLocalUser()` | `joseph.hilliard@thestrongholdcompanies.com` with Administrator role |
| Test users | `SeedLocalTestUsers()` | 5 accounts — see below |
| Demo audits | `AuditDemoDataSeeder.SeedDemoAudits()` | 20+ fictional audits per division; bails if any Submitted/Closed audit already exists |

### Local Test Users

| Email | Role | Use For |
|-------|------|---------|
| `itadmin@local.dev` | ITAdmin | Testing user management |
| `auditor@local.dev` | Auditor | Testing audit create/submit (division-scoped) |
| `auditadmin@local.dev` | AuditAdmin | Testing full audit lifecycle |
| `executive@local.dev` | Executive | Testing reports/compliance read-only |
| `normaluser@local.dev` | NormalUser | Testing corrective actions assignee view |

**Note:** In Local environment, auth is fully bypassed by default. The `DEV` role switcher pill (bottom-right) in the UI impersonates any role via `localStorage['stronghold-audit-dev-role']`. Available roles in the switcher: ITAdmin, Auditor, AuditAdmin, AuditReviewer, AuditManager, TemplateAdmin, Executive, NormalUser.

---

## Development / Production Environments

`DbInitializer.Initialize()` runs but **demo data and local test users are NOT seeded**. The call site in `Program.cs` is:

```csharp
// Development + Production: reference data only
DbInitializer.Initialize(context);
// Local only adds:
//   DbInitializer.SeedDefaultLocalUser(context);
//   DbInitializer.SeedLocalTestUsers(context);
//   AuditDemoDataSeeder.SeedDemoAudits(context);
```

Real users must be provisioned via Azure AD first-login + IT Admin role assignment.

---

## Role Cache

Resolved roles and division assignments are cached in memory per HTTP request scope (`IAuditUserContext` is scoped). There is no cross-request in-memory cache — every request resolves fresh from the DB via `AuthorizationBehavior`. Role changes take effect on the user's next request.
