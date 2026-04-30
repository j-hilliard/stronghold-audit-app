# Stronghold Audit App — Complete Project Handbook
*Last updated: 2026-04-23. Moving to new PC reference document.*

---

## Table of Contents
1. [What This App Is](#1-what-this-app-is)
2. [Tech Stack](#2-tech-stack)
3. [Solution Structure](#3-solution-structure)
4. [How to Start the App Locally](#4-how-to-start-the-app-locally)
5. [Environment & Configuration](#5-environment--configuration)
6. [Database — Schema, Models, Migrations](#6-database--schema-models-migrations)
7. [Backend Architecture](#7-backend-architecture)
8. [Authorization & Role System](#8-authorization--role-system)
9. [All API Endpoints](#9-all-api-endpoints)
10. [Frontend Architecture](#10-frontend-architecture)
11. [All Frontend Pages & Routes](#11-all-frontend-pages--routes)
12. [Background Services](#12-background-services)
13. [Email System](#13-email-system)
14. [AI Summary Feature](#14-ai-summary-feature)
15. [PDF Generation](#15-pdf-generation)
16. [Audit Trail & Non-Repudiation System](#16-audit-trail--non-repudiation-system)
17. [Complete Audit Workflow (end-to-end)](#17-complete-audit-workflow-end-to-end)
18. [Corrective Action Workflow](#18-corrective-action-workflow)
19. [Template Management Workflow](#19-template-management-workflow)
20. [CI/CD Pipeline](#20-cicd-pipeline)
21. [What Is Done / What Is Left](#21-what-is-done--what-is-left)
22. [Known Bugs & Fix Queue](#22-known-bugs--fix-queue)
23. [Migration Runbook](#23-migration-runbook)
24. [Dev Tips & Gotchas](#24-dev-tips--gotchas)

---

## 1. What This App Is

**Stronghold Audit App** is an enterprise safety compliance audit platform for Stronghold Companies. It manages the full lifecycle of field safety audits:

- Auditors go to job sites and conduct audits against a configurable question template
- Non-conforming findings auto-generate corrective actions (CAs) assigned to responsible parties
- AuditAdmins review submitted audits, generate an AI summary, and close them out
- Executives and management see dashboards, KPIs, trend reports
- Admins manage the audit template, user roles, email routing, and division settings

The app integrates with Azure Active Directory (SSO), uses a Claude AI model for auto-generating plain-language audit summaries, generates PDF reports via headless Chromium, and sends HTML email notifications.

---

## 2. Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET 10), C# |
| Pattern | MediatR (CQRS) — every operation is a Request/Handler class |
| Database | SQL Server (Azure SQL in prod), EF Core 10 Code First |
| ORM | Entity Framework Core — models drive schema, always |
| Auth | Azure Active Directory (MSAL), Microsoft.Identity.Web |
| Email | System.Net.Mail (SmtpClient) — no third-party library yet |
| PDF | PuppeteerSharp (headless Chromium — singleton) |
| AI | Claude Haiku via Anthropic HTTP API — generates audit summaries |
| Frontend | Vue 3 (Composition API) + Vite |
| UI Library | PrimeVue 4 (dark theme) + Tailwind CSS |
| State | Pinia stores |
| API Client | NSwag auto-generated `client.ts` (regenerated on every API build) |
| E2E Tests | Playwright |
| CI/CD | Azure DevOps Pipelines (`azure-pipelines.yml`) |
| Repo | Azure DevOps Git |

---

## 3. Solution Structure

```
Stronghold Audit App/           ← solution root
├── STH App Template.sln
├── CLAUDE.md                   ← AI agent rules (mandatory QA loop, EF rules)
├── dev-start.bat               ← one-click local launcher
├── azure-pipelines.yml         ← CI/CD pipeline
├── docker-compose.yml
│
├── Api/                        ← ASP.NET Core web API
│   ├── Api.csproj
│   ├── Program.cs              ← DI wiring, middleware, startup
│   ├── appsettings.json
│   ├── appsettings.Local.json  ← local dev overrides (not committed)
│   ├── Authorization/
│   │   ├── AuthorizationBehavior.cs    ← MediatR pipeline: enforces [AllowedAuthorizationRole]
│   │   ├── AllowedAuthorizationRole.cs ← attribute used on every Request class
│   │   ├── LocalDevAuthHandler.cs      ← bypasses Azure AD in Local env
│   │   └── AuditUserContext.cs         ← scoped: holds userId, isGlobal, divisionIds for request
│   ├── Controllers/
│   │   ├── AuditController.cs          ← all audit-module endpoints
│   │   ├── UserController.cs
│   │   ├── RoleController.cs
│   │   ├── UserRoleController.cs
│   │   ├── ReferenceDataController.cs
│   │   ├── SettingsController.cs
│   │   └── AdHelperController.cs
│   ├── Domain/
│   │   └── Audit/
│   │       ├── Admin/          ← template, settings, logs, user-role admin commands
│   │       ├── Audits/         ← audit lifecycle commands (create, submit, close, etc.)
│   │       ├── Divisions/      ← division settings
│   │       ├── Export/         ← Excel export of CAs
│   │       ├── Newsletter/     ← newsletter template editor
│   │       ├── ReportDrafts/   ← report composer drafts
│   │       ├── Reports/        ← compliance dashboard, scheduled reports, employee audits
│   │       └── Templates/      ← template version publishing
│   ├── Infrastructure/
│   │   └── AuditTrailInterceptor.cs    ← EF SaveChanges interceptor (auto audit trail)
│   ├── Services/
│   │   ├── EmailService.cs             ← SMTP email, dry-run mode in local
│   │   ├── AuditLogService.cs          ← human-readable action log writer
│   │   ├── AuditSummaryService.cs      ← Claude AI audit summary
│   │   ├── PdfGeneratorService.cs      ← PuppeteerSharp PDF generation
│   │   ├── CaReminderService.cs        ← daily background CA reminder emails
│   │   ├── ProcessLogService.cs        ← writes to ProcessLog table
│   │   ├── LogPurgeService.cs          ← purges old process logs
│   │   └── GraphService.cs             ← Microsoft Graph API calls
│   └── BackgroundServices/
│       └── ScheduledReportService.cs   ← sends scheduled reports via email
│
├── Data/                       ← EF Core Data layer
│   ├── Data.csproj
│   ├── AppDbContext.cs         ← all DbSets + OnModelCreating config
│   ├── DbInitializer.cs        ← seed data (roles, divisions, test users, demo audits)
│   ├── AuditDbInitializer.cs   ← audit-specific seed data
│   ├── AuditDemoDataSeeder.cs  ← demo audit records for local dev
│   ├── Models/
│   │   ├── Audit/              ← all audit-domain entity classes (32 models)
│   │   ├── User.cs
│   │   ├── UserRole.cs
│   │   ├── Role.cs
│   │   └── ...
│   └── Migrations/             ← EF migration files (never hand-edit)
│
├── Shared/                     ← shared enums/DTOs used by both Api and webapp
│   ├── Shared.csproj
│   └── Enumerations/
│       └── AuthorizationRole.cs   ← the authoritative role enum
│
└── webapp/                     ← Vue 3 frontend
    ├── src/
    │   ├── apiclient/
    │   │   ├── client.ts           ← AUTO-GENERATED by NSwag (never hand-edit)
    │   │   └── auditClient.ts      ← hand-written client for audit-specific endpoints
    │   ├── modules/
    │   │   └── audit-management/
    │   │       ├── features/       ← one folder per page/feature
    │   │       ├── stores/         ← Pinia stores for audit state
    │   │       └── router/
    │   │           └── index.ts    ← audit module routes
    │   ├── stores/
    │   │   ├── userStore.ts        ← current user, role helpers
    │   │   └── apiStore.ts         ← Axios instance, base URL
    │   └── router/
    │       └── index.ts            ← root router, auth guard
    ├── tests/e2e/                  ← Playwright E2E tests
    ├── .env.local                  ← local env (VITE_APP_API_BASE_URL etc.)
    └── package.json
```

---

## 4. How to Start the App Locally

### One-Click (Recommended)
Double-click `dev-start.bat` in the solution root.

This opens two terminal windows:
- **Audit API** → builds the API, starts at `http://localhost:7221`
- **Audit Frontend** → runs Vite dev server at `http://localhost:7220`

After API starts, the script auto-opens:
- `http://localhost:7220/audit-management/audits` (the app)
- `http://localhost:7221/swagger` (Swagger UI)

### Manual Start

**API:**
```bash
cd Api
set ASPNETCORE_ENVIRONMENT=Local
set ASPNETCORE_URLS=http://localhost:7221
dotnet build Api.csproj -p:skipNswagClientGeneration=true
dotnet run --no-build --no-launch-profile
```

**Frontend:**
```bash
cd webapp
npm run dev
```

### Dev Role Override
In Local environment you can override your role without changing DB records.
Open browser DevTools console and run:
```javascript
localStorage.setItem('stronghold-audit-dev-role', 'AuditAdmin')
```
Then refresh. Supported values: `Auditor`, `AuditAdmin`, `ITAdmin`, `Executive`, `NormalUser`, `Administrator`.

Clear override: `localStorage.removeItem('stronghold-audit-dev-role')`

The header `X-Dev-Role-Override` is automatically sent with every API request when a dev role is active. The `AuthorizationBehavior` reads this header and enforces the `[AllowedAuthorizationRole]` attribute using only that role.

---

## 5. Environment & Configuration

### Environments
| Name | Description |
|---|---|
| `Local` | Developer's machine. Auth bypassed. DB auto-migrated on startup. Demo data seeded. |
| `Development` | Azure dev slot. Azure AD auth. DB migrations run by CI/CD pipeline. |
| `Production` | Azure prod slot. Same as Development but production seed only. |

### Key Config Sections (`appsettings.Local.json`)
```json
{
  "ConnectionStrings": {
    "SqlDb": "<your local SQL connection string>"
  },
  "AzureAd": {
    "TenantId": "78d53608-54ca-4a74-8beb-8a1399c1189c",
    "ClientId": "619f5cca-8c0c-465c-8cfc-25427697f82c"
  },
  "Email": {
    "DryRun": true,
    "DevRedirectAddress": "joseph.hilliard@thestrongholdcompanies.com",
    "From": "noreply@thestrongholdcompanies.com",
    "Host": "",
    "Port": 587,
    "EnableSsl": true,
    "Username": "",
    "Password": ""
  },
  "Anthropic": {
    "ApiKey": ""
  },
  "App": {
    "BaseUrl": "http://localhost:7220"
  }
}
```

**Email behavior:**
- `DryRun: true` AND `Host` empty → logs the email, never sends
- Set `Host`, `Username`, `Password` to enable actual SMTP (Office 365: `smtp.office365.com:587`)
- `DevRedirectAddress` → all emails go to that address instead of real recipients (safe testing)

**Anthropic API Key:**
- Set to enable AI audit summaries on Submit
- Leave empty → AI silently disabled, summaries just don't appear

### Frontend Env Variables (`webapp/.env.local`)
```
VITE_APP_API_BASE_URL=http://localhost:7221
VITE_APP_BASE_URL=http://localhost:7220
VITE_BYPASS_AUTH=true
```

---

## 6. Database — Schema, Models, Migrations

### Schema
All audit tables are in the `audit` SQL schema. User/auth tables are in `dbo`.

### Key Tables

| Table | Description |
|---|---|
| `audit.Audit` | Main audit record. Status: Draft→Submitted→Closed (or Reopened) |
| `audit.AuditHeader` | Header fields per audit: Auditor, JobNumber, Location, AuditDate, PM, Client |
| `audit.AuditResponse` | One row per question answered in an audit |
| `audit.AuditFinding` | Generated on Submit for each NonConforming response |
| `audit.CorrectiveAction` | CA record. Status: Open→InProgress→Closed (or Voided) |
| `audit.CorrectiveActionPhoto` | Photos attached to CA closure |
| `audit.AuditAttachment` | File attachments on an audit |
| `audit.FindingPhoto` | Photos attached to NC findings |
| `audit.AuditTemplate` | A template (e.g. "Safety Audit") |
| `audit.AuditTemplateVersion` | Versioned snapshot of a template. Status: Draft→Published |
| `audit.AuditSection` | Section within a template version |
| `audit.AuditQuestion` | Question within a section. Has flags: RequirePhotoOnNc, AutoCreateCa |
| `audit.AuditVersionQuestion` | Junction: which questions appear in a version |
| `audit.AuditEnabledSection` | Per-division enabled sections (optional per-division filtering) |
| `audit.EmailRoutingRule` | Per-division email recipients for submission notifications |
| `audit.ReviewGroupMember` | Global review group recipients |
| `audit.Division` | Division/BU. Has ScoreTarget, SLA fields |
| `audit.DivisionJobPrefix` | Auto-complete job number prefixes per division |
| `audit.AuditActionLog` | Human-readable action log. Written by handlers explicitly. |
| `audit.AuditTrailLog` | EF-level change trail. Auto-written by AuditTrailInterceptor. |
| `audit.CaNotificationLog` | Dedup log for CA reminder emails |
| `audit.NewsletterTemplate` | HTML templates for newsletter/report distribution |
| `audit.ReportDraft` | Saved report composer drafts |
| `audit.ScheduledReport` | Scheduled report delivery configs |
| `dbo.Users` | User accounts synced from Azure AD |
| `dbo.Roles` | Role definitions |
| `dbo.UserRoles` | User↔Role assignments |
| `dbo.UserDivisions` | User↔Division scope assignments |

### Migration Rules (NON-NEGOTIABLE)
1. **Never** edit the database directly — only through EF migrations
2. **Never** hand-edit migration files
3. **Never** call `EnsureCreated()` anywhere
4. Always generate with: `dotnet ef migrations add <Name> --project Data --startup-project Api --context AppDbContext`
5. Use the `/migrate <Name>` skill in Claude Code
6. In Local: `Database.Migrate()` runs automatically at startup
7. In Dev/Prod: CI/CD pipeline applies `migrations.sql` (idempotent) before deployment

### All Migrations (in order)
Latest is `20260423113525_add_audit_trail_and_action_logs` — this is the currently applied state.

---

## 7. Backend Architecture

### MediatR CQRS Pattern
Every operation is a `Request` class + `Handler` class in `Api/Domain/`.

```csharp
// Request (in Domain/Audit/Audits/CloseAudit.cs)
[AllowedAuthorizationRole(AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator)]
public class CloseAudit : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string ClosedBy { get; set; } = null!;
}

// Handler in same file
public class CloseAuditHandler : IRequestHandler<CloseAudit, Unit> { ... }
```

Controllers call `_mediator.Send(new CloseAudit { ... })`.

### Pipeline Behaviors (run on every request)
1. `LoggingBehavior` — logs request entry/exit + duration
2. `AuthorizationBehavior` — validates `[AllowedAuthorizationRole]` attribute; populates `IAuditUserContext`

### IAuditUserContext
Scoped per HTTP request. Populated by `AuthorizationBehavior` after auth check.
```csharp
public interface IAuditUserContext {
    int UserId { get; }
    bool IsGlobalRole { get; }       // true = sees all divisions
    IReadOnlyList<int> DivisionIds { get; }  // scoped divisions if not global
    void Initialize(int userId, bool isGlobal, List<int> divisionIds);
}
```

### Auto-Registration
New first-time Azure AD users are automatically provisioned in the DB when they hit any authenticated endpoint (handled in `AuthorizationBehavior.cs`). They get the `User` role by default.

---

## 8. Authorization & Role System

### Official User-Facing Roles (what gets assigned to real users)

| Role | What They Can Do |
|---|---|
| **ITAdmin** | Manage users and roles. See admin section. |
| **Auditor** | Create audits, fill them out, submit for review, delete their own Draft audits. Cannot close/reopen. |
| **AuditAdmin** | Everything Auditor does + review submitted audits, close/reopen audits, manage CAs, manage templates, settings, email routing, delete any audit regardless of status. |
| **Executive** | Read-only dashboard + reports. Cannot create or modify audits. |
| **NormalUser** | View their assigned CAs, close CAs assigned to them. |

### Legacy/Internal Roles (still in system, not assigned to new users)
`TemplateAdmin`, `AuditManager`, `AuditReviewer`, `CorrectiveActionOwner`, `ReadOnlyViewer`, `ExecutiveViewer`, `Administrator`

`Administrator` is the superuser — bypasses everything.

### Frontend Role Helpers (`userStore.ts`)
```typescript
isAdmin          // Administrator role
isAuditAdmin     // AuditAdmin OR TemplateAdmin OR Administrator
isAuditor        // Auditor role
isITAdmin        // ITAdmin role
isExecutive      // Executive or ExecutiveViewer
isNormalUser     // NormalUser or CorrectiveActionOwner
canViewAudits    // any audit role
canCreateAudit   // AuditManager, Auditor, AuditAdmin
canManageCas     // most roles
canAccessAdminTemplates  // TemplateAdmin or AuditAdmin
```

### Division Scoping
- Global roles (Administrator, AuditAdmin, AuditManager, Executive, TemplateAdmin, ExecutiveViewer) see ALL divisions
- Scoped roles see only divisions in their `UserDivisions` assignment
- Role cache: 60 minutes in `IMemoryCache` per user

---

## 9. All API Endpoints

All under `AuditController.cs` at `/v1/`. Controller base class adds version prefix.

### Audits
| Method | Path | Handler | Auth |
|---|---|---|---|
| GET | `/v1/audits` | GetAuditList | Any audit role |
| GET | `/v1/audits/{id}` | GetAudit | Any audit role |
| POST | `/v1/audits` | CreateAudit | Auditor, AuditAdmin, AuditManager, TemplateAdmin, Admin |
| PUT | `/v1/audits/{id}` | SaveAuditResponses | Auditor, AuditAdmin, AuditManager, TemplateAdmin, Admin |
| POST | `/v1/audits/{id}/submit` | SubmitAudit → returns `SubmitAuditResult` | Auditor, AuditAdmin, AuditManager, TemplateAdmin, Admin |
| POST | `/v1/audits/{id}/close` | CloseAudit | AuditAdmin, AuditManager, TemplateAdmin, Admin |
| POST | `/v1/audits/{id}/reopen` | ReopenAudit | AuditAdmin, AuditManager, TemplateAdmin, Admin |
| DELETE | `/v1/audits/{id}` | DeleteAudit | AuditAdmin, AuditManager, TemplateAdmin, Admin, Auditor |
| GET | `/v1/audits/{id}/review` | GetAuditReview | Any audit role |

### Corrective Actions
| Method | Path | Handler |
|---|---|---|
| GET | `/v1/audits/corrective-actions` | GetCorrectiveActions |
| PUT | `/v1/audits/corrective-actions/{id}` | UpdateCorrectiveAction |
| POST | `/v1/audits/corrective-actions/{id}/assign` | AssignCorrectiveAction |
| POST | `/v1/audits/corrective-actions/{id}/close` | CloseCorrectiveAction |
| POST | `/v1/audits/corrective-actions/bulk-update` | BulkUpdateCorrectiveActions |
| GET | `/v1/audits/corrective-actions/export` | ExportCorrectiveActions |
| POST | `/v1/audits/corrective-actions/{id}/photos` | UploadCaClosurePhoto |

### Attachments & Photos
| Method | Path |
|---|---|
| GET | `/v1/audits/{id}/attachments` |
| POST | `/v1/audits/{id}/attachments` |
| DELETE | `/v1/audits/{id}/attachments/{attachmentId}` |
| GET | `/v1/audits/{id}/attachments/{attachmentId}/download` |
| GET | `/v1/audits/{id}/findings/photos` |

### Reports & Dashboard
| Method | Path | Handler |
|---|---|---|
| GET | `/v1/reports/compliance-status` | GetComplianceStatus |
| GET | `/v1/reports/audits-by-employee` | GetAuditsByEmployee |
| GET | `/v1/reports/generate` | GenerateReport → returns PDF file |
| GET | `/v1/reports/audit-report/{id}` | GetAuditReport |
| GET | `/v1/reports/section-trends` | GetSectionTrends |
| GET | `/v1/reports/repeat-findings` | GetRepeatFindings |
| GET | `/v1/reports/question-history/{id}` | GetQuestionHistory |
| GET | `/v1/reports/prior-audit-prefill` | GetPriorAuditPrefill |
| GET/POST/DELETE | `/v1/reports/scheduled` | Scheduled report CRUD |

### Admin
| Method | Path | Handler |
|---|---|---|
| GET | `/v1/admin/templates` | GetTemplates |
| GET | `/v1/admin/templates/{id}` | GetDraftVersionDetail |
| POST | `/v1/admin/templates/{id}/sections` | AddSection |
| PUT | `/v1/admin/templates/{id}/sections/{sectionId}` | UpdateSection |
| DELETE | `/v1/admin/templates/{id}/sections/{sectionId}` | RemoveSection |
| POST | `/v1/admin/templates/{id}/sections/{sectionId}/questions` | AddQuestion |
| PUT/DELETE | `/v1/admin/templates/{id}/sections/{sectionId}/questions/{qId}` | Update/RemoveQuestion |
| POST | `/v1/admin/templates/{id}/publish` | PublishTemplateVersion |
| POST | `/v1/admin/templates/{id}/clone` | CloneTemplateVersion |
| GET | `/v1/admin/templates/section-library` | GetSectionLibrary |
| GET | `/v1/admin/templates/archived-questions` | GetArchivedQuestions |
| GET/PUT | `/v1/admin/email-routing` | GetEmailRouting, UpdateEmailRouting |
| GET/PUT | `/v1/admin/review-group` | GetReviewGroup, SaveReviewGroup |
| GET | `/v1/admin/users-with-audit-roles` | GetUsersWithAuditRoles |
| POST | `/v1/admin/users/{userId}/audit-role` | SetUserAuditRole |
| GET/PUT | `/v1/admin/divisions/{divisionId}/score-target` | SetDivisionScoreTarget |
| GET/PUT | `/v1/admin/divisions/{divisionId}/sla` | Division SLA settings |
| GET | `/v1/admin/audit-logs` | GetAuditLogs (both action + trail logs) |

### Distribution
| Method | Path |
|---|---|
| GET | `/v1/audits/{id}/distribution-preview` |
| POST | `/v1/audits/{id}/send-distribution` |
| GET/POST | `/v1/audits/{id}/distribution-recipients` |

---

## 10. Frontend Architecture

### Stores (Pinia)
- `userStore` — current user, role computed properties, login/logout
- `apiStore` — Axios instance, base URL from env
- `auditStore` — current audit being edited, submit state
- `adminStore` — admin page state (templates, settings, users)

### API Client Pattern
Two clients:
1. `client.ts` — NSwag auto-generated (regenerated when API builds). Contains User, Role, etc. Never edit manually.
2. `auditClient.ts` — Hand-written `AuditClient` class. All audit-domain endpoints that aren't in the NSwag spec. Instantiated as: `new AuditClient(apiStore.api.defaults.baseURL, apiStore.api)`

### Auth Flow
- Azure MSAL (`@azure/msal-browser`) handles SSO in non-Local environments
- In Local env (`VITE_BYPASS_AUTH=true`): auth is skipped, all requests go through
- Dev role override via `localStorage.setItem('stronghold-audit-dev-role', 'RoleName')`

---

## 11. All Frontend Pages & Routes

All routes are under `/audit-management/` prefix.

| Path | Component | Who Sees It |
|---|---|---|
| `/audit-management/reports` | ReportsView | Dashboard — compliance KPIs, division scorecards |
| `/audit-management/audits` | AuditDashboardView | Audit list — filter, search, bulk delete |
| `/audit-management/audits/new` | NewAuditView | New audit setup form |
| `/audit-management/audits/:id` | AuditFormView | Audit question form + submit |
| `/audit-management/audits/:id/review` | AuditReviewView | Review page — close, reopen, send distribution |
| `/audit-management/corrective-actions` | CorrectiveActionsView | CA list — filter, assign, close, export |
| `/audit-management/admin/templates` | TemplateManagerView | Template builder |
| `/audit-management/admin/settings` | AuditSettingsView | Email routing, score targets, SLAs, user roles |
| `/audit-management/admin/users` | AdminUsersView | User management, assign roles |
| `/audit-management/admin/audit-log` | AdminAuditLogView | Non-repudiation log viewer (Action Log + Change Trail) |
| `/audit-management/reports/composer` | ReportComposerView | Drag-and-drop report builder |
| `/audit-management/reports/gallery` | ReportGalleryView | Generate/download PDF reports |
| `/audit-management/reports/scheduled` | ScheduledReportsView | Scheduled report delivery |
| `/audit-management/reports/by-employee` | AuditsByEmployeeView | Audits grouped by employee |
| `/audit-management/newsletter/template-editor` | NewsletterTemplateEditorView | Rich HTML email template editor |

### Sidebar Nav Structure
```
Dashboard
Audits
New Audit
Corrective Actions
─── Admin ───
Templates         (AuditAdmin/TemplateAdmin)
Settings          (AuditAdmin/TemplateAdmin)
Users             (ITAdmin/Administrator)
Audit Log         (AuditAdmin/Administrator/ITAdmin)
```

---

## 12. Background Services

### CaReminderService
Runs daily at midnight. Sends CA reminder emails to assignees.
- **DueSoon**: CA due in 3 days
- **DueToday**: CA due today
- **Overdue**: CA past due (sends every day until CA closed)
- Dedup: one email per CA + type per calendar day via `CaNotificationLog` table
- Falls back gracefully if email not configured

### ScheduledReportService
Runs every hour. Checks `ScheduledReport` table for reports due.
- Generates PDF via `PdfGeneratorService`
- Sends to configured recipients
- Updates `NextRunAt` after delivery

### LogPurgeService
Purges old `ProcessLog` entries to prevent table growth. Runs on startup and periodically.

---

## 13. Email System

### Configuration
Set in `appsettings.Local.json` under `Email:` section:
```
Host     = smtp.office365.com   (Office 365)
Port     = 587
EnableSsl = true
Username = your@email.com
Password = yourpassword
From     = noreply@thestrongholdcompanies.com
```

### Behavior Modes
| Mode | When | What Happens |
|---|---|---|
| **Dry Run** | `DryRun: true` AND no `DevRedirectAddress` | Logs intent, never sends |
| **Dev Redirect** | `DevRedirectAddress` set | Sends real email but redirects ALL recipients to that address |
| **Live** | Production or `DevRedirectAddress` not set in production | Sends to real recipients |

### Email Triggers
1. **Audit Submitted** — sends to: division email routing list + global review group + all AuditAdmins
2. **CA DueSoon/DueToday/Overdue** — sends to: CA assignee
3. **Distribution Email** — manual send from Review page; sends to distribution recipients

### Mailto Fallback (current workaround)
When SMTP is not configured, the "Submit for Review" button returns a `SubmitAuditResult` with all recipients and subject populated. The frontend builds a `mailto:` link and shows a "Notify Team" button that opens the user's email client (Outlook) pre-populated. This is the current state until SMTP is configured.

---

## 14. AI Summary Feature

When an audit is submitted, `AuditSummaryService` calls Claude Haiku (`claude-haiku-4-5-20251001`) with:
- Division code
- Conformance score + prior score (for trend)
- All non-conforming findings (section + question text)
- Warning items
- Corrected-on-site count

The API key is set in `appsettings.Local.json` under `Anthropic:ApiKey`.

Generated summary is:
- Saved to `audit.AiSummary` column on the Audit record
- Included in the submission email HTML body
- Displayed on the Review page

Falls back gracefully — if key absent or API fails, `AiSummary` is null and email/UI just skips that block.

---

## 15. PDF Generation

`PdfGeneratorService` uses PuppeteerSharp (headless Chromium). Registered as a **singleton** so the browser instance is reused across requests.

On first use, it downloads Chromium automatically (requires internet access or pre-downloaded).

PDF is generated by:
1. Navigating the headless browser to the audit report URL
2. Waiting for the page to fully render
3. Calling `page.PdfAsync()` with A4 paper size

Used by:
- `GenerateReport` handler (on-demand download)
- `ScheduledReportService` (for email attachments)

---

## 16. Audit Trail & Non-Repudiation System

Two separate mechanisms working together:

### AuditTrailInterceptor (automatic)
Singleton `SaveChangesInterceptor` registered on the DbContextFactory.

Tracked entity types: `Audit`, `CorrectiveAction`, `UserRole`, `User`, `AuditTemplateVersion`, `Division`, `AuditFinding`

For every save:
- `SavingChangesAsync` → captures entries from `ChangeTracker` before save
- `SavedChangesAsync` → writes to `audit.AuditTrailLog` via raw SQL (to avoid re-triggering itself)
- `SaveChangesFailed` → discards pending entries (no false positives)

For each entry captures:
- **Insert**: all field values as JSON in `NewValues`
- **Update**: changed columns list, old values JSON, new values JSON
- **Delete**: all field values as JSON in `OldValues`

User identity from `IHttpContextAccessor` → `preferred_username` claim (Azure AD email).

### AuditLogService (explicit)
Scoped service injected into handlers. Writes human-readable entries to `audit.AuditActionLog`.

Currently called by:
- `DeleteAudit` handler — severity Warning, records who deleted and how many CAs were voided
- `SubmitAudit` handler — via ProcessLogService
- Other handlers use `ProcessLogService` (separate process log, not the audit log)

### Admin Log Viewer
Page: `Admin → Audit Log` (`/audit-management/admin/audit-log`)

Two tabs:
- **Action Log** — human-readable entries with severity badges (Info/Warning/Error), expandable rows
- **Change Trail** — field-level diffs with Before/After JSON panels (red = old, green = new)

Filters: search, user email, entity type, action, date range. Server-side pagination.

---

## 17. Complete Audit Workflow (end-to-end)

```
1. AUDITOR creates audit
   └── NewAuditView → POST /v1/audits
       - Selects division, template, header info (auditor name, job number, location, date)
       - Creates Audit record with Status = "Draft"

2. AUDITOR fills out the form
   └── AuditFormView → PUT /v1/audits/{id} (SaveAuditResponses)
       - Answers each question: Conforming / NonConforming / Warning / N/A
       - Adds comments and photos to NC findings
       - Can save partial (auto-save not implemented — manual Save button)
       - Edit button only visible for Draft and Reopened audits

3. AUDITOR submits for review
   └── AuditFormView → POST /v1/audits/{id}/submit
       PRE-CHECKS:
         - Required photos on NC questions must be attached
         - Status must be Draft or Reopened
       ON SUBMIT:
         - AuditFinding records generated for all NC responses
         - Auto-CAs created for questions with AutoCreateCa=true
         - AI summary generated (Claude Haiku)
         - Status → "Submitted"
         - Email sent to: division recipients + review group + all AuditAdmins
         - Returns SubmitAuditResult (recipients, subject, review URL)
       FRONTEND:
         - Summary modal shows score, NC count, AI summary
         - "Notify Team" mailto: button appears if SMTP not configured

4. AUDITADMIN reviews
   └── AuditReviewView (GET /v1/audits/{id}/review)
       - Sees full audit findings, AI summary, score
       - Can write review summary notes
       - Manages corrective actions (assign, close)
       - Can "Reopen" (→ Reopened) if Submitted or Closed
       - Can "Close Audit" if no open CAs remain

5. AUDITADMIN closes audit
   └── POST /v1/audits/{id}/close
       PRE-CHECK: No open/in-progress CAs (block close if any exist)
       - Status → "Closed"

6. DISTRIBUTION (optional)
   └── From Review page: "Send Distribution" button
       - Preview recipient list and email body
       - Send HTML email to distribution list

7. AUDIT DELETED (if needed)
   └── DELETE /v1/audits/{id}
       - Auditors can only delete their own Draft audits
       - AuditAdmins can delete any status
       - ALL CAs → Status="Voided", IsDeleted=true (cascade)
       - AuditActionLog entry written (Warning severity)
```

---

## 18. Corrective Action Workflow

```
Status flow: Open → InProgress → Closed
                              ↳ Voided (never manually, only on audit delete or NC cleared)

CREATION:
- Auto-generated on Submit (if question has AutoCreateCa=true and response=NonConforming)
- Manually created by AuditAdmin from Review page

ASSIGNMENT:
- POST /v1/audits/corrective-actions/{id}/assign
- Sets AssignedTo (email), DueDate
- Assignee is notified (when email configured)

PROGRESS:
- PUT /v1/audits/corrective-actions/{id}
- Can update Status to InProgress, add notes, root cause, priority

CLOSURE:
- POST /v1/audits/corrective-actions/{id}/close
- Can attach closure photo (if RequireClosurePhoto set on Division)
- Sets Status = "Closed", ClosedAt, ClosedBy

BULK UPDATE:
- POST /v1/audits/corrective-actions/bulk-update
- Capped at 50 CAs per request (security limit)
- Only non-voided CAs can be bulk updated

REMINDERS:
- CaReminderService sends daily emails: DueSoon (3 days), DueToday, Overdue
- Deduplicated via CaNotificationLog
```

---

## 19. Template Management Workflow

```
Templates have Versions. Only one Published version is active at a time.

CREATE/EDIT:
- TemplateManagerView (admin only)
- Add/edit/remove sections and questions in Draft version
- Drag-and-drop reorder sections and questions
- Copy sections from other templates (section library)
- Each question has: QuestionText, ResponseType, RequirePhotoOnNc, AutoCreateCa

PUBLISH:
- POST /v1/admin/templates/{id}/publish
- Validates draft has sections with questions
- Marks previous published version as Archived
- Current draft → Published

CLONE:
- POST /v1/admin/templates/{id}/clone
- Creates a new Draft version from any existing version

SECTION LIBRARY:
- GET /v1/admin/templates/section-library
- Returns reusable sections from all templates for copy-paste
```

---

## 20. CI/CD Pipeline

File: `azure-pipelines.yml`

**Triggers:** push to `main` or `develop`, PR against both

**Build Stage:**
1. Restore NuGet packages
2. Build .NET solution
3. **Schema change guard** — if `Data/Migrations/`, `AppDbContext.cs`, or `Data/Models/` changed, fails unless `allowSchemaChange=true` variable is set
4. Generate idempotent SQL migration script (develop branch only)
5. Package API → zip artifact
6. Install Node + Playwright
7. Run `npm run qa:pr` (full Playwright E2E suite)
8. Publish test results (JUnit XML)
9. Build Vue (`npm run build:dev`)
10. Publish artifacts (webapp + API zip + migration SQL)

**Deploy Stage:** Currently disabled (`condition: false`) — Azure resources not yet provisioned.

**Azure DevOps Org:** `stronghold-company` | Repo: `STH WebApp Security Template`

---

## 21. What Is Done / What Is Left

### DONE (implemented and working)
- Full audit lifecycle (Create → Fill → Submit → Review → Close → Reopen → Delete)
- Cascade void of CAs on audit delete
- NonConforming finding generation on Submit
- Auto-CA creation on Submit (configurable per question)
- AI audit summary (Claude Haiku) on Submit
- Submission email (HTML) to division recipients + review group + AuditAdmins
- Mailto: fallback when SMTP not configured
- Distribution email from Review page (preview, send)
- CA reminder emails (daily background service)
- Scheduled report delivery
- PDF report generation (PuppeteerSharp)
- Report composer (drag-and-drop blocks)
- Compliance dashboard with KPIs, scorecards, charts
- Template manager (sections, questions, publish, clone, section library)
- Admin settings: email routing, review group, score targets, division SLAs
- User management (AdminUsersView — list, add, edit, assign roles)
- Audit Trail + Action Log system (tables created, interceptor running, admin viewer live)
- Non-repudiation admin viewer page (Admin → Audit Log) with before/after JSON diff
- Role-based UI gating throughout (isAuditor, isAuditAdmin, etc.)
- Administrator included in isAuditAdmin (bulk delete works)
- Edit button only on Draft/Reopened audits
- Reopen allowed from both Submitted and Closed (AuditAdmin only)
- Auditor role can delete own Draft audits
- Submit button gated to correct roles (Auditor, AuditAdmin, Admin)
- Dev role override system (localStorage + X-Dev-Role-Override header)
- Azure AD SSO (non-Local environments)
- Auto-user provisioning on first login
- IDOR protection (users can only access their scoped divisions' data)
- Bulk CA update capped at 50 records

### STILL TO DO / IN PROGRESS

#### HIGH PRIORITY
1. **SendGrid SMTP** — User has SendGrid credentials. Configure `Email:Host`, `Username`, `Password` in `appsettings.Local.json` and Azure App Config for dev/prod. Until then, SMTP emails don't send (mailto: fallback works).

2. **#5 Export Source Filter** — ExportCorrectiveActions.cs has a `Source?` filter but `AuditController.cs` doesn't pass `source` from query params, and the frontend `exportExcel()` doesn't send `filterSource`. Two-line fix in controller + one line in CorrectiveActionsView.vue.

3. **#10 Print routes have no auth guard** — `/audit-management/print/:id`, `/audit-management/print-review/:id`, and report routes bypass the auth `beforeEach` guard in `webapp/src/router/index.ts`. Fix: add route meta `requiresAuth: true` or explicit guard checks.

#### MEDIUM PRIORITY
4. **#12 Role Management UI** — No frontend page to create/manage role definitions. User asked for an "Add Role" button beside Add User. This is a large build:
   - Fix `GetRole.cs` handler (currently returns all roles instead of one by ID)
   - Add DELETE endpoint to `RoleController.cs`
   - Add `Permissions` table with feature keys (migration required)
   - Replace `AllowedAuthorizationRole` attributes with dynamic permission check
   - Build admin UI: Role Management tab — create/edit/delete roles, assign per-role screen permissions

5. **Deleted audit list filter** — `GetAuditList.cs` may still show soft-deleted audits. Verify `.Where(a => !a.IsDeleted)` filter exists.

6. **Direct URL access to deleted audits** — `GetAudit.cs` needs `&& !a.IsDeleted` in the query filter.

7. **Add User duplicate email** — `CreateUser.cs` should check for duplicate email and return 409 Conflict with a clear message instead of silently failing.

8. **Role dropdown shows all roles** — Add User dialog in `AdminUsersView.vue` shows all roles including legacy ones. Should only show the 5 official user-facing roles.

#### FUTURE / ROADMAP
- Azure App Configuration service wired to dev/prod slots
- Deploy stage in CI/CD pipeline
- SendGrid integration (replace System.Net.Mail)
- Advanced RBAC with per-feature permissions (Phase 3)
- Mobile-responsive audit form (for field use)
- Offline capability for auditors in the field
- Bulk audit template import/export
- Advanced analytics — trend charts by question, by auditor, by division

---

## 22. Known Bugs & Fix Queue

*(These are the confirmed defects from the last major audit. Fix ONE at a time, verify, then next.)*

| # | Severity | Issue | Fix Location |
|---|---|---|---|
| #5 | HIGH | Export ignores Source filter | `AuditController.cs` + `CorrectiveActionsView.vue` exportExcel() |
| #10 | MEDIUM | Print/report routes have no auth guard | `webapp/src/router/index.ts` |
| #12 | MEDIUM | Role management UI missing | New page + backend CRUD |
| Check | CRITICAL | GetAuditList missing !IsDeleted filter | `Api/Domain/Audit/Audits/GetAuditList.cs` |
| Check | CRITICAL | GetAudit missing !IsDeleted filter | `Api/Domain/Audit/Audits/GetAudit.cs` |
| Check | HIGH | CreateUser no duplicate email check | `Api/Domain/Users/CreateUser.cs` |
| Check | HIGH | Role dropdown shows all roles in Add User | `webapp/src/modules/audit-management/features/admin-users/views/AdminUsersView.vue` |

---

## 23. Migration Runbook

### On New Machine — First Time Setup
```bash
# 1. Clone the repo (Azure DevOps SSH)
git clone git@ssh.dev.azure.com:v3/stronghold-company/STH%20WebApp%20Security%20Template/STH%20WebApp%20Security%20Template

# 2. Create your appsettings.Local.json in Api/ folder (NOT committed)
#    Copy the template from section 5 above and fill in your SQL connection string

# 3. Install .NET 10 SDK (check global.json for exact version)
# 4. Install Node 20 LTS
# 5. Install webapp dependencies
cd webapp && npm install

# 6. Run dev-start.bat (or start manually — see section 4)
#    On first run, EF Core will auto-migrate the database and seed test data
```

### Adding a Migration
Use the `/migrate` skill in Claude Code:
```
/migrate add_your_migration_name
```

Or manually:
```bash
cd "solution root"
dotnet tool restore
dotnet ef migrations add your_migration_name --project Data --startup-project Api --context AppDbContext
```

Then review the generated `Up()` and `Down()` methods before running.

Apply locally:
```bash
dotnet ef database update --project Data --startup-project Api --context AppDbContext --connection "your connection string"
```

### Connection String Format (SQL Server)
```
Server=YOURSERVER;Database=StrongholdAuditDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Or with SQL auth:
```
Server=YOURSERVER;Database=StrongholdAuditDb;User Id=sa;Password=yourpass;TrustServerCertificate=True;
```

---

## 24. Dev Tips & Gotchas

### API Rebuild After Code Changes
The API holds DLLs open while running. You must kill it before rebuilding.

```powershell
Stop-Process -Name "Api" -Force
```
Then rebuild: `dotnet build Api/Api.csproj -p:skipNswagClientGeneration=true`
Then restart: `dotnet run --no-build --no-launch-profile`

### NSwag Client Regeneration
The `client.ts` file is auto-generated when the API builds (unless `-p:skipNswagClientGeneration=true` is passed). On your local machine, skip it to avoid the overhead. If you add new REST endpoints to the NSwag-generated API, let it regenerate by removing the flag.

### Auth Cache
The role/division cache is 60 minutes. If you change a user's role and want it reflected immediately, restart the API (cache lives in-process).

### EF Migrations — NEVER Hand-Edit
The `Designer.cs` files next to each migration are auto-generated snapshots. Never modify them. If a migration looks wrong, delete the migration file + designer file, run `dotnet ef migrations add` again.

### ConcurrentDictionary in AuditTrailInterceptor
The interceptor is a singleton but DbContexts are scoped. The `ConcurrentDictionary<DbContext, List<AuditTrailLog>>` is keyed by context instance — this is intentional. Entries are removed after being written. If you see audit trail entries missing, check whether `SaveChangesFailed` was triggered (clears pending entries).

### One Bug at a Time
The standing rule for this project is: fix ONE bug, stop, wait for user to verify before moving to the next. Do not bundle fixes. This was established after multiple incidents where bundled changes caused regression confusion.

### Soft Deletes
All audit entities use soft delete: `IsDeleted`, `DeletedAt`, `DeletedBy` columns. The EF model does NOT have a global query filter for `!IsDeleted` — every query must manually include `.Where(a => !a.IsDeleted)`. This is intentional to allow explicit access to deleted records in admin/audit log contexts. Always verify your queries include this filter where appropriate.

### Branch Strategy
Current branch: `feat/phase-2c-enhancements`
Main branch: `main`
All work should be on feature branches and PRed to `main`.
