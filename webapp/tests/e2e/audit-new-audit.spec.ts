import { Page, Route } from '@playwright/test';
import { test, expect } from './fixtures';
import { expectNoRuntimeErrors, gotoAndStabilize } from './utils/ui';

type DivisionRow = {
    Id: number;
    Code: string;
    Name: string;
    AuditType: 'JobSite' | 'Facility';
};

const baselineDivisions: DivisionRow[] = [
    { Id: 1, Code: 'CIV', Name: 'Civil', AuditType: 'JobSite' },
    { Id: 1, Code: 'CIV', Name: 'Civil Duplicate', AuditType: 'JobSite' },
    { Id: 2, Code: 'FAC', Name: 'Facilities', AuditType: 'Facility' },
];

function json(route: Route, payload: unknown, status = 200) {
    return route.fulfill({
        status,
        headers: {
            'access-control-allow-origin': '*',
            'content-type': 'application/json',
        },
        body: JSON.stringify(payload),
    });
}

function parseDivisionId(route: Route): number {
    const body = route.request().postData();
    if (!body) return 0;
    try {
        const data = JSON.parse(body) as { divisionId?: number };
        return Number(data.divisionId ?? 0);
    } catch {
        return 0;
    }
}

async function installAuditMocks(
    page: Page,
    onCreate?: (divisionId: number) => void,
) {
    const createdAuditId = 900;

    await page.route(/\/v1\/divisions(\?.*)?$/i, async (route) => {
        const payload = [
            ...baselineDivisions,
            // Invalid row (no Id) should be filtered out by the store normalizer.
            { Code: 'BAD', Name: 'Invalid Row', AuditType: 'JobSite' },
        ];
        await json(route, payload);
    });

    // Admin templates — needed by NewAuditView to show the template card after division select.
    await page.route(/\/v1\/admin\/templates(\?.*)?$/i, async (route) => {
        await json(route, [
            { id: 3001, templateId: 1, divisionCode: 'CIV', divisionName: 'Civil',      versionNumber: 2, status: 'Active', questionCount: 10 },
            { id: 3002, templateId: 2, divisionCode: 'FAC', divisionName: 'Facilities', versionNumber: 1, status: 'Active', questionCount: 5  },
        ]);
    });

    await page.route(/\/v1\/admin\/versions\/(3001|3002)(\?.*)?$/i, async (route) => {
        await json(route, { id: 3001, versionNumber: 1, divisionCode: 'CIV', divisionName: 'Civil', sections: [{ id: 1, name: 'General', sectionCode: null, displayOrder: 1, isRequired: false, questions: [] }] });
    });

    await page.route(/\/v1\/audits(\?.*)?$/i, async (route) => {
        const method = route.request().method().toUpperCase();
        if (method === 'GET') {
            await json(route, []);
            return;
        }
        if (method !== 'POST') {
            await json(route, { message: 'Method not allowed in mock.' }, 405);
            return;
        }
        const divisionId = parseDivisionId(route);
        onCreate?.(divisionId);
        const isFacility = divisionId === 2;
        await json(route, {
            id: createdAuditId,
            divisionId,
            divisionCode: isFacility ? 'FAC' : 'CIV',
            divisionName: isFacility ? 'Facilities' : 'Civil',
            templateVersionId: isFacility ? 3002 : 3001,
            auditType: isFacility ? 'Facility' : 'JobSite',
            status: 'Draft',
            createdAt: '2026-03-31T12:00:00Z',
            createdBy: 'qa-e2e',
            submittedAt: null,
            header: {},
            responses: [],
        }, 201);
    });

    await page.route(new RegExp(`/v1/audits/${createdAuditId}(\\?.*)?$`, 'i'), async (route) => {
        await json(route, {
            id: createdAuditId,
            divisionId: 2,
            divisionCode: 'FAC',
            divisionName: 'Facilities',
            templateVersionId: 3002,
            auditType: 'Facility',
            status: 'Draft',
            createdAt: '2026-03-31T12:00:00Z',
            createdBy: 'qa-e2e',
            submittedAt: null,
            header: {},
            responses: [],
        });
    });

    await page.route(/\/v1\/templates\/active(\?.*)?$/i, async (route) => {
        await json(route, {
            versionId: 3002, versionNumber: 1, divisionCode: 'FAC',
            divisionName: 'Facilities', auditType: 'Facility', sections: [],
        });
    });
}

test.describe('Audit New page guardrails', () => {
    test('dedupes PascalCase division payload and keeps Start Audit disabled before selection', async ({ page, runtimeErrors }) => {
        await installAuditMocks(page);
        await gotoAndStabilize(page, '/audit-management/audits/new');

        // Division dropdown must exist and show exactly 2 options after dedup + invalid-row filter.
        const divisionSelect = page.locator('select');
        await expect(divisionSelect).toBeVisible();
        const optionTexts = await divisionSelect.locator('option:not([disabled])').allTextContents();
        expect(optionTexts).toHaveLength(2);
        expect(optionTexts.some(t => /Civil/i.test(t))).toBe(true);
        expect(optionTexts.some(t => /Facilities/i.test(t))).toBe(true);

        // Start Audit stays disabled until a division is chosen.
        await expect(page.getByRole('button', { name: 'Start Audit' })).toBeDisabled();

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('only one division selectable and Start Audit posts selected division id', async ({ page, runtimeErrors }) => {
        let postedDivisionId = 0;
        await installAuditMocks(page, (divisionId) => {
            postedDivisionId = divisionId;
        });
        await gotoAndStabilize(page, '/audit-management/audits/new');

        // Select Civil first — template auto-loads when division is chosen.
        // Template card shows "Division — vN" (e.g. "Civil — v2")
        await page.selectOption('select', { value: '1' });
        await expect(page.getByText(/Civil — v\d/i)).toBeVisible({ timeout: 8000 });
        await expect(page.getByRole('button', { name: 'Start Audit' })).toBeEnabled();

        // Switch division — Civil template clears, Facilities template loads.
        await page.selectOption('select', { value: '2' });
        await expect(page.getByText(/Facilities — v\d/i)).toBeVisible({ timeout: 8000 });
        await expect(page.getByText(/Civil — v\d/i)).not.toBeVisible();

        const startAuditButton = page.getByRole('button', { name: 'Start Audit' });
        await expect(startAuditButton).toBeEnabled();
        await startAuditButton.click();

        await expect(page).toHaveURL(/\/audit-management\/audits\/900$/);
        expect(postedDivisionId).toBe(2);

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('Cancel returns to the audit dashboard route', async ({ page, runtimeErrors }) => {
        await installAuditMocks(page);
        await gotoAndStabilize(page, '/audit-management/audits/new');

        await page.getByRole('button', { name: 'Cancel' }).click();
        await expect(page).toHaveURL(/\/audit-management\/audits$/);
        await expect(page.getByRole('heading', { name: 'Compliance Audits' })).toBeVisible();

        expectNoRuntimeErrors(runtimeErrors);
    });
});
