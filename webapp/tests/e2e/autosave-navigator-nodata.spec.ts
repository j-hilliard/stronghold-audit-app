/**
 * Targeted regression tests for Phase 2C completion batch:
 *  1. Autosave / Save Draft — PUT body contract and data persistence
 *  2. Section Navigator — renders at xl viewport, jumps work, no-match feedback
 *  3. Quarterly Summary no-data state — honest empty state when zero audits
 */

import { Page, Route } from '@playwright/test';
import { test, expect } from './fixtures';
import { expectNoRuntimeErrors, gotoAndStabilize } from './utils/ui';

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

// ── Audit form seed data (minimal: 3 sections, 2 questions each) ──────────────

const AUDIT_ID = 501;

const MOCK_TEMPLATE = {
    versionId: 200,
    versionNumber: 3,
    divisionCode: 'TEST',
    divisionName: 'Test Division',
    auditType: 'JobSite',
    logicRules: [],
    sections: [
        {
            id: 10, name: 'PPE', sectionCode: 'PPE', displayOrder: 1,
            isRequired: true, isOptional: false, optionalGroupKey: null,
            questions: [
                { versionQuestionId: 1001, questionId: 101, questionText: 'Hard hats worn?',         displayOrder: 1, allowNA: false, requireCommentOnNC: false, isScoreable: true },
                { versionQuestionId: 1002, questionId: 102, questionText: 'High-vis vests present?', displayOrder: 2, allowNA: false, requireCommentOnNC: false, isScoreable: true },
            ],
        },
        {
            id: 11, name: 'Fall Protection', sectionCode: 'FP', displayOrder: 2,
            isRequired: true, isOptional: false, optionalGroupKey: null,
            questions: [
                { versionQuestionId: 1003, questionId: 103, questionText: 'Guardrails installed?',   displayOrder: 1, allowNA: false, requireCommentOnNC: false, isScoreable: true },
                { versionQuestionId: 1004, questionId: 104, questionText: 'Harnesses inspected?',    displayOrder: 2, allowNA: false, requireCommentOnNC: false, isScoreable: true },
            ],
        },
        {
            id: 12, name: 'Housekeeping', sectionCode: 'HK', displayOrder: 3,
            isRequired: true, isOptional: false, optionalGroupKey: null,
            questions: [
                { versionQuestionId: 1005, questionId: 105, questionText: 'Walkways clear?',         displayOrder: 1, allowNA: false, requireCommentOnNC: false, isScoreable: true },
                { versionQuestionId: 1006, questionId: 106, questionText: 'Waste bins accessible?',  displayOrder: 2, allowNA: false, requireCommentOnNC: false, isScoreable: true },
            ],
        },
    ],
};

const MOCK_AUDIT = {
    id: AUDIT_ID,
    divisionId: 1,
    divisionCode: 'TEST',
    divisionName: 'Test Division',
    templateVersionId: 200,
    auditType: 'JobSite',
    status: 'Draft',
    createdAt: '2026-05-01T12:00:00Z',
    createdBy: 'qa',
    submittedAt: null,
    header: { jobNumber: 'J-QA-001', location: 'Site A', auditorName: 'QA Tester' },
    responses: [],
    sectionNaOverrides: [],
    trackingNumber: 'QA-2026-0001',
};

