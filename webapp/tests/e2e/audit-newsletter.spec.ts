import { Page, Route } from '@playwright/test';
import { expect, test } from './fixtures';
import { expectNoRuntimeErrors, gotoAndStabilize } from './utils/ui';

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

async function installNewsletterMocks(page: Page) {
    await page.route(/\/v1\/divisions(\?.*)?$/i, async (route) => {
        await json(route, [
            { id: 1, code: 'ETS', name: 'Evergreen Technical Services', auditType: 'JobSite' },
            { id: 2, code: 'SHC', name: 'Stronghold Companies', auditType: 'JobSite' },
        ]);
    });

    await page.route(/\/v1\/audits\/report(\?.*)?$/i, async (route) => {
        await json(route, {
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
                {
                    id: 3002,
                    divisionCode: 'ETS',
                    status: 'Submitted',
                    auditDate: '2026-03-05',
                    auditor: 'Bob Auditor',
                    jobNumber: 'J-3002',
                    location: 'Odessa',
                    scorePercent: 90,
                    nonConformingCount: 2,
                    warningCount: 1,
                },
            ],
        });
    });

    await page.route(/\/v1\/audits\/section-trends(\?.*)?$/i, async (route) => {
        await json(route, {
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
                {
                    sectionName: 'Permitting',
                    divisionTrend: [
                        { quarter: '2025 Q3', findingsPerAudit: 0.51, auditCount: 8, ncCount: 4 },
                        { quarter: '2025 Q4', findingsPerAudit: 0.44, auditCount: 9, ncCount: 4 },
                        { quarter: '2026 Q1', findingsPerAudit: 0.42, auditCount: 12, ncCount: 5 },
                    ],
                    companyTrend: [
                        { quarter: '2025 Q3', findingsPerAudit: 0.33, auditCount: 36, ncCount: 12 },
                        { quarter: '2025 Q4', findingsPerAudit: 0.31, auditCount: 40, ncCount: 12 },
                        { quarter: '2026 Q1', findingsPerAudit: 0.27, auditCount: 44, ncCount: 12 },
                    ],
                },
            ],
        });
    });
}

test.describe('Audit newsletter view', () => {
    test('renders key newsletter sections and allows narrative generation', async ({ page, runtimeErrors }) => {
        await installNewsletterMocks(page);

        await gotoAndStabilize(page, '/audit-management/newsletter?divisionId=1&year=2026&quarter=1');

        await expect(page.getByRole('heading', { name: 'Compliance Newsletter' })).toBeVisible();
        await expect(page.getByText('Stronghold Compliance Newsletter')).toBeVisible();
        await expect(page.getByText('Findings Overview')).toBeVisible();
        await expect(page.getByText('Per-Section Trend Lines')).toBeVisible();
        await expect(page.getByText('Auditor Performance')).toBeVisible();
        await expect(page.getByText('Corrective Actions Log')).toBeVisible();

        await page.getByRole('button', { name: 'Generate with AI (Draft)' }).click();
        const narrative = page.locator('textarea');
        await expect(narrative).not.toHaveValue('');
        await expect(narrative).toHaveValue(/2026 Q1/);

        expectNoRuntimeErrors(runtimeErrors);
    });
});
