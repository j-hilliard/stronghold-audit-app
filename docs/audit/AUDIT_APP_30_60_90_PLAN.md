# AUDIT APP 30-60-90 DAY EXECUTION PLAN
Generated: 2026-04-28

---

## 30-DAY SPRINT — Foundation (P0 Items Only)

These items must be done before any major refactor is safe. They eliminate confirmed bugs, establish the service abstraction layer, and lay the design system foundation.

### 30-Day Deliverables

| Item | Description | Files | Risk |
|---|---|---|---|
| F1: Create `useAuditService()` composable | Replace all 28 `new AuditClient(...)` calls with a single shared factory composable. This is the architectural unlock for everything else. | Create: `webapp/src/services/useAuditService.ts` | Low — additive only |
| F2: Replace all 28 `new AuditClient()` calls | Update every view, composable, and component to call `useAuditService()` instead of instantiating directly. | 15 files modified (see PR1 detail below) | Low — mechanical replacement |
| F3: Fix `AdminAuditLogView.vue` runtime error | The row expansion on the Action tab throws a runtime error (confirmed in QA screenshots 78 and 80). Add null guards and optional chaining to the expanded row template. | `webapp/src/modules/audit-management/features/admin-audit-log/views/AdminAuditLogView.vue` | Low |
| F4: Add Tailwind design tokens | Define surface, brand, and status color tokens in `tailwind.config.js`. Do not replace existing classes yet — add tokens only so they are available for new work. | `webapp/tailwind.config.js` | None |
| F5: Create `EmptyState.vue` shared component | Standardize the empty state pattern across all list pages. | Create: `webapp/src/components/feedback/EmptyState.vue` | None |
| F6: Create `useCountUp.ts` shared composable | The count-up animation function is copy-pasted identically in `ReportsView.vue` and `CorrectiveActionsView.vue`. Extract to shared composable. | Create: `webapp/src/composables/useCountUp.ts` | None |
| F7: Fix Section N/A P0 gap | Enforce section N/A exclusion on the backend in `SaveAuditResponses.cs`, `GetAuditReview.cs`, `GetAuditReport.cs`. Frontend scoring already excludes N/A sections; backend must match. | `Api/Domain/Audit/Audits/SaveAuditResponses.cs`, `GetAuditReview.cs`, `Api/Domain/Audit/Reports/GetAuditReport.cs` | Medium — data logic change |

**30-Day Goal**: Zero confirmed bugs in the P0 list. Shared service composable in place. Design tokens defined. The app is stable and the foundation for refactoring exists.

---

## 60-DAY SPRINT — Architecture + Design System (P1 Items)

These are the highest-leverage structural changes. Each one improves the codebase permanently and unblocks feature work.

### 60-Day Deliverables

| Item | Description | Files | Risk |
|---|---|---|---|
| A1: Extract `useReportCharts.ts` from ReportsView | Extract all 5 chart config computeds (~300 lines). | Create composable, shrink ReportsView | Medium |
| A2: Extract `useReportDrilldown.ts` from ReportsView | Extract drill-down state (6 drill functions + filter computed + scroll logic). | Create composable, shrink ReportsView | Low |
| A3: Extract `useReportKpis.ts` from ReportsView | Extract KPI visibility state, localStorage persistence, count-up animation. | Create composable, shrink ReportsView | Low |
| A4: Extract `useAuditReportData.ts` composable | Shared data source for ReportsView, NewsletterView, QuarterlySummaryView with unified date computation. | Create composable | Low |
| A5: Shrink `ReportsView.vue` to ~250 lines | After extracting 4 composables, the view becomes orchestration-only. | Modify ReportsView.vue | Medium |
| A6: Extract `useReviewDistribution.ts` from AuditReviewView | Distribution recipient management, add/remove, filtering, dialog state. | Create composable, shrink AuditReviewView | Medium |
| A7: Extract `useReviewEmail.ts` from AuditReviewView | Distribution email preview fetch, fallback builder, send, subject edit. | Create composable, shrink AuditReviewView | Medium |
| A8: Extract `useCaList.ts` from CorrectiveActionsView | Pagination, filters, load, KPI counts. | Create composable, shrink CorrectiveActionsView | Low |
| A9: Port `NewsletterView.vue` to AppLayout + Tailwind | Replace all raw CSS, route inside AppLayout, call `useAuditReportData`. | Modify NewsletterView.vue, router/index.ts | Medium |
| A10: Port `QuarterlySummaryView.vue` to AppLayout + Tailwind | Same as above. | Modify QuarterlySummaryView.vue, router/index.ts | Medium |
| A11: Fix `ReportComposerView.vue` empty state and inputs | Add EmptyState guidance, replace raw `<select>`/`<input>` with PrimeVue components. | Modify ReportComposerView.vue | Low |
| A12: Create `FilterBar.vue` shared component | Standardize filter row pattern used on 5+ pages. | Create component | Low |
| A13: Create `KpiCard.vue` shared component | Extract KPI card pattern used on 3+ pages. | Create component | Low |
| A14: Replace `adminStore.ts` with composables | Split into `useTemplateEditor.ts`, `useEmailRouting.ts`, `useAuditUserRoles.ts`. Delete adminStore. | Create 3 composables, update TemplateManagerView, AdminUsersView, AuditSettingsView | High |

