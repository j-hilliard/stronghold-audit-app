# Environment / Bootstrap Matrix

**Last updated:** 2026-05-05

---

## Environment Names

| Name | `ASPNETCORE_ENVIRONMENT` | Used For |
|------|--------------------------|----------|
| Local | `Local` | Developer laptops — full dev bypass, demo data |
| Development | `Development` | Azure dev slot — real auth, reference data only |
| Production | `Production` | Azure production slot — real auth, reference data only |

---

## Startup Behavior by Environment

| Behavior | Local | Development | Production |
|----------|:-----:|:-----------:|:----------:|
| `Database.Migrate()` auto-applied | ✅ | ❌ | ❌ |
| `DbInitializer.Initialize()` (roles + divisions + templates) | ✅ | ✅ | ✅ |
| `SeedDefaultLocalUser()` (joseph.hilliard admin) | ✅ | ❌ | ❌ |
| `SeedLocalTestUsers()` (5 test accounts) | ✅ | ❌ | ❌ |
| `AuditDemoDataSeeder.SeedDemoAudits()` (20+ demo audits) | ✅ | ❌ | ❌ |
| Azure AD authentication enforced | ❌ (bypassed) | ✅ | ✅ |
| `[AllowedAuthorizationRole]` checked | ❌ (bypassed unless `X-Dev-Role-Override` header set) | ✅ | ✅ |
| Dev role switcher pill visible | ✅ | ❌ | ❌ |
| Swagger UI enabled | ✅ | ✅ | ❌ |

---

## Migration Deployment

Migrations are **never** auto-applied in Development or Production. The CI/CD pipeline:

1. Builds the `Data` project.
2. Generates an idempotent SQL script:
   ```bash
   dotnet ef migrations script --idempotent --output migrations.sql \
     --project Data --startup-project Api --context AppDbContext
   ```
3. Applies `migrations.sql` to the target database before app deployment.

This means the database schema is always ahead of or equal to the running code at deployment time.

---

## Auth Bypass Details (Local Only)

In `Local` environment:
- `AuthorizationBehavior` skips role checks entirely when no `X-Dev-Role-Override` HTTP header is present.
- The dev role switcher sets `localStorage['stronghold-audit-dev-role']`; the Vue `userStore` reads this at initialization to simulate a role in the frontend.
- Even with dev role override active, the backend bypass remains — only the frontend role gate changes.
- To test real backend auth in Local: add `X-Dev-Role-Override: AuditAdmin` (or any role name) to the request headers. `AuthorizationBehavior` will then enforce role checks.

---

## PuppeteerSharp (PDF Generation)

- Headless Chromium browser is launched as a singleton at startup.
- Requests to `/v1/reports/generate-pdf` spin up one page per request.
- In Local: browser renders `http://localhost:5173` (Vite dev server must be running).
- In cloud environments: browser renders the app's own public URL from `appsettings.json → AppBaseUrl`.
- There is no environment-specific auth bypass for PuppeteerSharp requests — these are unauthenticated browser navigations to print routes; the print routes do not expose sensitive data on their own.

---

## Email (SMTP)

| Environment | Behavior |
|-------------|----------|
| Local | SMTP config in `appsettings.Local.json` — typically pointed at MailHog or disabled |
| Development | SMTP config in Azure Key Vault → `appsettings.Development.json` |
| Production | SMTP config in Azure Key Vault → `appsettings.Production.json` |

No email is sent unless `SmtpSettings.Host` is configured. Missing config throws at send time, not at startup.
