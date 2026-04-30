import { test, expect, Page, APIRequestContext } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';

test.describe.configure({ timeout: 900_000, mode: 'serial' });

const sweepRoot = path.resolve(process.cwd(), '..', 'docs', 'qa-evidence', 'QA_AUDIT_20260429_FULL_APP_SCREEN_SWEEP');
const screenshotRoot = path.join(sweepRoot, 'screenshots');
const apiBase = process.env.PLAYWRIGHT_API_BASE_URL ?? process.env.VITE_APP_API_BASE_URL ?? 'http://localhost:7221';

type AuditListItem = {
    id: number;
    status?: string | null;
    divisionCode?: string | null;
};

function ensureDirs() {
    fs.mkdirSync(screenshotRoot, { recursive: true });
}

async function disableAnimations(page: Page) {
    await page.addStyleTag({
        content: `
            *, *::before, *::after {
                transition: none !important;
                animation: none !important;
                caret-color: transparent !important;
            }
            .p-toast, .p-tooltip, #nprogress, .p-ink {
                display: none !important;
            }
        `,
    });
}

async function gotoStable(page: Page, route: string) {
    await page.goto(route, { waitUntil: 'domcontentloaded' });
    try {
        await page.waitForLoadState('networkidle', { timeout: 7_000 });
    } catch {
        // Some pages keep background requests alive.
    }
    await disableAnimations(page);
    await page.waitForTimeout(250);
}

async function capture(page: Page, name: string) {
    await disableAnimations(page);
    await page.screenshot({
        path: path.join(screenshotRoot, `${name}.png`),
        fullPage: true,
    });
}

async function clickIfVisible(page: Page, selectorOrText: string, name?: string) {
    try {
        const bySelector = page.locator(selectorOrText).first();
        if (await bySelector.count()) {
            if (await bySelector.isVisible({ timeout: 750 })) {
                await bySelector.click({ force: true });
                await page.waitForTimeout(250);
                return true;
            }
        }
    } catch {
        // Fall through to text when selectorOrText is plain text or invalid CSS.
    }

    const byText = page.getByText(selectorOrText, { exact: name === 'exact' }).first();
    if (await byText.count()) {
        try {
            if (await byText.isVisible({ timeout: 750 })) {
                await byText.click({ force: true });
                await page.waitForTimeout(250);
                return true;
            }
        } catch {
            return false;
        }
    }
    return false;
}

async function closeOverlay(page: Page) {
    await page.keyboard.press('Escape');
    await page.waitForTimeout(150);
}

async function closeDialog(page: Page) {
    const close = page.locator('.p-dialog-header-close:visible, button[aria-label="Close"]:visible').last();
    if (await close.count()) {
        await close.click({ force: true });
        await page.waitForTimeout(200);
        return;
    }
    await closeOverlay(page);
}

async function selectFirstNativeOption(page: Page, selectLocator: string) {
    const select = page.locator(selectLocator).first();
    if (!(await select.count())) return false;
    const values = await select.locator('option').evaluateAll((options) =>
        options
            .filter((option) => !(option as HTMLOptionElement).disabled)
            .map((option) => (option as HTMLOptionElement).value)
            .filter((value) => value !== '' && value !== 'null')
    );
    if (!values.length) return false;
    await select.selectOption(values[0]);
    await page.waitForTimeout(700);
    return true;
}

async function fetchAudits(request: APIRequestContext): Promise<AuditListItem[]> {
    const response = await request.get(`${apiBase}/v1/audits`, { timeout: 15_000 });
    if (!response.ok()) return [];
    const data = await response.json();
    if (Array.isArray(data)) return data;
    if (Array.isArray(data?.items)) return data.items;
    return [];
}

