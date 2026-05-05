/**
 * External CA Public Token Flow — E2E Contract Suite
 *
 * Covers the /ca/:token route (ExternalCaView.vue) — the public-facing page
 * that lets external assignees (contractors, non-app users) update a corrective
 * action without logging in.
 *
 * Scenarios:
 *   1. Valid token loads the CA detail page
 *   2. Invalid / expired token shows the access error state
 *   3. Open / Overdue CA shows "Confirm Work In Progress" and can move to InProgress
 *   4. InProgress CA shows "Submit Completed Work" section
 *   5. Cannot submit without attaching a proof photo
 *   6. With a photo attached, submit succeeds and CA transitions to Submitted
 *   7. Closed CA shows no actionable submit flow
 *   8. Voided CA shows no actionable submit flow
 *
 * All API calls are intercepted in-memory.  PublicCaClient uses /v1/public/ca/:token.
 * The route is /ca/:token — outside the /audit-management/* prefix, so no auth mocks needed.
 */

import { Page, Route } from '@playwright/test';
import { test, expect } from './fixtures';
import { expectNoRuntimeErrors, gotoAndStabilize } from './utils/ui';

// ── Helpers ──────────────────────────────────────────────────────────────────

const VALID_TOKEN   = 'abc123def456abc123def456abc123def456abc123def456abc123def456abc1';
const EXPIRED_TOKEN = 'expired000000000000000000000000000000000000000000000000000000000';

function replyJson(route: Route, payload: unknown, status = 200) {
    return route.fulfill({
        status,
        headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
        body: JSON.stringify(payload),
    });
}

function replyEmpty(route: Route, status = 204) {
    return route.fulfill({ status, headers: { 'access-control-allow-origin': '*' } });
}

function buildCaDto(overrides: Record<string, unknown> = {}) {
    return {
        id: 1,
        description: 'Install guardrail on Level 2 before next shift.',
        rootCause: null,
        status: 'Open',
        priority: 'Normal',
        dueDate: '2026-05-15',
        assignedTo: 'John Smith',
        auditTitle: 'TKIE Compliance Audit — April 2026',
        expiresAt: '2026-08-01T00:00:00Z',
        ...overrides,
    };
}

async function installPublicMocks(page: Page, token: string, caOverrides: Record<string, unknown> = {}) {
    // POST /v1/public/ca/:token/photos  (photo upload) — registered first (lowest priority)
    await page.route(new RegExp(`/v1/public/ca/${token}/photos(\\?.*)?$`, 'i'), (route) =>
        replyJson(route, {
            id: 99,
            fileName: 'closure.jpg',
            filePath: '/uploads/closure.jpg',
            fileSizeBytes: 102400,
            uploadedAt: new Date().toISOString(),
        }, 201)
    );

    // GET + PUT /v1/public/ca/:token — registered last (highest priority)
    await page.route(new RegExp(`/v1/public/ca/${token}(\\?.*)?$`, 'i'), (route) => {
        const method = route.request().method().toUpperCase();
        if (method === 'GET') {
            return replyJson(route, buildCaDto(caOverrides));
        }
        if (method === 'PUT') {
            return replyEmpty(route);
        }
        return replyJson(route, buildCaDto(caOverrides));
    });
}

async function installExpiredTokenMock(page: Page, token: string) {
    await page.route(new RegExp(`/v1/public/ca/${token}(\\?.*)?$`, 'i'), (route) =>
        route.fulfill({
            status: 404,
            headers: { 'access-control-allow-origin': '*', 'content-type': 'application/json' },
            body: JSON.stringify({ detail: 'This link has expired or is no longer valid.' }),
        })
    );
}

// ─────────────────────────────────────────────────────────────────────────────
// 1. Valid token — page loads
// ─────────────────────────────────────────────────────────────────────────────

