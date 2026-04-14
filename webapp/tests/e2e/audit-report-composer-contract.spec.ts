import { Page, Route } from '@playwright/test';
import { expect, test } from './fixtures';
import { expectNoRuntimeErrors, gotoAndStabilize } from './utils/ui';

const composerGateEnabled = process.env.PW_AUDIT_COMPOSER_GATE === 'true';

function replyJson(route: Route, payload: unknown, status = 200) {
    return route.fulfill({
        status,
        headers: {
            'access-control-allow-origin': '*',
            'content-type': 'application/json',
        },
        body: JSON.stringify(payload),
    });
}

function parseBody(requestBody: string | null): Record<string, unknown> {
    if (!requestBody) return {};
    try {
        return JSON.parse(requestBody) as Record<string, unknown>;
    } catch {
        return {};
    }
}

async function installComposerMocks(page: Page) {
    const reportUrls: string[] = [];
    const trendUrls: string[] = [];
    const createBodies: Record<string, unknown>[] = [];
    const updateBodies: Record<string, unknown>[] = [];
    const deletedDraftIds: number[] = [];
    let draftList = [
        {
            id: 42,
            divisionId: 1,
            divisionCode: 'ETS',
            title: 'ETS Q1 Newsletter',
            period: 'Q1 2026',
            dateFrom: '2026-01-01',
            dateTo: '2026-03-31',
            createdAt: '2026-04-01T12:00:00Z',
            updatedAt: '2026-04-02T10:00:00Z',
            createdBy: 'qa@stronghold.local',
        },
        {
            id: 43,
            divisionId: 1,
            divisionCode: 'ETS',
            title: 'ETS Q2 Draft',
            period: 'Q2 2026',
            dateFrom: '2026-04-01',
            dateTo: '2026-06-30',
            createdAt: '2026-04-03T12:00:00Z',
            updatedAt: '2026-04-04T10:00:00Z',
            createdBy: 'qa@stronghold.local',
        },
    ];

    await page.route(/\/v1\/divisions(\?.*)?$/i, async (route) => {
        return replyJson(route, [
            { id: 1, code: 'ETS', name: 'Evergreen Technical Services', auditType: 'JobSite' },
            { id: 2, code: 'SHC', name: 'Stronghold Companies', auditType: 'JobSite' },
        ]);
    });

    await page.route(/\/v1\/audits\/report(\?.*)?$/i, async (route) => {
        reportUrls.push(route.request().url());
        return replyJson(route, {
            totalAudits: 12,
            avgScorePercent: 91.4,
            totalNonConforming: 14,
            totalWarnings: 5,
            correctedOnSiteCount: 8,
            trend: [],
            sectionBreakdown: [
                { sectionName: 'PPE', ncCount: 4 },
                { sectionName: 'Permitting', ncCount: 5 },
                { sectionName: 'Equipment', ncCount: 5 },
            ],
            openCorrectiveActions: [
                {
                    id: 901,
                    auditId: 3001,
                    description: 'Replace damaged fall-protection lanyard',
                    assignedTo: 'J. Foreman',
                    dueDate: '2026-04-12',
                    status: 'Open',
                    isOverdue: false,
                    daysOpen: 4,
                },
            ],
            rows: [
                {
                    id: 3001,
                    divisionCode: 'ETS',
                    status: 'Submitted',
                    auditDate: '2026-02-13',
                    auditor: 'Alice Inspector',
                    jobNumber: 'J-3001',
                    location: 'Midland',
                    scorePercent: 92,
                    nonConformingCount: 1,
                    warningCount: 0,
                },
            ],
        });
    });

    await page.route(/\/v1\/audits\/section-trends(\?.*)?$/i, async (route) => {
        trendUrls.push(route.request().url());
        return replyJson(route, {
            quarters: ['2025 Q3', '2025 Q4', '2026 Q1'],
            sections: [
                {
                    sectionName: 'PPE',
                    divisionTrend: [
                        { quarter: '2025 Q3', findingsPerAudit: 0.42, auditCount: 8, ncCount: 3 },
                        { quarter: '2025 Q4', findingsPerAudit: 0.35, auditCount: 9, ncCount: 3 },
                        { quarter: '2026 Q1', findingsPerAudit: 0.33, auditCount: 12, ncCount: 4 },
                    ],
                    companyTrend: [
                        { quarter: '2025 Q3', findingsPerAudit: 0.29, auditCount: 36, ncCount: 10 },
                        { quarter: '2025 Q4', findingsPerAudit: 0.25, auditCount: 40, ncCount: 10 },
                        { quarter: '2026 Q1', findingsPerAudit: 0.23, auditCount: 44, ncCount: 10 },
                    ],
                },
            ],
        });
    });

    // Newsletter template — none saved yet; return 200/null so browser doesn't log a 404 console error
    await page.route(/\/v1\/audits\/newsletter-template(\?.*)?$/i, async (route) => {
        return replyJson(route, null, 200);
    });

    await page.route(/\/v1\/audits\/report-drafts\/42(\?.*)?$/i, async (route) => {
        const method = route.request().method().toUpperCase();
        if (method === 'GET') {
            return replyJson(route, {
                id: 42,
                divisionId: 1,
                divisionCode: 'ETS',
                title: 'ETS Q1 Newsletter',
                period: 'Q1 2026',
                dateFrom: '2026-01-01',
                dateTo: '2026-03-31',
                blocksJson: JSON.stringify([
                    {
                        id: 'bar-1',
                        type: 'chart-bar',
                        isEdited: false,
                        style: { backgroundColor: '#0f172a' },
                        content: {
                            title: 'Findings by Section',
                            labels: ['PPE'],
                            datasets: [{ label: 'ETS', data: [0.3], backgroundColor: '#2563eb' }],
                            caption: 'Existing caption note',
                        },
                    },
                    {
                        id: 'nar-1',
                        type: 'narrative',
                        isEdited: true,
                        style: { backgroundColor: '#111827' },
                        content: {
                            text: 'Prior analyst narrative.',
                            aiPromptContext: 'ets-q1',
                        },
                    },
                ]),
                rowVersion: 'AAAAAAAAB9E=',
                createdAt: '2026-04-01T12:00:00Z',
                updatedAt: '2026-04-02T10:00:00Z',
                createdBy: 'qa@stronghold.local',
            });
        }

        if (method === 'PUT') {
            updateBodies.push(parseBody(route.request().postData()));
            return route.fulfill({ status: 204, headers: { 'access-control-allow-origin': '*' } });
        }

        return route.fulfill({ status: 405 });
    });

    await page.route(/\/v1\/audits\/report-drafts\/(\d+)(\?.*)?$/i, async (route) => {
        const method = route.request().method().toUpperCase();
        const url = new URL(route.request().url());
        const match = url.pathname.match(/\/v1\/audits\/report-drafts\/(\d+)/i);
        const id = Number(match?.[1] ?? 0);
        if (method === 'DELETE' && id > 0) {
            deletedDraftIds.push(id);
            draftList = draftList.filter(d => d.id !== id);
            return route.fulfill({ status: 204, headers: { 'access-control-allow-origin': '*' } });
        }
        return route.fallback();
    });

    await page.route(/\/v1\/audits\/report-drafts(\?.*)?$/i, async (route) => {
        const method = route.request().method().toUpperCase();

        if (method === 'GET') {
            return replyJson(route, draftList);
        }

        if (method === 'POST') {
            createBodies.push(parseBody(route.request().postData()));
            return replyJson(route, 42, 201);
        }

        return route.fulfill({ status: 405 });
    });

    return {
        reportUrls: () => reportUrls,
        trendUrls: () => trendUrls,
        createBodies: () => createBodies,
        updateBodies: () => updateBodies,
        deletedDraftIds: () => deletedDraftIds,
    };
}

