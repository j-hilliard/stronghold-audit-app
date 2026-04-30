/**
 * Admin User Management — Full Contract Suite
 *
 * Covers: AdminUsersView.vue — user list, role assignment, role removal,
 * enable/disable, search, filter by status/role.
 *
 * API calls are intercepted in-memory; no live backend required.
 */

import { Page, Route } from '@playwright/test';
import { test, expect } from './fixtures';
import { expectNoRuntimeErrors, gotoAndStabilize } from './utils/ui';

function replyJson(route: Route, payload: unknown, status = 200) {
    return route.fulfill({
        status,
        headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
        body: JSON.stringify(payload),
    });
}
function replyEmpty(route: Route) {
    return route.fulfill({ status: 204, headers: { 'access-control-allow-origin': '*' } });
}

// ── Seed data ─────────────────────────────────────────────────────────────────

const MOCK_ROLES = [
    { roleId: 1, name: 'Administrator',   description: 'System administrator' },
    { roleId: 2, name: 'AuditManager',    description: 'Manages audits' },
    { roleId: 3, name: 'AuditReviewer',   description: 'Reviews audits' },
    { roleId: 4, name: 'ExecutiveViewer', description: 'Read-only executive view' },
];

const MOCK_USERS = [
    {
        userId: 1,
        azureAdObjectId: 'aaa-111',
        firstName: 'Alice',
        lastName: 'Admin',
        email: 'alice@stronghold.com',
        company: 'Stronghold',
        department: 'IT',
        title: 'System Administrator',
        active: true,
        lastLogin: '2026-04-15T10:00:00Z',
        roles: [{ userId: 1, roleId: 1, role: { roleId: 1, name: 'Administrator', description: '' } }],
    },
    {
        userId: 2,
        azureAdObjectId: 'bbb-222',
        firstName: 'Bob',
        lastName: 'Auditor',
        email: 'bob@stronghold.com',
        company: 'Stronghold',
        department: 'Safety',
        title: 'Safety Auditor',
        active: true,
        lastLogin: '2026-04-16T08:30:00Z',
        roles: [{ userId: 2, roleId: 2, role: { roleId: 2, name: 'AuditManager', description: '' } }],
    },
    {
        userId: 3,
        azureAdObjectId: 'ccc-333',
        firstName: 'Carol',
        lastName: 'Disabled',
        email: 'carol@stronghold.com',
        company: 'Stronghold',
        department: 'Operations',
        title: 'Operations Manager',
        active: false,
        lastLogin: null,
        roles: [],
    },
];

type MockUser = {
    userId: number;
    azureAdObjectId: string;
    firstName: string;
    lastName: string;
    email: string;
    company: string;
    department: string;
    title: string;
    active: boolean;
    lastLogin: string | null;
    roles: Array<{ userId: number; roleId: number; role: { roleId: number; name: string; description: string } }>;
};

type MockState = {
    users: MockUser[];
    addRoleRequests: unknown[];
    removeRoleRequests: unknown[];
    toggleRequests: { userId: number; action: 'activate' | 'disable' }[];
};