**60-Day Goal**: ReportsView, AuditReviewView, and CorrectiveActionsView each below 300 lines. Newsletter and Quarterly Summary visually consistent with the rest of the app. Report data comes from one composable. Admin store eliminated.

---

## 90-DAY SPRINT — Features + Reporting + Automation (P2 Items)

New capabilities, unified reporting output, and automation completions.

### 90-Day Deliverables

| Item | Description | Files | Risk |
|---|---|---|---|
| B1: Add `ReportRunLog` model + migration | Delivery history for all scheduled and on-demand reports. | New model, migration, handler | Low |
| B2: Wire newsletter + quarterly-summary to scheduler | Server-side HTML templates + `GenerateReport` branches. | `Api/Domain/Audit/Reports/GenerateReport.cs`, `ScheduledReportService.cs` | Medium |
| B3: Add CA assignment notification email | When a CA is created/assigned, email the assignee. | `Api/Services/CaReminderService.cs` or new `CaCreatedHandler.cs` | Low |
| B4: Wire distribution email to audit submission (auto-send) | When an audit is submitted with routing rules configured, auto-send the distribution email instead of requiring manual admin trigger. | `Api/Domain/Audit/Audits/SubmitAudit.cs` | Medium |
| B5: Add CA escalation (Day 15 + Day 30) | `CaReminderService.cs` — add second and third escalation passes using `DivisionSla.EscalationEmail`. | `Api/Services/CaReminderService.cs` | Low |
| B6: Add `DivisionAuditOverdueService` | Daily background job: detect divisions past their `AuditFrequencyDays` window, notify division manager via routing rules. | New background service | Medium |
| B7: Add `StickyBottomActions.vue` for mobile | Mobile-only sticky action bar for AuditFormView Save/Submit. | Create component, update AuditFormView | Low |
| B8: Add `ScoreRing.vue` component | Extract inline SVG score ring from AuditReviewView. Reuse on post-submit summary modal. | Create component | Low |
| B9: Add `DivisionHealthCard.vue` component | Extract inline division health card from ReportsView. | Create component | Low |
| B10: Add `ErrorBanner.vue` component | Standardize error display (not just toast). | Create component, add to load-failure paths | Low |
| B11: Add auditor performance report type | New `auditor-performance` TemplateId in `GenerateReport` handler and `ReportGalleryView`. | Backend handler + gallery entry | Medium |

**90-Day Goal**: All automation events wired. Reporting system fully unified and schedulable. Mobile audit form field-ready. All shared UI primitives in place.

---

## PR SEQUENCE

### PR1 — Service Layer Foundation (30-Day Sprint)
### PR2 — View Decomposition (60-Day Sprint Part 1)
### PR3 — Design System + Reporting Unification (60-Day Sprint Part 2)
### PR4 — Mobile + UX Polish (90-Day Sprint Part 1)
### PR5 — Automation + Notifications (90-Day Sprint Part 2)

---

### PR1 — Service Layer Foundation

**Goal**: Eliminate all 28 scattered `new AuditClient()` instantiations, fix the confirmed AdminAuditLogView runtime error, add design system tokens, and add 2 shared primitives. Zero new features. Zero UI changes. All existing tests continue to pass.

**Exact files to CREATE**:

```
webapp/src/services/useAuditService.ts
webapp/src/composables/useCountUp.ts
webapp/src/components/feedback/EmptyState.vue
webapp/src/components/feedback/ErrorBanner.vue
```

