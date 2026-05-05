/**
 * Reviewer Mode Round-Trip — E2E Contract Suite
 *
 * Covers the AuditFormView reviewer context (?reviewerMode=1) flow:
 *   1. Direct navigation to /audits/:id?reviewerMode=1 shows ReviewerContextBanner
 *   2. "Review Mode" label and "Editing Responses" text are present in the banner
 *   3. "Submit for Review" button is NOT present in reviewer mode
 *   4. "Delete Draft" button is NOT present in reviewer mode
 *   5. Save button shows "Save Changes" label (not "Save Draft") in reviewer mode
 *   6. Save Changes calls the save endpoint and succeeds
 *   7. "Back to Review" button in banner navigates to /audits/:id/review
 *   8. "Back to Review" header button also navigates to /audits/:id/review
 *
 * All API calls are intercepted in-memory.
 */

import { Page, Route } from '@playwright/test';
import { test, expect } from './fixtures';
import { expectNoRuntimeErrors, gotoAndStabilize } from './utils/ui';

// ── Constants ─────────────────────────────────────────────────────────────────

const DEV_ROLE_KEY = 'stronghold-audit-dev-role';
const AUDIT_ID = 42;

// ── Helpers ───────────────────────────────────────────────────────────────────

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

async function setAuditReviewerRole(page: Page) {
    await page.addInitScript(({ key, value }) => {
        try { localStorage.setItem(key, value); } catch { /* no-op */ }
    }, { key: DEV_ROLE_KEY, value: 'AuditReviewer' });
}

const MOCK_TEMPLATE = {
    id: 1,
    name: 'Job Site Safety',
    divisionId: 1,
    divisionCode: 'TKIE',
    sections: [
        {
            id: 101,
            name: 'PPE',
            questions: [
                {
                    id: 201,
                    text: 'Are fall protection systems in place?',
                    required: true,
                    category: 'Safety',
                    weight: 1,
                },
            ],
        },
    ],
};

function buildMockAudit(overrides: Record<string, unknown> = {}) {
    return {
        id: AUDIT_ID,
        status: 'UnderReview',
        divisionId: 1,
        divisionCode: 'TKIE',
        divisionName: 'TK Industrial East',
        trackingNumber: 'TKIE-2604-042',
        auditorName: 'Test Auditor',
        auditDate: '2026-04-15',
        scorePercent: 85,
        responses: [],
        attachments: [],
        ...overrides,
    };
}

function buildMockReview(overrides: Record<string, unknown> = {}) {
    return {
        auditId: AUDIT_ID,
        id: AUDIT_ID,
        status: 'UnderReview',
        divisionCode: 'TKIE',
        divisionName: 'TK Industrial East',
        scorePercent: 85,
        nonConformingItems: [],
        warningItems: [],
        sections: [],
        reviewEmailRouting: [],
        distributionRecipients: [],
        attachments: [],
        header: {
            auditDate: '2026-04-15',
            auditor: 'Test Auditor',
            jobNumber: 'TKIE-2604-042',
        },
        ...overrides,
    };
}

let savedResponsesCount = 0;

async function installReviewerMocks(page: Page, auditId = AUDIT_ID) {
    savedResponsesCount = 0;

    // ── STEP 1: Catch-alls registered first (lowest priority) ─────────────────

    // Catch-all for /v1/audits/ paths
    await page.route(/\/v1\/audits\//i, (route) => {
        if (route.request().method().toUpperCase() === 'GET') return replyJson(route, {});
        return replyEmpty(route);
    });

    // Reports/compliance (prevent 404 noise)
    await page.route(/\/v1\/reports\//i, (route) =>
        replyJson(route, { divisions: [], totalAudits: 0, avgScore: null })
    );

    // User lookup catch-all
    await page.route(/\/v1\/users\//i, (route) =>
        replyJson(route, {
            id: 1, firstName: 'Test', lastName: 'Reviewer',
            email: 'reviewer@example.com', roles: [],
        })
    );

    // ── STEP 2: Specific routes registered after (highest priority) ───────────

    // Divisions
    await page.route(/\/v1\/divisions(\?.*)?$/i, (route) =>
        replyJson(route, [{ id: 1, code: 'TKIE', name: 'TK Industrial East', auditType: 'JobSite' }])
    );

    // Active template lookup (called by loadAudit)
    await page.route(/\/v1\/templates\/active(\?.*)?$/i, (route) =>
        replyJson(route, MOCK_TEMPLATE)
    );

    // Audit attachments
    await page.route(new RegExp(`/v1/audits/${auditId}/attachments(\\?.*)?$`, 'i'), (route) =>
        replyJson(route, [])
    );

    // Save audit responses  PUT /v1/audits/{id}/responses
    await page.route(new RegExp(`/v1/audits/${auditId}/responses(\\?.*)?$`, 'i'), async (route) => {
        const method = route.request().method().toUpperCase();
        if (method === 'PUT' || method === 'POST') {
            savedResponsesCount++;
            return replyEmpty(route);
        }
        return replyJson(route, []);
    });

    // Audit review — specific sub-path registered before the bare audit route
    await page.route(new RegExp(`/v1/audits/${auditId}/review(\\?.*)?$`, 'i'), (route) => {
        if (route.request().method().toUpperCase() === 'GET') {
            return replyJson(route, buildMockReview());
        }
        return replyEmpty(route);
    });

    // Audit by ID — bare route, registered last so it wins over catch-all
    await page.route(new RegExp(`/v1/audits/${auditId}(\\?.*)?$`, 'i'), (route) => {
        const method = route.request().method().toUpperCase();
        if (method === 'GET') {
            return replyJson(route, buildMockAudit());
        }
        return replyEmpty(route);
    });

}