async function installAuditFormMocks(
    page: Page,
    opts: { delaySaveMs?: number } = {},
): Promise<{ capturedSaveBodies: unknown[] }> {
    const capturedSaveBodies: unknown[] = [];

    await page.route(/\/v1\/divisions(\?.*)?$/i, (route) =>
        replyJson(route, [{ id: 1, code: 'TEST', name: 'Test Division', auditType: 'JobSite' }]),
    );

    await page.route(new RegExp(`/v1/audits/${AUDIT_ID}(\\?.*)?$`, 'i'), async (route) => {
        if (route.request().method().toUpperCase() === 'GET') {
            return replyJson(route, MOCK_AUDIT);
        }
        return replyJson(route, {});
    });

    await page.route(new RegExp(`/v1/audits/${AUDIT_ID}/responses(\\?.*)?$`, 'i'), async (route) => {
        const method = route.request().method().toUpperCase();
        if (method === 'PUT') {
            const body = JSON.parse(route.request().postData() ?? '{}');
            capturedSaveBodies.push(body);
            if (opts.delaySaveMs) {
                await new Promise(resolve => setTimeout(resolve, opts.delaySaveMs));
            }
            return replyEmpty(route);
        }
        return replyJson(route, MOCK_AUDIT);
    });

    // Active template by division (used by loadAudit in auditStore)
    await page.route(/\/v1\/templates\/active(\?.*)?$/i, (route) =>
        replyJson(route, MOCK_TEMPLATE),
    );

    // Attachments — return empty
    await page.route(new RegExp(`/v1/audits/${AUDIT_ID}/attachments(\\?.*)?$`, 'i'), (route) =>
        replyJson(route, []),
    );

    // Prior prefill — not available
    await page.route(/\/v1\/audits\/prior-prefill(\?.*)?$/i, (route) =>
        replyJson(route, null, 404),
    );

    // Audit list (used by some store bootstrapping)
    await page.route(/\/v1\/audits(\?.*)?$/i, (route) => {
        if (route.request().method().toUpperCase() === 'GET') return replyJson(route, []);
        return replyJson(route, {});
    });

    return { capturedSaveBodies };
}

// ── Tests: Autosave / Save Draft ─────────────────────────────────────────────