test.describe('Audit report composer contract (feature-gated)', () => {
    test.skip(!composerGateEnabled, 'Enable with PW_AUDIT_COMPOSER_GATE=true when report composer UI is implemented.');

    test('persists date range and preserves annotations/styles during data regeneration', async ({ page, runtimeErrors }) => {
        const state = await installComposerMocks(page);

        await gotoAndStabilize(page, '/audit-management/reports/composer');
        await expect(page.getByRole('heading', { name: 'Report Composer' })).toBeVisible();

        // Select division ETS (select element — use selectOption, not fill)
        await page.locator('[data-testid="composer-filter-division"]').selectOption({ value: '1' });
        await page.locator('[data-testid="composer-filter-from"]').fill('2026-01-01');
        await page.locator('[data-testid="composer-filter-to"]').fill('2026-03-31');

        // Generate report — API must receive the correct date params
        await page.locator('[data-testid="composer-generate"]').click();

        await expect.poll(() => state.reportUrls().length).toBeGreaterThan(0);
        await expect.poll(() => state.trendUrls().length).toBeGreaterThan(0);
        // Report endpoint receives the selected date range
        expect(state.reportUrls()[0]).toContain('dateFrom=2026-01-01');
        expect(state.reportUrls()[0]).toContain('dateTo=2026-03-31');
        // Section trends intentionally omit date filters (full history for trend lines)

        // Load existing draft from the dropdown (select element — use selectOption, not click)
        await page.locator('[data-testid="composer-draft-select"]').selectOption({ value: '42' });

        // Wait for draft blocks to render in the canvas
        await expect(page.locator('[data-testid="composer-block-bar-1"]')).toBeVisible();
        await expect(page.locator('[data-testid="composer-block-nar-1"]')).toBeVisible();

        // Select the chart-bar block to show its properties in the right panel
        await page.locator('[data-testid="composer-block-bar-1"]').click();
        const caption = page.locator('[data-testid="block-caption-input"]');
        const styleBg = page.locator('[data-testid="block-style-backgroundColor"]');
        await expect(caption).toHaveValue('Existing caption note');

        // Edit caption and background color
        await caption.fill('Edited caption persists');
        await styleBg.fill('#123456');

        // Click the narrative block — inner text div has @click="editing=true" so
        // clicking the block wrapper both selects it and enters edit mode
        await page.locator('[data-testid="composer-block-nar-1"]').click();
        const narrative = page.locator('[data-testid="block-narrative-text"]');
        await expect(narrative).toBeVisible();
        await expect(narrative).toHaveValue('Prior analyst narrative.');
        await narrative.fill('Manual narrative should persist after refresh');

        // Refresh data — re-generate with same date params; engine preserves authored content
        await page.locator('[data-testid="composer-generate"]').click();

        // Re-select chart-bar block after regeneration to verify persistence in property panel
        await expect(page.locator('[data-testid="composer-block-bar-1"]')).toBeVisible();
        await page.locator('[data-testid="composer-block-bar-1"]').click();
        await expect(caption).toHaveValue('Edited caption persists');
        await expect(styleBg).toHaveValue('#123456');
        // Narrative textarea persists on canvas (editing stays true for same component key)
        await expect(narrative).toHaveValue('Manual narrative should persist after refresh');

        // Save draft — button is enabled because isDirty=true after edits
        await page.locator('[data-testid="composer-save"]').click();

        await expect.poll(() => state.updateBodies().length).toBeGreaterThan(0);
        const payload = state.updateBodies()[0];
        expect(payload.dateFrom).toBe('2026-01-01');
        expect(payload.dateTo).toBe('2026-03-31');

        const blocksJson = String(payload.blocksJson ?? '[]');
        const parsedBlocks = JSON.parse(blocksJson) as Array<Record<string, unknown>>;
        const barBlock = parsedBlocks.find(b => b.type === 'chart-bar') as Record<string, unknown> | undefined;
        const narrativeBlock = parsedBlocks.find(b => b.type === 'narrative') as Record<string, unknown> | undefined;

        expect(barBlock).toBeTruthy();
        expect(narrativeBlock).toBeTruthy();
        expect((barBlock?.content as { caption?: string })?.caption).toBe('Edited caption persists');
        expect((barBlock?.style as { backgroundColor?: string })?.backgroundColor).toBe('#123456');
        expect((narrativeBlock?.content as { text?: string })?.text).toBe('Manual narrative should persist after refresh');

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('manage drafts dialog can bulk-delete selected drafts', async ({ page, runtimeErrors }) => {
        const state = await installComposerMocks(page);

        await gotoAndStabilize(page, '/audit-management/reports/composer');
        await expect(page.getByRole('heading', { name: 'Report Composer' })).toBeVisible();

        await page.getByRole('button', { name: /Manage Drafts/i }).click();
        const dialog = page.getByRole('dialog', { name: /Manage Drafts/i });
        await expect(dialog).toBeVisible();
        await expect(dialog.locator('tbody tr')).toHaveCount(2);

        await dialog.locator('tbody tr').first().click();
        const deleteButton = dialog.getByRole('button', { name: /Delete Selected \(1\)/i });
        await expect(deleteButton).toBeVisible();

        page.once('dialog', d => d.accept());
        await deleteButton.click();

        await expect.poll(() => state.deletedDraftIds().length).toBe(1);
        await expect(dialog.locator('tbody tr')).toHaveCount(1);

        expectNoRuntimeErrors(runtimeErrors);
    });

    test('print/export mounts print-root and preserves chart pixels as images', async ({ page, runtimeErrors }) => {
        const state = await installComposerMocks(page);
        void state;

        await page.addInitScript(() => {
            // @ts-expect-error test-only global
            window.__printCalls = 0;
            window.print = () => {
                // @ts-expect-error test-only global
                window.__printCalls += 1;
            };
        });

        await gotoAndStabilize(page, '/audit-management/reports/composer');
        await expect(page.getByRole('heading', { name: 'Report Composer' })).toBeVisible();

        await page.locator('[data-testid="composer-filter-division"]').selectOption({ value: '1' });
        await page.locator('[data-testid="composer-draft-select"]').selectOption({ value: '42' });
        await expect(page.locator('[data-testid="composer-block-bar-1"] canvas').first()).toBeVisible();

        await page.locator('[data-testid="composer-print"]').click();
        const printRoot = page.locator('#print-root');
        await expect(printRoot).toBeVisible();

        // Converted charts should become <img data:image/png...> in print clone.
        const printedChartImages = printRoot.locator('img[src^="data:image/png"]');
        await expect(printedChartImages.first()).toBeVisible();

        const printCalls = await page.evaluate(() => {
            // @ts-expect-error test-only global
            return window.__printCalls ?? 0;
        });
        expect(printCalls).toBeGreaterThan(0);

        expectNoRuntimeErrors(runtimeErrors);
    });
});