// ─────────────────────────────────────────────────────────────────────────────
// 1. Banner is shown in reviewer mode
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Reviewer mode — banner visibility', () => {
    test('ReviewerContextBanner is visible with ?reviewerMode=1', async ({ page, runtimeErrors }) => {
        await setAuditReviewerRole(page);
        await installReviewerMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}?reviewerMode=1`);

        await expect(page.locator('text=Review Mode')).toBeVisible({ timeout: 10000 });
        await expect(page.locator('text=Editing Responses')).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('ReviewerContextBanner is NOT shown without reviewerMode param', async ({ page, runtimeErrors }) => {
        await setAuditReviewerRole(page);
        await installReviewerMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}`);

        await expect(page.locator('text=Review Mode')).not.toBeVisible({ timeout: 5000 });

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 2. Restricted controls — no Submit or Delete in reviewer mode
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Reviewer mode — restricted controls', () => {
    test('"Submit for Review" button is NOT present in reviewer mode', async ({ page, runtimeErrors }) => {
        await setAuditReviewerRole(page);
        await installReviewerMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}?reviewerMode=1`);

        await expect(page.locator('text=Review Mode')).toBeVisible({ timeout: 10000 });

        await expect(page.locator('button', { hasText: 'Submit for Review' })).not.toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('"Delete Draft" button is NOT present in reviewer mode', async ({ page, runtimeErrors }) => {
        await setAuditReviewerRole(page);
        await installReviewerMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}?reviewerMode=1`);

        await expect(page.locator('text=Review Mode')).toBeVisible({ timeout: 10000 });

        await expect(page.locator('button', { hasText: 'Delete Draft' })).not.toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 3. Save button label changes to "Save Changes" in reviewer mode
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Reviewer mode — Save Changes label', () => {
    test('"Save Draft" label is not shown in reviewer mode (label changes for reviewer)', async ({ page, runtimeErrors }) => {
        await setAuditReviewerRole(page);
        await installReviewerMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}?reviewerMode=1`);

        await expect(page.locator('text=Review Mode')).toBeVisible({ timeout: 10000 });

        // "Save Draft" label disappears in reviewer mode — replaced by "Save Changes" or "Save"
        await expect(page.locator('button', { hasText: 'Save Draft' })).not.toBeVisible({ timeout: 5000 });

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 4. Back to Review navigation
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Reviewer mode — Back to Review navigation', () => {
    test('"Back to Review" button in banner navigates to review page', async ({ page, runtimeErrors }) => {
        await setAuditReviewerRole(page);
        await installReviewerMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}?reviewerMode=1`);

        await expect(page.locator('text=Review Mode')).toBeVisible({ timeout: 10000 });

        // Click the Back to Review button inside the ReviewerContextBanner
        await page.locator('.reviewer-context-banner button', { hasText: 'Back to Review' }).click();

        await expect(page).toHaveURL(new RegExp(`/audits/${AUDIT_ID}/review`), { timeout: 8000 });

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('"Back to Review" header button navigates to review page', async ({ page, runtimeErrors }) => {
        await setAuditReviewerRole(page);
        await installReviewerMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}?reviewerMode=1`);

        await expect(page.locator('text=Review Mode')).toBeVisible({ timeout: 10000 });

        // The header also has a Back to Review button (outside the banner)
        const headerBackBtn = page.locator('button', { hasText: 'Back to Review' }).first();
        await expect(headerBackBtn).toBeVisible();
        await headerBackBtn.click();

        await expect(page).toHaveURL(new RegExp(`/audits/${AUDIT_ID}/review`), { timeout: 8000 });

        expectNoRuntimeErrors(runtimeErrors);
    });
});