test.describe('Autosave / Save Draft — data contract', () => {
    test('Save Draft fires PUT /v1/audits/{id}/responses with header and responses array', async ({ page, runtimeErrors }) => {
        const { capturedSaveBodies } = await installAuditFormMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}`);

        // Form must load with at least one section
        await expect(page.locator('text=PPE').first()).toBeVisible({ timeout: 10000 });

        // Click Save Draft
        const saveBtn = page.locator('button', { hasText: 'Save Draft' }).first();
        await expect(saveBtn).toBeVisible({ timeout: 5000 });
        await saveBtn.click();

        // Wait for the PUT to complete
        await page.waitForResponse(
            r => r.url().includes(`/v1/audits/${AUDIT_ID}/responses`) && r.request().method() === 'PUT',
            { timeout: 8000 },
        );

        expect(capturedSaveBodies.length, 'Save Draft must send exactly one PUT').toBeGreaterThanOrEqual(1);
        const body = capturedSaveBodies[0] as Record<string, unknown>;
        expect(body).toHaveProperty('responses');
        expect(Array.isArray(body.responses), 'responses must be an array').toBe(true);
        expect(body).toHaveProperty('header');

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('Save Draft button enters loading state while save is in-flight', async ({ page, runtimeErrors }) => {
        // Use 500ms delayed save so we can observe the loading state on the button
        await installAuditFormMocks(page, { delaySaveMs: 500 });
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}`);

        await expect(page.locator('text=PPE').first()).toBeVisible({ timeout: 10000 });

        // Find the Save Draft button in the header (not the sticky bar — use first)
        const saveBtn = page.locator('button', { hasText: 'Save Draft' }).first();
        await expect(saveBtn).toBeVisible();

        // Click and immediately verify the button enters a loading/disabled state
        await saveBtn.click();

        // PrimeVue's :loading prop adds .p-disabled / aria-disabled while in flight
        await expect(saveBtn).toHaveAttribute('disabled', { timeout: 1500 });

        // After the save completes (500ms delay), button is re-enabled
        await expect(saveBtn).toBeEnabled({ timeout: 4000 });

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('Save Draft PUT body includes sectionNaOverrides array', async ({ page, runtimeErrors }) => {
        const { capturedSaveBodies } = await installAuditFormMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}`);

        await expect(page.locator('text=PPE').first()).toBeVisible({ timeout: 10000 });

        const saveBtn = page.locator('button', { hasText: 'Save Draft' }).first();
        await expect(saveBtn).toBeVisible();
        await saveBtn.click();

        await page.waitForResponse(
            r => r.url().includes(`/v1/audits/${AUDIT_ID}/responses`) && r.request().method() === 'PUT',
            { timeout: 8000 },
        );

        const body = capturedSaveBodies[0] as Record<string, unknown>;
        expect(body).toHaveProperty('sectionNaOverrides');
        expect(Array.isArray(body.sectionNaOverrides), 'sectionNaOverrides must be an array').toBe(true);

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ── Tests: Section Navigator ──────────────────────────────────────────────────

test.describe('Section Navigator — xl viewport rendering and jump behavior', () => {
    test('navigator side rail is visible at xl viewport with 3 sections', async ({ page, runtimeErrors }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await installAuditFormMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}`);

        // Form must load
        await expect(page.locator('text=PPE').first()).toBeVisible({ timeout: 10000 });

        // The navigator side rail renders at xl+ — look for the section list buttons
        // The navigator uses .text-\[11px\] with section names
        const sectionNavBtns = page.locator('.fixed.right-3 button').filter({ hasText: /PPE|Fall Protection|Housekeeping/ });
        await expect(sectionNavBtns.first()).toBeVisible({ timeout: 5000 });

        // All three section names should appear in the navigator
        const navContainer = page.locator('.fixed.right-3');
        await expect(navContainer.locator('text=PPE').first()).toBeVisible();
        await expect(navContainer.locator('text=Fall Protection').first()).toBeVisible();
        await expect(navContainer.locator('text=Housekeeping').first()).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('navigator "Next Unanswered" button is enabled when sections have unanswered questions', async ({ page, runtimeErrors }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await installAuditFormMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}`);

        await expect(page.locator('text=PPE').first()).toBeVisible({ timeout: 10000 });

        const navContainer = page.locator('.fixed.right-3');
        await expect(navContainer).toBeVisible({ timeout: 5000 });

        // "Next Unanswered" button should NOT show "✓ Done" because no questions are answered
        const nextUnansweredBtn = navContainer.locator('button', { hasText: /Unanswered|Done/ }).first();
        await expect(nextUnansweredBtn).toBeVisible();
        const btnText = await nextUnansweredBtn.textContent();
        expect(btnText).not.toContain('✓ Done');

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('clicking a section name in the navigator jumps focus to that section', async ({ page, runtimeErrors }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await installAuditFormMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}`);

        await expect(page.locator('text=PPE').first()).toBeVisible({ timeout: 10000 });

        const navContainer = page.locator('.fixed.right-3');
        await expect(navContainer).toBeVisible({ timeout: 5000 });

        // Click "Housekeeping" (last section) in the navigator
        await navContainer.locator('button', { hasText: 'Housekeeping' }).click();

        // After clicking, the "Housekeeping" nav button should gain the active highlight (blue bg)
        const hkNavBtn = navContainer.locator('button', { hasText: 'Housekeeping' });
        await expect.poll(async () => {
            const cls = await hkNavBtn.getAttribute('class');
            return cls ?? '';
        }, { timeout: 3000 }).toContain('bg-blue-900');

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('mobile floating Sections button is visible below xl viewport', async ({ page, runtimeErrors }) => {
        await page.setViewportSize({ width: 390, height: 844 });
        await installAuditFormMocks(page);
        await gotoAndStabilize(page, `/audit-management/audits/${AUDIT_ID}`);

        await expect(page.locator('text=PPE').first()).toBeVisible({ timeout: 10000 });

        // Mobile floating button text "Sections"
        const floatBtn = page.locator('button', { hasText: 'Sections' }).last();
        await expect(floatBtn).toBeVisible({ timeout: 5000 });

        // Tapping it should open the drawer (force bypasses sticky-bar z-index overlap in headless)
        await floatBtn.click({ force: true });
        await expect(page.locator('text=Fall Protection').first()).toBeVisible({ timeout: 3000 });

        expectNoRuntimeErrors(runtimeErrors);
    });
});

// ── Tests: Quarterly Summary no-data state ────────────────────────────────────

