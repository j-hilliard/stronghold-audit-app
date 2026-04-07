# QA Defect Handoff - 2026-04-01

## Scope
- DEF-0002: Sidebar navigation blank page regression.
- DEF-0003: Missing template-admin controls.
- DEF-0004: Missing KPI/reporting controls.
- DEF-0005: Template selector label mapping mismatch (`undefined` in UI).
- DEF-0007: Audit API availability/proxy failures on `/v1/divisions` and `/v1/audits`.

## DEF-0002

### Problem
- Observed: after starting an audit, repeated sidebar route changes can land on a blank content panel until refresh.
- Expected: each route change should render page content without refresh.

### Reproduction
1. Run `npm run test:e2e:audit:core`.
2. Review failure in `tests/e2e/audit-navigation-stability.spec.ts`.
3. See screenshot artifact where breadcrumb updates but page body is blank.

### Suggested Solution
- Verify route-view transition behavior and async component mount lifecycle in audit module routes.
- Add deterministic render guard in reports/new-audit/audits views so heading/content mount is confirmed after navigation.
- Add unit/integration coverage for repeated route hops from in-progress audit context.

---

## DEF-0003

### Problem
- Observed: Template Manager is placeholder-only.
- Expected: clone version, add question, drag/drop reorder, publish flow.

### Reproduction
1. Set `PW_AUDIT_TEMPLATE_GATE=true`.
2. Run `npm run test:e2e:audit:template-gate`.
3. Test fails waiting for `Create New Version` button.

### Suggested Solution
- Implement controls and API wiring required by:
  - `tests/e2e/audit-template-admin-contract.spec.ts`
  - `docs/requirements/audit-template-engine-requirements.md`
- Add required `data-testid` hooks in UI.

---

## DEF-0004

### Problem
- Observed: Reports page is placeholder-only.
- Expected: KPI cards, filters, scoped report rows, export action.

### Reproduction
1. Set `PW_AUDIT_REPORTING_GATE=true`.
2. Run `npm run test:e2e:audit:reporting-gate`.
3. Test fails waiting for report filter selector.

### Suggested Solution
- Implement report filter panel, KPI cards, grid, and export action.
- Wire report API endpoints with server-side scope filtering.
- Add required `data-testid` hooks in UI and ensure row-level restrictions are enforced in API output.

---

## DEF-0005

### Problem
- Observed: template selector row displays `undefined` label in Template Manager.
- Expected: meaningful template/version labels from API payload.

### Reproduction
1. Set `PW_AUDIT_TEMPLATE_GATE=true`.
2. Run `npm run test:e2e:audit:template-gate`.
3. Review screenshot artifact showing `undefined` in the template row.

### Suggested Solution
- Align UI mapping fields with API contract keys (`templateName`, version label fields, status field).
- Add fallback text only for true null/empty states, not for contract mismatch.

---

## DEF-0007

### Problem
- Observed: audit bootstrap endpoints intermittently fail with `500` or `ECONNREFUSED`, causing `Failed to load divisions/audits` toasts and blank pages.
- Expected: `GET /v1/divisions` and `GET /v1/audits` are reachable and return `200` JSON.

### Reproduction
1. Run `npm run qa:baseline`.
2. Observe failures in:
   - `tests/e2e/audit-live-api-guard.spec.ts`
   - `tests/e2e/audit-live-blank-screen-guard.spec.ts`
3. Confirm Vite proxy logs show `ECONNREFUSED` for `/v1/divisions` and `/v1/audits`.

### Suggested Solution
- Ensure ASP.NET API is running and bound on `https://localhost:7221` before frontend start.
- Eliminate port conflicts where non-API process occupies 7221.
- Add startup preflight check in dev script to verify API endpoint health before allowing frontend launch.