test('live audit app screenshot sweep', async ({ page, request }) => {
    ensureDirs();
    page.setDefaultTimeout(3_000);
    page.setDefaultNavigationTimeout(20_000);

    const log: string[] = [];
    const consoleErrors: string[] = [];
    page.on('console', (message) => {
        if (message.type() === 'error') consoleErrors.push(message.text());
    });
    page.on('pageerror', (error) => consoleErrors.push(error.message));

    await page.addInitScript(() => {
        window.print = () => console.log('[qa-sweep] window.print stubbed');
    });

    const health = await request.get(`${apiBase}/v1/divisions`, { timeout: 15_000 });
    expect(health.ok(), `API health failed for ${apiBase}/v1/divisions`).toBeTruthy();

    const audits = await fetchAudits(request);
    const draftAudit = audits.find((a) => ['Draft', 'Reopened'].includes(String(a.status))) ?? audits[0];
    const reviewAudit = audits.find((a) => !['Draft', 'Reopened'].includes(String(a.status))) ?? audits[0];

    async function step(name: string, action: () => Promise<void>) {
        try {
            log.push(`START ${name}`);
            await action();
            log.push(`OK ${name}`);
        } catch (error) {
            log.push(`ERROR ${name}: ${(error as Error).message}`);
            try {
                await capture(page, `ERROR-${name.replace(/[^a-z0-9]+/gi, '-').toLowerCase()}`);
            } catch {
                // Best-effort only.
            }
        }
    }

    await page.setViewportSize({ width: 1536, height: 960 });

    await step('reports-dashboard-base', async () => {
        await gotoStable(page, '/audit-management/reports');
        await capture(page, '01-reports-dashboard-base');
    });

    await step('reports-dashboard-menus-and-customize', async () => {
        await clickIfVisible(page, 'button[title="Customize widgets"]');
        await capture(page, '02-reports-customize-panel');
        await closeOverlay(page);
        await clickIfVisible(page, 'button:has-text("Reports")');
        await capture(page, '03-reports-menu-open');
        await closeOverlay(page);
        const splitMenu = page.locator('.p-splitbutton-menubutton').first();
        if (await splitMenu.count()) {
            await splitMenu.click({ force: true });
            await page.waitForTimeout(250);
            await capture(page, '04-reports-export-menu-open');
            await closeOverlay(page);
        }
    });

    await step('reports-dashboard-tabs', async () => {
        for (const label of ['Overview', 'Action Items', 'Audit History', 'Analysis', 'Performance']) {
            const tab = page.getByRole('button', { name: new RegExp(label, 'i') }).first();
            if (await tab.count()) {
                await tab.click({ force: true, timeout: 2_000 });
                await page.waitForTimeout(250);
                await capture(page, `05-reports-tab-${label.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`);
            } else {
                log.push(`SKIP reports tab: ${label}`);
            }
        }
    });

    await step('audit-list-base-filters-print-selection', async () => {
        await gotoStable(page, '/audit-management/audits');
        await capture(page, '10-audits-list-base');
        const dropdown = page.locator('.p-dropdown').first();
        if (await dropdown.count()) {
            await dropdown.click({ force: true });
            await capture(page, '11-audits-list-filter-dropdown-open');
            await closeOverlay(page);
        }
            await page.getByRole('button', { name: /Print Blank Form/i }).click({ timeout: 2_000 });
        await capture(page, '12-audits-print-blank-form-dialog');
        await closeDialog(page);
        const checkbox = page.locator('.p-checkbox-box').first();
        if (await checkbox.count()) {
            await checkbox.click({ force: true });
            await page.waitForTimeout(250);
            await capture(page, '13-audits-list-row-selected-bulk-state');
        }
    });

    await step('new-audit-base-selected-division', async () => {
        await gotoStable(page, '/audit-management/audits/new');
        await capture(page, '20-new-audit-base');
        if (await selectFirstNativeOption(page, 'select')) {
            await capture(page, '21-new-audit-division-selected');
        }
    });

    if (draftAudit?.id) {
        await step('audit-form-base-and-controls', async () => {
            await gotoStable(page, `/audit-management/audits/${draftAudit.id}`);
            await capture(page, '30-audit-form-base');
            await page.getByRole('button', { name: /Collapse All/i }).click({ force: true, timeout: 2_000 });
            await capture(page, '31-audit-form-collapse-all');
            await page.getByRole('button', { name: /Expand All/i }).click({ force: true, timeout: 2_000 });
            await capture(page, '32-audit-form-expand-all');
            if (await clickIfVisible(page, 'Mark N/A')) {
                await capture(page, '33-audit-form-section-na-dialog-or-state');
                await closeOverlay(page);
            }
        });
    } else {
        log.push('SKIP audit form: no audit IDs returned by API');
    }

    if (reviewAudit?.id) {
        await step('audit-review-base-modals-print', async () => {
            await gotoStable(page, `/audit-management/audits/${reviewAudit.id}/review`);
            await capture(page, '40-audit-review-base');
            for (const label of ['Add Recipient', 'Send Distribution Email', 'Reopen Audit', 'Close Audit']) {
                if (await clickIfVisible(page, label)) {
                    await capture(page, `41-audit-review-${label.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`);
                    await closeDialog(page);
                }
            }
            await gotoStable(page, `/audit-management/print-review/${reviewAudit.id}`);
            await capture(page, '42-audit-review-print-route');
        });
    } else {
        log.push('SKIP audit review: no audit IDs returned by API');
    }

    await step('corrective-actions-base-filters-bulk-dialogs', async () => {
        await gotoStable(page, '/audit-management/corrective-actions');
        await capture(page, '50-corrective-actions-base');
        const caCheckbox = page.locator('.p-checkbox-box').first();
        if (await caCheckbox.count()) {
            await caCheckbox.click({ force: true });
            await capture(page, '51-corrective-actions-bulk-toolbar');
            for (const label of ['Close All', 'Reassign']) {
                if (await clickIfVisible(page, label)) {
                    await capture(page, `52-corrective-actions-bulk-${label.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`);
                    await closeDialog(page);
                }
            }
        }
        const rowAction = page.locator('.ca-row-actions button:visible').first();
        if (await rowAction.count()) {
            await rowAction.click({ force: true });
            await capture(page, '53-corrective-actions-first-row-action-dialog');
            await closeDialog(page);
        }
    });

    await step('template-manager-active-and-draft-surfaces', async () => {
        await gotoStable(page, '/audit-management/admin/templates');
        await capture(page, '60-template-manager-base');
        if (await selectFirstNativeOption(page, 'select')) {
            await capture(page, '61-template-manager-division-selected');
        }
        const versionSelect = page.locator('select').nth(1);
        if (await versionSelect.count()) {
            const draftValue = await versionSelect.locator('option').evaluateAll((options) => {
                const option = options.find((o) => (o.textContent ?? '').includes('Draft')) as HTMLOptionElement | undefined;
                return option?.value ?? null;
            });
            if (draftValue) {
                await versionSelect.selectOption(draftValue);
                await page.waitForTimeout(700);
                await capture(page, '62-template-manager-draft-selected');
                if (await clickIfVisible(page, 'Add Blank Section')) {
                    await capture(page, '63-template-manager-add-blank-section-open');
                }
                if (await clickIfVisible(page, 'Skip Logic Rules')) {
                    await capture(page, '64-template-manager-skip-logic-open');
                }
                if (await clickIfVisible(page, 'Publish')) {
                    await capture(page, '65-template-manager-publish-confirm');
                    await closeDialog(page);
                }
            }
        }
    });

    await step('admin-settings-tabs-and-add-recipient', async () => {
        await gotoStable(page, '/audit-management/admin/settings');
        await capture(page, '70-admin-settings-email-tab');
        if (await clickIfVisible(page, 'Add Person')) {
            await capture(page, '71-admin-settings-add-recipient-dialog');
            await closeOverlay(page);
        }
        for (const label of ['Score Targets', 'User Roles']) {
            await page.getByText(label, { exact: true }).click({ force: true, timeout: 2_000 });
            await page.waitForTimeout(300);
            await capture(page, `72-admin-settings-${label.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`);
        }
    });

    await step('admin-users-dialogs', async () => {
        await gotoStable(page, '/audit-management/admin/users');
        await capture(page, '75-admin-users-base');
        if (await clickIfVisible(page, 'Add User')) {
            await capture(page, '76-admin-users-add-user-dialog');
            await closeDialog(page);
        }
        const edit = page.locator('.user-row-actions button:visible').first();
        if (await edit.count()) {
            await edit.click({ force: true });
            await capture(page, '77-admin-users-first-row-action-dialog');
            await closeDialog(page);
        }
    });

    await step('admin-audit-log-tabs', async () => {
        await gotoStable(page, '/audit-management/admin/audit-log');
        await capture(page, '78-admin-audit-log-action-tab');
        if (await clickIfVisible(page, 'Change Trail')) {
            await capture(page, '79-admin-audit-log-change-trail-tab');
        }
        const expander = page.locator('.p-row-toggler, button[aria-label="Expand Row"]').first();
        if (await expander.count()) {
            await expander.click({ force: true });
            await capture(page, '80-admin-audit-log-expanded-row');
        }
    });

    await step('report-composer-base-drafts-newsletter-blocks', async () => {
        await gotoStable(page, '/audit-management/reports/composer');
        await capture(page, '90-report-composer-base-empty');
        if (await clickIfVisible(page, 'Manage Drafts')) {
            await capture(page, '91-report-composer-manage-drafts');
            await closeDialog(page);
        }
        await selectFirstNativeOption(page, '[data-testid="composer-filter-division"]');
        const reportType = page.locator('select').nth(1);
        if (await reportType.count()) {
            await reportType.selectOption('newsletter');
            await page.waitForTimeout(300);
            await capture(page, '92-report-composer-newsletter-mode');
            if (await clickIfVisible(page, 'Newsletter Settings')) {
                await capture(page, '93-report-composer-newsletter-settings-panel');
            }
        }
        if (await clickIfVisible(page, 'Cover Page')) {
            await page.waitForTimeout(250);
            await capture(page, '94-report-composer-cover-block-added');
            const block = page.locator('[data-testid^="composer-block-"]').first();
            if (await block.count()) {
                await block.click({ force: true });
                await capture(page, '95-report-composer-cover-block-selected-properties');
            }
        }
    });

    await step('report-gallery-config', async () => {
        await gotoStable(page, '/audit-management/reports/gallery');
        await capture(page, '100-report-gallery-base');
        const card = page.locator('.template-card').first();
        if (await card.count()) {
            await card.click({ force: true });
            await capture(page, '101-report-gallery-config-dialog');
            await closeDialog(page);
        }
    });

    await step('scheduled-reports-dialog', async () => {
        await gotoStable(page, '/audit-management/reports/scheduled');
        await capture(page, '105-scheduled-reports-base');
        if (await clickIfVisible(page, 'New Schedule')) {
            await capture(page, '106-scheduled-reports-new-schedule-dialog');
            await closeDialog(page);
        }
    });

    await step('audits-by-employee', async () => {
        await gotoStable(page, '/audit-management/reports/by-employee');
        await capture(page, '110-audits-by-employee-base');
    });

    await step('newsletter-template-editor', async () => {
        await gotoStable(page, '/audit-management/newsletter/template-editor');
        await capture(page, '115-newsletter-template-editor-base');
    });

    await step('newsletter-and-quarterly-summary', async () => {
        await gotoStable(page, '/audit-management/newsletter');
        await capture(page, '120-newsletter-base');
        await gotoStable(page, '/audit-management/reports/quarterly-summary');
        await capture(page, '121-quarterly-summary-base');
    });

    await step('print-blank-form-route', async () => {
        const divisions = await health.json();
        const divisionId = Array.isArray(divisions) && divisions[0]?.id ? divisions[0].id : 1;
        await gotoStable(page, `/audit-management/print/${divisionId}`);
        await capture(page, '125-print-blank-form-route');
    });

    await step('responsive-key-pages', async () => {
        for (const viewport of [
            { name: 'tablet', width: 768, height: 1024 },
            { name: 'mobile', width: 390, height: 844 },
        ]) {
            await page.setViewportSize({ width: viewport.width, height: viewport.height });
            for (const route of [
                { path: '/audit-management/reports', name: 'reports' },
                { path: '/audit-management/audits', name: 'audits' },
                { path: '/audit-management/corrective-actions', name: 'corrective-actions' },
                { path: '/audit-management/reports/composer', name: 'composer' },
            ]) {
                await gotoStable(page, route.path);
                await capture(page, `130-${viewport.name}-${route.name}`);
            }
        }
    });

    fs.writeFileSync(path.join(sweepRoot, 'sweep-log.json'), JSON.stringify({
        apiBase,
        auditCount: audits.length,
        draftAuditId: draftAudit?.id ?? null,
        reviewAuditId: reviewAudit?.id ?? null,
        log,
        consoleErrors,
    }, null, 2));
});
