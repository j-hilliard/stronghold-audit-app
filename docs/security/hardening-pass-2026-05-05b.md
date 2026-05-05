# Hardening Pass B — 2026-05-05

**Branch:** feat/phase-2c-enhancements  
**Pass:** Second hardening pass — security, reporting alignment, bootstrap, scheduled report safety

---

## Changes Applied

### 1. CORS — Environment-Aware Policy (`Api/Program.cs`)

**Before:** `app.UseCors(AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader())` — wide open in all environments.

**After:**
- Local: still AllowAnyOrigin (Vite dev server at localhost:5173 requires this)
- Non-local: reads `Cors:AllowedOrigins` from config. If configured → restricts to those origins with credentials. If not configured → falls back to open policy with a **log warning** so operators know to configure it.

**Config keys added:**
- `appsettings.json`: no Cors key (non-local behavior uses App Config)
- `appsettings.Local.json`: `Cors:AllowedOrigins: ["http://localhost:5173", "http://localhost:7220"]`

**Production action required:** Add `Cors:AllowedOrigins` to Azure App Config pointing at the production app URL. Until that's set, the warning fires but behavior is unchanged from before.

---

### 2. Auth Cache TTL Reduction (`Api/Authorization/AuthorizationBehavior.cs`)

**Before:** 60-minute absolute expiry per user — a demoted user retained access for up to 1 hour.

**After:** 5-minute absolute expiry — role changes take effect within one cache window.

**Tradeoff:** Slightly more DB reads per user (one read per 5 min instead of per 60 min). Acceptable for a low-request-rate internal audit app.

---

### 3. Reporting Permission Alignment

**Problem found:** `canViewReports` in `userStore.ts` included AuditAdmin, Executive, AuditManager, Admin — but NOT AuditReviewer. The backend `GetComplianceStatus`, `GetScheduledReports`, and `GenerateReport` handlers all allow AuditReviewer. This meant AuditReviewers could call the APIs but the frontend route guard (`path.includes('/reports') && !canViewReports`) blocked them from every `/reports` page.

**Fix (userStore.ts):** Added `isAuditReviewer.value` to `canViewReports`.

**Fix (module router):** Added `requiresReports: true` meta flag to all report routes (reports, reports/composer, reports/gallery, reports/scheduled, reports/by-employee). This makes guard intent explicit in the route definition rather than relying on the fragile `path.includes('/reports')` string match.

**Fix (root router):** Replaced `path.includes('/reports') && !canViewReports` check with `meta.requiresReports && !canViewReports`. Explicit meta check is cleaner and not fragile against route renaming.

---

### 4. Default Landing Route (`webapp/src/modules/audit-management/router/index.ts`)

**Before:** Default empty path redirected to `/reports`. Users without `canViewReports` (Auditor, NormalUser, ITAdmin) immediately hit `/unauthorized` on first load.

**After:** Redirects to `/audits`. Most audit roles have `canViewAudits`; the route guard then handles any remaining restrictions gracefully.

---

### 5. DbInitializer — Idempotent Seeding + Role Description Fix

**Fix:** `SeedUserRoles()` previously bailed with `if (dbContext.Roles.Any()) return`. This meant new base roles added to the list were never seeded if any roles existed. Changed to per-role existence check (matching the already-correct `SeedAuditRoles` pattern).

**Role description fix:** `AuditReviewer` was described as "read-only access to submitted audits and reports. Cannot edit templates or audits." This was stale — AuditReviewer can edit responses on UnderReview audits, write review summaries, approve, and distribute. Description updated to match actual behavior.

---

### 6. ScheduledReportService — Config Gate (`Api/BackgroundServices/ScheduledReportService.cs`)

**Problem:** Service registered unconditionally. In Local dev it attempted PDF generation (PuppeteerSharp) on a 5-minute timer even when no one wanted it. Email was already DryRun-gated by EmailService, but PDF generation ran anyway and consumed resources.

**Fix:** Added config check at startup of `ExecuteAsync`:
```csharp
if (!_config.GetValue<bool>("ScheduledReports:Enabled", true)) { return; }
```

**Config values set:**
- `appsettings.json`: `ScheduledReports:Enabled = true` (production default preserved — no prod config change needed)
- `appsettings.Local.json`: `ScheduledReports:Enabled = false` (local dev disabled)

---

## Verified Route Guard Matrix (post-fix)

| Route | Guard Mechanism | Allowed Roles |
|-------|----------------|---------------|
| `/audits` | `meta.requiresAudit` + root guard | All audit roles + Executive |
| `/audits/new` | `meta.requiresCreateAudit` | AuditAdmin, AuditManager, Auditor, Admin |
| `/audits/:id/review` | root guard regex + `!isAuditReviewer && !isAuditAdmin && !isAdmin` | AuditReviewer, AuditAdmin, Admin |
| `/corrective-actions` | root guard `meta.requiresCA` / path check | All except ITAdmin |
| `/reports` + sub-routes | `meta.requiresReports` | AuditAdmin, Executive, AuditManager, **AuditReviewer** (new), Admin |
| `/admin/templates` | `meta.requiresAuditAdmin` | AuditAdmin, TemplateAdmin, Admin |
| `/admin/settings` | `meta.requiresAuditAdmin` | Same |
| `/admin/audit-log` | `meta.requiresAuditAdmin` | Same |
| `/admin/users` | `meta.requiresITAdmin` | ITAdmin, Admin |
| `/newsletter/template-editor` | `meta.requiresAuditAdmin` | AuditAdmin, TemplateAdmin, Admin |

---

## Responsive Audit

See separate report in this directory. Code fixes applied where breakage found.

---

## Build Verification

- `dotnet build Api/Api.csproj --no-incremental`: 0 CS compiler errors (DLL copy warnings only from running process)
- E2E functional tests: see test run in session

---

## Remaining Deferred Items

| Item | Reason deferred |
|------|----------------|
| CORS: set Cors:AllowedOrigins in production App Config | Requires Azure portal access to set in App Config; code is ready |
| Admin users panel: invalidate auth cache on role change | Would require injecting IMemoryCache into admin user handlers; lower priority now that TTL is 5 min |
| Graph/Outlook draft integration | Explicitly out of scope |
| Light mode | Out of scope |