**Exact files to MODIFY**:

```
webapp/tailwind.config.js                                                  ← Add color tokens
webapp/src/modules/audit-management/stores/auditStore.ts                   ← Replace getClient() factory with useAuditService()
webapp/src/modules/audit-management/stores/adminStore.ts                   ← Replace getClient() factory with useAuditService()
webapp/src/modules/audit-management/features/admin-audit-log/views/AdminAuditLogView.vue     ← Fix runtime error + replace getClient()
webapp/src/modules/audit-management/features/admin-settings/views/AuditSettingsView.vue      ← Replace getClient()
webapp/src/modules/audit-management/features/audit-dashboard/views/AuditDashboardView.vue    ← Replace getClient()
webapp/src/modules/audit-management/features/corrective-actions/views/CorrectiveActionsView.vue ← Replace getClient()
webapp/src/modules/audit-management/features/audit-review/views/AuditReviewView.vue          ← Replace getClient()
webapp/src/modules/audit-management/features/new-audit/views/NewAuditView.vue               ← Replace getClient()
webapp/src/modules/audit-management/features/template-manager/views/TemplateManagerView.vue  ← Replace getClient()
webapp/src/modules/audit-management/features/reports/views/ReportsView.vue                   ← Replace getClient()
webapp/src/modules/audit-management/features/reports/views/NewsletterView.vue               ← Replace getClient()
webapp/src/modules/audit-management/features/reports/views/QuarterlySummaryView.vue         ← Replace getClient()
webapp/src/modules/audit-management/features/reports/views/ReportComposerView.vue           ← Replace 6 getClient() calls
webapp/src/modules/audit-management/features/reports/views/ScheduledReportsView.vue         ← Replace getClient()
webapp/src/modules/audit-management/features/reports/composables/useReportEngine.ts         ← Replace 2 getClient() calls
webapp/src/modules/audit-management/features/reports/composables/useReportDraft.ts          ← Replace getClient()
webapp/src/modules/audit-management/features/reports/views/NewsletterTemplateEditorView.vue ← Replace 2 getClient() calls
webapp/src/modules/audit-management/features/audit-form/views/PrintableAuditFormView.vue   ← Replace getClient()
webapp/src/modules/audit-management/features/audit-review/views/PrintableAuditReviewView.vue ← Replace getClient()
webapp/src/modules/audit-management/features/audit-form/components/AuditAttachments.vue    ← Replace getClient()
webapp/src/modules/audit-management/features/audit-form/components/AuditQuestionRow.vue    ← Replace getClient()
```

**Exact files NOT to touch**:
- `Data/Migrations/` — no schema changes in PR1
- `Api/` — no backend changes in PR1
- `auditStore.ts` core logic (visibleSections, calculateScore, logic rules, draft save) — do NOT touch these
- `webapp/src/router/index.ts` — no route changes in PR1
- Any `.spec.ts` test files — no test changes in PR1

**The `useAuditService.ts` implementation**:

```typescript
// webapp/src/services/useAuditService.ts
import { AuditClient } from '@/apiclient/auditClient';
import { useApiStore } from '@/stores/apiStore';

/**
 * Returns a configured AuditClient instance using the authenticated Axios instance.
 * Use this instead of `new AuditClient(...)` directly in views and composables.
 *
 * Pattern: call once at the top of a setup() function.
 *   const service = useAuditService();
 *   const data = await service.getAuditList(...);
 */
export function useAuditService(): AuditClient {
    const apiStore = useApiStore();
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}
```

Note: the stores (`auditStore.ts`, `adminStore.ts`) have their own `getClient()` factory method internally — these are acceptable because stores are not components. Replace the inline factory in stores to call `useAuditService()` for consistency, but the store `getClient()` methods can remain as thin wrappers.

**Tests to add**:

1. Add a unit test for `useCountUp.ts` composable — verify count-up from 0 to N produces the correct final value.
2. Add an E2E test for `AdminAuditLogView.vue` — navigate to `/audit-management/admin/audit-log`, switch to Action tab, expand a row, assert no runtime error. Add to `audit-live-api-guard.spec.ts` or a new `admin-audit-log-contract.spec.ts`.

**Exact acceptance criteria**:

1. `webapp/tests/e2e/audit-live-api-guard.spec.ts` passes (all existing tests green).
2. `npm run test:e2e:audit:live-guard` passes.
3. `npm run test:e2e:audit:core` passes.
4. `npm run test:e2e:audit:corrective-actions` passes.
5. Zero occurrences of `new AuditClient(` remain in `webapp/src/` (verify with grep: `grep -r "new AuditClient(" webapp/src/` must return empty).
6. `AdminAuditLogView.vue` Action tab row expansion does not throw a runtime error (verified by E2E test or manual screenshot).
7. `tailwind.config.js` contains `theme.extend.colors.surface`, `theme.extend.colors.brand`, `theme.extend.colors.status`.

**What breaks if it goes wrong and how to detect it**:

- If a view calls `useAuditService()` outside a Vue composition context (e.g., in a `watch` callback that fires after the component is unmounted), Pinia will throw "No active Pinia instance." Detection: `npm run test:e2e:audit:live-guard` will fail with a console error.
- If any `getClient()` replacement misses an await or changes the error handling, API calls in that view will silently fail. Detection: `npm run test:e2e:audit:core` covers the main audit workflow — if this fails, a core API call was broken.
- The AdminAuditLogView fix (null guard) could mask a real data problem if applied too broadly. Detection: verify the fix with `if (!data.someField)` guards, not by removing the render of those fields entirely. The E2E test confirms the page loads and the row expands.

---

### PR2 — View Decomposition (First Half of 60-Day Sprint)

**Goal**: Extract composables from ReportsView.vue and AuditReviewView.vue. Both views should be under 300 lines after this PR.

**Files to create**: `useReportData.ts`, `useReportCharts.ts`, `useReportDrilldown.ts`, `useReportKpis.ts`, `useReviewDistribution.ts`, `useReviewEmail.ts`

**Files to modify**: `ReportsView.vue` (1,658 → ~250 lines), `AuditReviewView.vue` (1,151 → ~350 lines)

**Acceptance criteria**: Both views render identically to before. All existing E2E tests pass. Line counts verified by CI step.

**Blast radius**: Medium. ReportsView is the highest-traffic view. Any broken computed or missing `watch` will immediately fail the live-guard tests.

---

### PR3 — Design System + Reporting Unification

**Goal**: Port NewsletterView and QuarterlySummaryView to AppLayout + Tailwind. Fix ReportComposerView empty state and inputs. Create FilterBar and KpiCard primitives.

**Files to create**: `FilterBar.vue`, `KpiCard.vue`, `ScoreRing.vue`

**Files to modify**: `NewsletterView.vue`, `QuarterlySummaryView.vue`, `ReportComposerView.vue`, `router/index.ts`

**Acceptance criteria**: Newsletter and Quarterly Summary are visually consistent with the rest of the app. Print output is preserved. Report Composer has an EmptyState and uses PrimeVue inputs.

**Blast radius**: Low for the composable work. Medium for the router change (routing newsletter + quarterly inside AppLayout). Must verify the print CSS works after the AppLayout wrapper is added.

---

### PR4 — Mobile + UX Polish

**Goal**: Mobile sticky action bar on AuditFormView, touch target fixes on AuditQuestionRow, shared EmptyState and ErrorBanner deployed to all pages.

**Files to create**: `StickyBottomActions.vue`

**Files to modify**: `AuditFormView.vue`, `AuditQuestionRow.vue`, all views with inconsistent empty states

**Acceptance criteria**: On 375px viewport, Save Draft and Submit buttons are visible without scrolling. All pages use EmptyState component. Visual sweep passes at mobile viewport.

**Blast radius**: Low. UI-only changes. E2E tests cover the submit flow.

---

### PR5 — Automation + Notifications

**Goal**: CA assignment notification, auto-send distribution email on submission, CA escalation at Day 15 and Day 30, DivisionAuditOverdueService.

**Files to create**: `DivisionAuditOverdueService.cs`, `ReportRunLog.cs`, new migration

**Files to modify**: `SubmitAudit.cs`, `CaReminderService.cs`, `ScheduledReportService.cs`

**Acceptance criteria**: CA assignment creates a notification email. Division overdue alert fires on the daily job. Escalation emails fire at Day 15 and Day 30 (verified in dry-run mode via log output). ScheduledReportService writes to ReportRunLog on each run.

**Blast radius**: Medium for email changes. EmailService has dry-run mode that prevents accidental sends. All notification changes must be verified in dry-run before any production deploy.
