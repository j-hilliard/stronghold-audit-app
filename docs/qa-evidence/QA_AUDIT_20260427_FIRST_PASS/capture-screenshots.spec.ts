import { test } from '@playwright/test';
import path from 'node:path';

const evidenceRoot = process.env.QA_AUDIT_EVIDENCE_ROOT
    ? path.resolve(process.env.QA_AUDIT_EVIDENCE_ROOT)
    : path.resolve(__dirname, 'screenshots');

function json(route: any, payload: unknown, status = 200) {
    return route.fulfill({
        status,
        headers: {
            'access-control-allow-origin': '*',
            'content-type': 'application/json',
        },
        body: JSON.stringify(payload),
    });
}

const divisions = [
    { id: 1, code: 'TKIE', name: 'TK Industrial East', auditType: 'JobSite' },
    { id: 2, code: 'FAC', name: 'Facilities', auditType: 'Facility' },
];

const report = {
    totalAudits: 24,
    avgScorePercent: 88.5,
    totalNonConforming: 9,
    totalWarnings: 7,
    correctedOnSiteCount: 5,
    trend: [
        { week: '2026-W10', avgScore: 86.4, auditCount: 5 },
        { week: '2026-W11', avgScore: 89.2, auditCount: 6 },
    ],
    sectionBreakdown: [
        { sectionName: 'PPE', ncCount: 4 },
        { sectionName: 'Permitting', ncCount: 5 },
        { sectionName: 'Equipment', ncCount: 3 },
        { sectionName: 'Scaffolds', ncCount: 2 },
    ],
    openCorrectiveActions: [
        {
            id: 1,
            auditId: 100,
            description: 'Fix lanyard',
            assignedTo: 'J. Doe',
            dueDate: '2026-05-01',
            status: 'Open',
            isOverdue: false,
            daysOpen: 3,
        },
    ],
    rows: [
        {
            id: 100,
            divisionCode: 'TKIE',
            status: 'Submitted',
            auditDate: '2026-02-10',
            auditor: 'Alice',
            jobNumber: 'J-100',
            location: 'Houston',
            scorePercent: 89,
            nonConformingCount: 9,
            warningCount: 7,
        },
    ],
};

const templates = [
    {
        id: 100,
        templateId: 1,
        divisionCode: 'TKIE',
        divisionName: 'TK Industrial East',
        versionNumber: 7,
        status: 'Active',
        questionCount: 4,
    },
    {
        id: 101,
        templateId: 1,
        divisionCode: 'TKIE',
        divisionName: 'TK Industrial East',
        versionNumber: 8,
        status: 'Draft',
        questionCount: 4,
    },
];

const draftDetail = {
    id: 101,
    templateId: 1,
    versionNumber: 8,
    status: 'Draft',
    divisionCode: 'TKIE',
    divisionName: 'TK Industrial East',
    sections: [
        {
            id: 10,
            name: 'PPE',
            displayOrder: 1,
            questions: [
                {
                    versionQuestionId: 1001,
                    questionId: 501,
                    questionText: 'Hard hats worn correctly?',
                    displayOrder: 1,
                    weight: 1,
                    allowNA: true,
                    requireCommentOnNC: true,
                    isScoreable: true,
                    isLifeCritical: false,
                    requirePhotoOnNc: false,
                    autoCreateCa: true,
                },
            ],
        },
    ],
};

test.beforeEach(async ({ page }) => {
    await page.route(/\/v1\/divisions(\?.*)?$/i, (route) => json(route, divisions));
    await page.route(/\/v1\/audits\/report(\?.*)?$/i, (route) => json(route, report));
    await page.route(/\/v1\/audits\/section-trends(\?.*)?$/i, (route) => json(route, { quarters: [], sections: [] }));
    await page.route(/\/v1\/audits\/report-drafts(\?.*)?$/i, (route) => json(route, []));
    await page.route(/\/v1\/audits\/newsletter-template(\?.*)?$/i, (route) => json(route, null));
    await page.route(/\/v1\/admin\/templates(\?.*)?$/i, (route) => json(route, templates));
    await page.route(/\/v1\/admin\/versions\/101(\?.*)?$/i, (route) => json(route, draftDetail));
    await page.route(/\/v1\/admin\/section-library(\?.*)?$/i, (route) => json(route, []));
    await page.route(/\/v1\/admin\/logic-rules(\?.*)?$/i, (route) => json(route, []));
});

test('capture mocked audit evidence screenshots', async ({ page }) => {
    await page.setViewportSize({ width: 1536, height: 960 });

    await page.goto('/audit-management/reports');
    await page.getByRole('heading', { name: 'Audit Dashboard' }).waitFor();
    await page.screenshot({ path: path.join(evidenceRoot, 'reports-dashboard-mocked.png'), fullPage: false });

    await page.goto('/audit-management/newsletter?divisionId=1&year=2026&quarter=1');
    await page.getByRole('heading', { name: 'Compliance Newsletter' }).waitFor();
    await page.screenshot({ path: path.join(evidenceRoot, 'newsletter-mocked.png'), fullPage: false });

    await page.goto('/audit-management/reports/composer');
    await page.getByRole('heading', { name: 'Report Composer' }).waitFor();
    await page.screenshot({ path: path.join(evidenceRoot, 'report-composer-mocked.png'), fullPage: false });

    await page.goto('/audit-management/admin/templates');
    await page.getByRole('heading', { name: 'Template Manager' }).waitFor();
    await page.locator('select').first().selectOption('TKIE');
    await page.locator('select').nth(1).selectOption('101');
    await page.getByText('PPE').first().waitFor();
    await page.screenshot({ path: path.join(evidenceRoot, 'template-manager-draft-mocked.png'), fullPage: false });
});