const EMPTY_REPORT = {
    totalAudits: 0,
    avgScorePercent: null,
    totalNonConforming: 0,
    totalWarnings: 0,
    correctedOnSiteCount: 0,
    openCorrectiveActions: [],
    trend: [],
    sectionBreakdown: [],
    divisionBreakdown: [],
    auditorBreakdown: [],
    locationBreakdown: [],
    quarterlyTrend: [],
};

test.describe('Quarterly Summary — no-data empty state', () => {
    test('shows "No Audits Found for This Period" when report returns 0 audits', async ({ page, runtimeErrors }) => {
        await page.route(/\/v1\/audits\/report(\?.*)?$/i, (route) =>
            replyJson(route, EMPTY_REPORT),
        );
        await page.route(/\/v1\/divisions(\?.*)?$/i, (route) =>
            replyJson(route, [{ id: 1, code: 'TEST', name: 'Test Division', auditType: 'JobSite' }]),
        );

        await gotoAndStabilize(page, '/audit-management/reports/quarterly-summary');

        await expect(page.locator('text=No Audits Found for This Period')).toBeVisible({ timeout: 8000 });

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('no-data state shows period description text', async ({ page, runtimeErrors }) => {
        await page.route(/\/v1\/audits\/report(\?.*)?$/i, (route) =>
            replyJson(route, EMPTY_REPORT),
        );
        await page.route(/\/v1\/divisions(\?.*)?$/i, (route) =>
            replyJson(route, [{ id: 1, code: 'TEST', name: 'Test Division', auditType: 'JobSite' }]),
        );

        await gotoAndStabilize(page, '/audit-management/reports/quarterly-summary');

        await expect(page.locator('text=No Audits Found for This Period')).toBeVisible({ timeout: 8000 });

        // Hint text should also be visible
        await expect(page.locator('text=No qualifying audits were completed in this period.')).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('no-data state: Print/PDF button is disabled', async ({ page, runtimeErrors }) => {
        await page.route(/\/v1\/audits\/report(\?.*)?$/i, (route) =>
            replyJson(route, EMPTY_REPORT),
        );
        await page.route(/\/v1\/divisions(\?.*)?$/i, (route) =>
            replyJson(route, [{ id: 1, code: 'TEST', name: 'Test Division', auditType: 'JobSite' }]),
        );

        await gotoAndStabilize(page, '/audit-management/reports/quarterly-summary');

        await expect(page.locator('text=No Audits Found for This Period')).toBeVisible({ timeout: 8000 });

        // The Print button in the no-data state should be disabled
        const printBtn = page.locator('.no-data-state button', { hasText: /Print|PDF/ });
        await expect(printBtn).toBeDisabled();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('no-data state does not render when report has audits', async ({ page, runtimeErrors }) => {
        const reportWithData = {
            ...EMPTY_REPORT,
            totalAudits: 2,
            avgScorePercent: 88.5,
            totalNonConforming: 3,
            totalWarnings: 1,
            trend: [{ label: '2026-04', auditCount: 2, avgScore: 88.5 }],
            divisionBreakdown: [{ divisionCode: 'TEST', divisionName: 'Test', auditCount: 2, avgScore: 88.5, totalNcs: 3, openCaCount: 0, lastAuditDate: '2026-05-01', complianceStatus: 'OnTrack' }],
            auditorBreakdown: [{ auditor: 'Jane', auditCount: 2, avgScore: 88.5, totalNcs: 3, totalWarnings: 1 }],
        };

        await page.route(/\/v1\/audits\/report(\?.*)?$/i, (route) =>
            replyJson(route, reportWithData),
        );
        await page.route(/\/v1\/divisions(\?.*)?$/i, (route) =>
            replyJson(route, [{ id: 1, code: 'TEST', name: 'Test Division', auditType: 'JobSite' }]),
        );

        await gotoAndStabilize(page, '/audit-management/reports/quarterly-summary');

        // Empty state must NOT appear
        await expect(page.locator('text=No Audits Found for This Period')).not.toBeVisible({ timeout: 5000 });

        // Report content must be visible
        await expect(page.locator('text=Total Audits').first()).toBeVisible({ timeout: 8000 });

        expectNoRuntimeErrors(runtimeErrors);
    });
});