async function installAdminUsersMocks(page: Page): Promise<MockState> {
    const state: MockState = {
        users:              MOCK_USERS.map(u => ({ ...u, roles: [...u.roles] })) as MockUser[],
        addRoleRequests:    [],
        removeRoleRequests: [],
        toggleRequests:     [],
    };

    // GET all roles
    await page.route(/\/v1\/Role(\?.*)?$/i, (route) => {
        if (route.request().method() === 'GET') return replyJson(route, MOCK_ROLES);
        return replyJson(route, {});
    });

    // GET all users
    await page.route(/\/v1\/User\/AllUsers(\?.*)?$/i, (route) =>
        replyJson(route, state.users),
    );

    // PUT ActivateUser
    await page.route(/\/v1\/User\/ActivateUser\/(\d+)$/i, async (route) => {
        const url = route.request().url();
        const id  = parseInt(url.match(/\/ActivateUser\/(\d+)/)![1]);
        state.toggleRequests.push({ userId: id, action: 'activate' });
        const u = state.users.find(u => u.userId === id);
        if (u) u.active = true;
        return replyJson(route, true, 202);
    });

    // PUT DisableUser
    await page.route(/\/v1\/User\/DisableUser\/(\d+)$/i, async (route) => {
        const url = route.request().url();
        const id  = parseInt(url.match(/\/DisableUser\/(\d+)/)![1]);
        state.toggleRequests.push({ userId: id, action: 'disable' });
        const u = state.users.find(u => u.userId === id);
        if (u) u.active = false;
        return replyJson(route, true, 202);
    });

    // POST UserRole (add role)
    await page.route(/\/v1\/UserRole(\?.*)?$/i, async (route) => {
        const method = route.request().method().toUpperCase();
        if (method === 'POST') {
            const body = JSON.parse(route.request().postData() ?? '{}');
            state.addRoleRequests.push(body);
            return replyJson(route, body, 201);
        }
        return replyJson(route, {});
    });

    // DELETE UserRole/{userId}/{roleId}
    await page.route(/\/v1\/UserRole\/\d+\/\d+$/i, async (route) => {
        if (route.request().method().toUpperCase() === 'DELETE') {
            const url   = route.request().url();
            const parts = url.match(/\/UserRole\/(\d+)\/(\d+)$/);
            state.removeRoleRequests.push({ userId: parseInt(parts![1]), roleId: parseInt(parts![2]) });
            return replyJson(route, {}, 202);
        }
        return replyJson(route, {});
    });

    return state;
}

const ADMIN_USERS_URL = 'http://localhost:7308/audit-management/admin/users';

// ── Tests ─────────────────────────────────────────────────────────────────────

test.describe('Admin Users — page render', () => {
    test('page loads with user table and stat cards', async ({ page, runtimeErrors }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        // Stat cards visible
        await expect(page.locator('.bg-slate-800').first()).toBeVisible();

        // All 3 mock users should appear — wait for first one to confirm table rendered
        await expect(page.getByText('Alice Admin')).toBeVisible({ timeout: 8000 });
        await expect(page.getByText('Bob Auditor')).toBeVisible();
        await expect(page.getByText('Carol Disabled')).toBeVisible();

        await expectNoRuntimeErrors(runtimeErrors);
    });

    test('stat cards show correct counts', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        // Total: 3, Active: 2, Disabled: 1
        const cardTexts = await page.locator('.bg-slate-800').allTextContents();
        const allText   = cardTexts.join(' ');
        expect(allText).toMatch(/3/);  // total
        expect(allText).toMatch(/2/);  // active
    });

    test('role chips are shown for each user', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        await expect(page.getByText('Administrator').first()).toBeVisible();
        await expect(page.getByText('AuditManager').first()).toBeVisible();
    });
});

test.describe('Admin Users — search and filter', () => {
    test('search by name filters the table', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        const searchInput = page.getByPlaceholder(/search by name/i);
        await searchInput.fill('Alice');
        await page.waitForTimeout(300);

        await expect(page.getByText('Alice Admin')).toBeVisible();
        await expect(page.getByText('Bob Auditor')).not.toBeVisible();
    });

    test('filter by Disabled Only hides active users', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        // Find and select the status filter dropdown
        const dropdown = page.locator('.p-dropdown').nth(0);
        await dropdown.click();
        await page.getByRole('option', { name: 'Disabled Only' }).click();
        await page.waitForTimeout(200);

        await expect(page.getByText('Carol Disabled')).toBeVisible();
        await expect(page.getByText('Alice Admin')).not.toBeVisible();
        await expect(page.getByText('Bob Auditor')).not.toBeVisible();
    });
});