test.describe('External CA — valid token loads', () => {
    test('page renders CA description and audit title', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN);
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('h1', { hasText: 'Corrective Action Update' })).toBeVisible({ timeout: 10000 });
        await expect(page.locator('text=Install guardrail on Level 2')).toBeVisible();
        await expect(page.locator('text=TKIE Compliance Audit')).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('page shows status, priority, and due date info', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN);
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Open')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('text=Normal')).toBeVisible();
        await expect(page.locator('text=2026-05-15')).toBeVisible();
        await expect(page.locator('text=John Smith')).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 2. Invalid / expired token — shows error state
// ─────────────────────────────────────────────────────────────────────────────

test.describe('External CA — invalid or expired token', () => {
    test('shows Access Unavailable error for invalid token', async ({ page, runtimeErrors }) => {
        await installExpiredTokenMock(page, EXPIRED_TOKEN);
        await gotoAndStabilize(page, `/ca/${EXPIRED_TOKEN}`);

        await expect(page.locator('text=Access Unavailable')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('text=expired or is no longer valid')).toBeVisible();

        // No actionable submit form should appear
        await expect(page.locator('button', { hasText: 'Mark as In Progress' })).not.toBeVisible();
        await expect(page.locator('button', { hasText: 'Submit Work Complete' })).not.toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 3. Open / Overdue CA — can mark as InProgress
// ─────────────────────────────────────────────────────────────────────────────

test.describe('External CA — Open / Overdue status flow', () => {
    test('Open CA shows Confirm Work In Progress section', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'Open' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Confirm Work In Progress')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('button', { hasText: 'Mark as In Progress' })).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('Overdue CA shows Confirm Work In Progress section', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'Overdue' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Confirm Work In Progress')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('button', { hasText: 'Mark as In Progress' })).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('clicking Mark as In Progress shows success message', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'Open' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('button', { hasText: 'Mark as In Progress' })).toBeVisible({ timeout: 10000 });
        await page.locator('button', { hasText: 'Mark as In Progress' }).click();

        // Success message appears
        await expect(page.locator('text=Marked as In Progress')).toBeVisible({ timeout: 8000 });

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 4. InProgress CA — Submit Completed Work section
// ─────────────────────────────────────────────────────────────────────────────

test.describe('External CA — InProgress status: submit work', () => {
    test('InProgress CA shows Submit Completed Work section', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'InProgress' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Submit Completed Work')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('button', { hasText: 'Submit Work Complete' })).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('Submit Work Complete button is disabled without a photo', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'InProgress' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Submit Completed Work')).toBeVisible({ timeout: 10000 });

        const submitBtn = page.locator('button', { hasText: 'Submit Work Complete' });
        await expect(submitBtn).toBeVisible();
        await expect(submitBtn).toBeDisabled();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('warning message visible when no photo attached', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'InProgress' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Submit Completed Work')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('text=proof photo is required')).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('attaching a photo enables Submit Work Complete button', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'InProgress' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Submit Completed Work')).toBeVisible({ timeout: 10000 });

        // Upload a photo via the hidden file input
        const fileInput = page.locator('input[type="file"]');
        await fileInput.setInputFiles({
            name: 'proof.jpg',
            mimeType: 'image/jpeg',
            buffer: Buffer.from('fake-image-data'),
        });

        // After photo is selected, submit button should be enabled
        const submitBtn = page.locator('button', { hasText: 'Submit Work Complete' });
        await expect(submitBtn).toBeEnabled({ timeout: 5000 });

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('submitting with photo shows success message (Submitted state)', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'InProgress' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Submit Completed Work')).toBeVisible({ timeout: 10000 });

        const fileInput = page.locator('input[type="file"]');
        await fileInput.setInputFiles({
            name: 'proof.jpg',
            mimeType: 'image/jpeg',
            buffer: Buffer.from('fake-image-data'),
        });

        await expect(page.locator('button', { hasText: 'Submit Work Complete' })).toBeEnabled({ timeout: 5000 });
        await page.locator('button', { hasText: 'Submit Work Complete' }).click();

        // Success message appears
        await expect(page.locator('text=Work submitted successfully')).toBeVisible({ timeout: 8000 });

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 5. Submitted CA — pending review, no submit actions
// ─────────────────────────────────────────────────────────────────────────────

test.describe('External CA — Submitted status: pending admin review', () => {
    test('Submitted CA shows pending review message with no submit action', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'Submitted' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=Work submitted')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('text=pending admin review')).toBeVisible();

        // No submit or in-progress buttons
        await expect(page.locator('button', { hasText: 'Submit Work Complete' })).not.toBeVisible();
        await expect(page.locator('button', { hasText: 'Mark as In Progress' })).not.toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 6. Closed / Voided CA — no actionable submit flow
// ─────────────────────────────────────────────────────────────────────────────

test.describe('External CA — Closed / Voided: no actionable submit', () => {
    test('Closed CA shows closed message and no submit actions', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'Closed' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=has been closed')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('text=No further action is required')).toBeVisible();

        await expect(page.locator('button', { hasText: 'Submit Work Complete' })).not.toBeVisible();
        await expect(page.locator('button', { hasText: 'Mark as In Progress' })).not.toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('Voided CA shows voided message and no submit actions', async ({ page, runtimeErrors }) => {
        await installPublicMocks(page, VALID_TOKEN, { status: 'Voided' });
        await gotoAndStabilize(page, `/ca/${VALID_TOKEN}`);

        await expect(page.locator('text=has been voided')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('text=No further action is required')).toBeVisible();

        await expect(page.locator('button', { hasText: 'Submit Work Complete' })).not.toBeVisible();
        await expect(page.locator('button', { hasText: 'Mark as In Progress' })).not.toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });
});
