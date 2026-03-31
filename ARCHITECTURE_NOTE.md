# Architecture Note — Stronghold Audit App

## What This Is

This project is the **Stronghold Compliance Audit Application**, built inside a copy of the
STH Enterprise Vue Template. The original template folder (`Desktop/Enterprise template`) is
preserved untouched and is never modified here.

The legacy audit files (`Desktop/Audit Form/`) are **read-only reference only** — they define
the behavioral specification (questions, scoring logic, statuses, workflows) but are never
modified and never imported into this project.

---

## Stack

| Layer | Technology |
|---|---|
| Frontend | Vue 3 · TypeScript · Vite 6 · PrimeVue · Tailwind CSS · Pinia · Vue Router |
| Backend | ASP.NET Core · C# · CQRS via MediatR · FluentValidation · AutoMapper |
| ORM | Entity Framework Core · SQL Server / Azure SQL |
| API Docs | NSwag (auto-generates `webapp/src/apiclient/client.ts` from the API) |
| Auth | Auth bypass for Phase 1 (`VITE_BYPASS_AUTH=true`). Azure AD SSO added in a later phase. |
| Testing | Playwright (visual, E2E, integration, logic) |

---

## Dev Ports

| Service | Port |
|---|---|
| Frontend (Vite) | **https://localhost:7220** |
| Backend API | **https://localhost:7221** |

These ports are reserved for this app. They do not conflict with other apps on 7208–7215.

---

## Extension Points — Where Audit Code Plugs In

These are the four files you touch to register a new module in this template:

| File | What to do |
|---|---|
| `webapp/src/apps.ts` | Add the audit app entry to the app registry |
| `webapp/src/router/index.ts` | Register the `/audit` route group under `AppLayout` |
| `webapp/src/stores/appStore.ts` | Add audit menu items (sidebar navigation) |
| `webapp/src/modules/audit-management/` | All new audit feature code lives here |

---

## Non-Negotiables — Do Not Modify These

These files/components belong to the enterprise shell. The audit module plugs **into** them,
not **around** them. They are not redesigned or replaced.

| Component / File | Role |
|---|---|
| `webapp/src/layout/AppLayout.vue` | Main layout wrapper (header, sidebar, content area) |
| `webapp/src/components/layout/TheTopBar.vue` | Top navigation bar |
| `webapp/src/components/layout/TheMenu.vue` | Sidebar menu |
| `webapp/src/stores/userStore.ts` | Auth state and user info |
| `webapp/src/stores/apiStore.ts` | Axios instance, token management, interceptors |
| `webapp/src/assets/css/theme.css` | Global dark theme (Lara Dark Blue / PrimeVue) |
| `webapp/src/components/` | Base UI components (buttons, forms, tables, dialogs) |
| `Api/Authorization/` | Authorization attribute and behavior pipeline |
| `Api/Domain/LoggingBehavior.cs` | MediatR pipeline logging |

---

## Module Structure

New audit code follows the same pattern as `incident-management`:

```
webapp/src/modules/audit-management/
  router/
    index.ts                  ← audit route definitions
  features/
    audit-dashboard/
    audit-form/               ← core form engine
    audit-review/
    template-manager/         ← admin: add/remove questions, publish versions
    reports/
    admin-settings/
  stores/
    auditStore.ts
    templateStore.ts
  components/                 ← shared components within the audit module
```

Backend follows the same pattern as `Api/Domain/IncidentReports/`:

```
Api/Domain/Audits/
  CreateAudit.cs
  GetAudit.cs
  GetAuditList.cs
  SaveAuditResponses.cs
  SubmitAudit.cs
  GetAuditReview.cs

Api/Domain/AuditTemplates/
  GetActiveTemplate.cs
  CloneTemplateVersion.cs
  AddQuestion.cs
  RemoveQuestion.cs
  ReorderQuestions.cs
  PublishTemplateVersion.cs
  GetArchivedQuestions.cs

Api/Controllers/
  AuditController.cs
```

---

## Auth — Phase 1

Auth is bypassed for Phase 1 local development:
- `VITE_BYPASS_AUTH=true` in `webapp/.env`
- `LocalDevAuthHandler.cs` in `Api/Authorization/` bypasses JWT validation when `ASPNETCORE_ENVIRONMENT=Local`
- No Azure AD tenant, client ID, or secret needed locally

Azure AD SSO is added in a later phase. The auth hooks are already in the codebase — it is
a configuration change, not an architectural change.

---

## Documentation Index

All documentation lives in `docs/` alongside the code:

```
docs/
  architecture/
    ARCHITECTURE_NOTE.md      ← this file
    system-overview.md        ← Mermaid C4 system diagram
    diagrams/
      system-overview.drawio
      azure-infrastructure.drawio  (added before cloud deploy)
  database/
    schema.md                 ← Mermaid ER diagram
    schema.dbml               ← dbdiagram.io notation
    migrations-log.md         ← what each migration added and why
  api/
    api-contract.md           ← endpoint list, shapes, auth rules
  workflows/
    audit-lifecycle.md        ← Mermaid state diagram
    audit-workflow-swimlane.drawio
    template-versioning.md
    score-calculation.md
  frontend/
    component-guide.md
    store-guide.md
  decisions/
    ADR-001 through ADR-008
```