test.describe('Admin Users — role management', () => {
    test('Add role button opens assign dialog', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        // Click the "Add" chip button on Bob's row
        const bobRow = page.locator('tr', { has: page.getByText('Bob Auditor') });
        const addBtn = bobRow.getByText('Add');
        await addBtn.click();

        await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 5000 });
        await expect(page.locator('.p-dialog-title')).toHaveText('Assign Role');
        await expect(page.locator('.p-dialog').getByText('Bob Auditor')).toBeVisible();
    });

    test('Assign Role dialog Cancel closes it', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        const bobRow = page.locator('tr', { has: page.getByText('Bob Auditor') });
        await bobRow.getByText('Add').click();
        await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 4000 });

        await page.getByRole('button', { name: 'Cancel' }).first().click();
        await expect(page.locator('.p-dialog')).not.toBeVisible({ timeout: 3000 });
    });

    test('Assign Role button disabled until role selected', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        const bobRow = page.locator('tr', { has: page.getByText('Bob Auditor') });
        await bobRow.getByText('Add').click();
        await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 4000 });

        const assignBtn = page.getByRole('button', { name: 'Assign Role' });
        await expect(assignBtn).toBeDisabled();
    });

    test('Remove role (×) opens confirm dialog', async ({ page, runtimeErrors }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        // Alice has Administrator role — click × on it
        const aliceRow = page.locator('tr', { has: page.getByText('Alice Admin') });
        const removeBtn = aliceRow.locator('.pi-times').first();
        await removeBtn.click();

        await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 5000 });
        await expect(page.getByText('Remove Role')).toBeVisible();
        await expect(page.locator('.p-dialog').getByText('Administrator', { exact: true })).toBeVisible();

        await expectNoRuntimeErrors(runtimeErrors);
    });

    test('Remove role confirm dialog Cancel closes it', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        const aliceRow = page.locator('tr', { has: page.getByText('Alice Admin') });
        await aliceRow.locator('.pi-times').first().click();
        await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 4000 });

        await page.getByRole('button', { name: 'Cancel' }).click();
        await expect(page.locator('.p-dialog')).not.toBeVisible({ timeout: 3000 });
    });

    test('Remove role confirm dialog sends DELETE request', async ({ page }) => {
        const state = await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        const aliceRow = page.locator('tr', { has: page.getByText('Alice Admin') });
        await aliceRow.locator('.pi-times').first().click();
        await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 4000 });

        await page.getByRole('button', { name: 'Remove' }).click();
        await page.waitForTimeout(800);

        expect(state.removeRoleRequests).toHaveLength(1);
        expect((state.removeRoleRequests[0] as any).userId).toBe(1);
    });
});

test.describe('Admin Users — enable/disable', () => {
    test('disable button visible on active user row (hover)', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        // Hover Alice's row to reveal the action button
        const aliceRow = page.locator('tr', { has: page.getByText('Alice Admin') });
        await aliceRow.hover();

        // Disable button (ban icon) should appear
        const disableBtn = aliceRow.locator('[title="Disable user"]');
        await expect(disableBtn).toBeVisible({ timeout: 3000 });
    });

    test('enable button visible on disabled user row (hover)', async ({ page }) => {
        await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        const carolRow = page.locator('tr', { has: page.getByText('Carol Disabled') });
        await carolRow.hover();

        const enableBtn = carolRow.locator('[title="Enable user"]');
        await expect(enableBtn).toBeVisible({ timeout: 3000 });
    });

    test('clicking enable fires ActivateUser API', async ({ page }) => {
        const state = await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        const carolRow = page.locator('tr', { has: page.getByText('Carol Disabled') });
        await carolRow.hover();
        await carolRow.locator('[title="Enable user"]').click();
        await page.waitForTimeout(800);

        expect(state.toggleRequests).toHaveLength(1);
        expect(state.toggleRequests[0]).toMatchObject({ userId: 3, action: 'activate' });
    });

    test('clicking disable fires DisableUser API', async ({ page }) => {
        const state = await installAdminUsersMocks(page);
        await gotoAndStabilize(page, ADMIN_USERS_URL);

        const aliceRow = page.locator('tr', { has: page.getByText('Alice Admin') });
        await aliceRow.hover();
        await aliceRow.locator('[title="Disable user"]').click();
        await page.waitForTimeout(800);

        expect(state.toggleRequests).toHaveLength(1);
        expect(state.toggleRequests[0]).toMatchObject({ userId: 1, action: 'disable' });
    });
});
