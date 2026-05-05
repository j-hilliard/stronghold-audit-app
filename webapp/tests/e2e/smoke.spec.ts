import { test, expect } from './fixtures';
import { expectNoRuntimeErrors, gotoAndStabilize } from './utils/ui';

const DEV_ROLE_KEY = 'stronghold-audit-dev-role';

test.describe('Smoke gate', () => {
    test('audit management shell loads and top-level navigation is healthy', async ({ page, runtimeErrors }) => {
        await page.addInitScript(({ key, value }) => {
            localStorage.setItem(key, value);
        }, { key: DEV_ROLE_KEY, value: 'AuditAdmin' });

        // Catch-all registered first (lowest priority in Playwright — last-registered wins)
        await page.route(/\/v1\//i, (route) => {
            if (route.request().method().toUpperCase() === 'GET') {
                return route.fulfill({
                    status: 200,
                    headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
                    body: JSON.stringify({ items: [], totalCount: 0 }),
                });
            }
            return route.continue();
        });
        // Specific routes registered last (highest priority)
        // /v1/audits returns a plain array (not paged) — getAuditList does r.data directly
        await page.route(/\/v1\/audits(\?.*)?$/i, (route) => {
            if (route.request().method().toUpperCase() === 'GET') {
                return route.fulfill({
                    status: 200,
                    headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
                    body: JSON.stringify([]),
                });
            }
            return route.continue();
        });
        await page.route(/\/v1\/divisions(\?.*)?$/i, (route) =>
            route.fulfill({
                status: 200,
                headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
                body: JSON.stringify([{ id: 1, code: 'TKIE', name: 'TK Industrial East', auditType: 'JobSite' }]),
            })
        );

        await gotoAndStabilize(page, '/audit-management/audits');

        // Sidebar navigation must be present
        await expect(page.locator('.layout-menu-container')).toBeVisible({ timeout: 10000 });

        // Core nav items for AuditAdmin
        await expect(page.locator('.layout-menuitem-text', { hasText: 'Audits' }).first()).toBeVisible();
        await expect(page.locator('.layout-menuitem-text', { hasText: 'Corrective Actions' }).first()).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('corrective actions page loads without blocking errors', async ({ page, runtimeErrors }) => {
        await page.addInitScript(({ key, value }) => {
            localStorage.setItem(key, value);
        }, { key: DEV_ROLE_KEY, value: 'AuditAdmin' });

        await page.route(/\/v1\/audits\/corrective-actions(\?.*)?$/i, (route) =>
            route.fulfill({
                status: 200,
                headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
                body: JSON.stringify({
                    items: [], totalCount: 0, pageNumber: 1, pageSize: 25, totalPages: 0,
                    openCount: 0, inProgressCount: 0, overdueCount: 0, closedCount: 0,
                }),
            })
        );
        await page.route(/\/v1\/divisions(\?.*)?$/i, (route) =>
            route.fulfill({
                status: 200,
                headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
                body: JSON.stringify([{ id: 1, code: 'TKIE', name: 'TK Industrial East', auditType: 'JobSite' }]),
            })
        );
        await page.route(/\/v1\//i, (route) => {
            if (route.request().method().toUpperCase() === 'GET') {
                return route.fulfill({
                    status: 200,
                    headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
                    body: JSON.stringify([]),
                });
            }
            return route.continue();
        });

        await gotoAndStabilize(page, '/audit-management/corrective-actions');

        // Page header visible
        await expect(page.locator('h1, h2').filter({ hasText: /corrective action/i }).first()).toBeVisible({ timeout: 10000 });

        expectNoRuntimeErrors(runtimeErrors);
    });
});
