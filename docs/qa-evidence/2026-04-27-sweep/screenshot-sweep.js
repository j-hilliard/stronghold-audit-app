/**
 * Visual QA Screenshot Sweep — 2026-04-27
 * Run from repo root: node docs/qa-evidence/2026-04-27-sweep/screenshot-sweep.js
 */
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const BASE_URL = 'http://localhost:5173';
const OUT_DIR = path.join(__dirname, 'screenshots');
fs.mkdirSync(OUT_DIR, { recursive: true });

const PAGES = [
  { route: '/audit-management/reports',                slug: 'reports-dashboard' },
  { route: '/audit-management/audits',                 slug: 'audits-list' },
  { route: '/audit-management/audits/new',             slug: 'new-audit' },
  { route: '/audit-management/audits/1',               slug: 'audit-form-id1' },
  { route: '/audit-management/audits/1/review',        slug: 'audit-review-id1' },
  { route: '/audit-management/corrective-actions',     slug: 'corrective-actions' },
  { route: '/audit-management/admin/templates',        slug: 'admin-templates' },
  { route: '/audit-management/admin/settings',         slug: 'admin-settings' },
  { route: '/audit-management/admin/users',            slug: 'admin-users' },
  { route: '/audit-management/admin/audit-log',        slug: 'admin-audit-log' },
  { route: '/audit-management/reports/composer',       slug: 'report-composer' },
  { route: '/audit-management/reports/gallery',        slug: 'report-gallery' },
  { route: '/audit-management/reports/scheduled',      slug: 'scheduled-reports' },
  { route: '/audit-management/reports/by-employee',    slug: 'audits-by-employee' },
  { route: '/audit-management/newsletter/template-editor', slug: 'newsletter-template-editor' },
];

const VIEWPORTS = [
  { name: 'desktop', width: 1280, height: 720 },
  { name: 'tablet',  width: 768,  height: 1024 },
  { name: 'mobile',  width: 390,  height: 844 },
];

const MODES = ['dark', 'light'];

const CONSOLE_ERRORS = {};

async function setMode(page, mode) {
  await page.evaluate((m) => {
    const html = document.documentElement;
    if (m === 'dark') {
      html.classList.add('dark');
      html.classList.remove('light');
    } else {
      html.classList.remove('dark');
      html.classList.add('light');
    }
    try { localStorage.setItem('theme', m); } catch(e) {}
    try { localStorage.setItem('vueuse-color-scheme', m); } catch(e) {}
  }, mode);
  await page.waitForTimeout(300);
}

async function ensureAdminRole(page) {
  // Click the DevRoleSwitcher if it exists and select Administrator
  try {
    const switcher = await page.$('[data-testid="dev-role-switcher"], .dev-role-switcher, select[class*="dev"], select[class*="role"]');
    if (switcher) {
      await switcher.selectOption({ label: 'Administrator' });
      await page.waitForTimeout(300);
    }
  } catch (e) {
    // role switcher not present or not needed
  }
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const results = [];

  for (const vp of VIEWPORTS) {
    const context = await browser.newContext({
      viewport: { width: vp.width, height: vp.height },
      // Capture console errors per context
    });
    const page = await context.newPage();
    const pageErrors = [];

    page.on('console', msg => {
      if (msg.type() === 'error') {
        pageErrors.push({ text: msg.text(), location: msg.location() });
      }
    });
    page.on('pageerror', err => {
      pageErrors.push({ text: err.message, stack: err.stack });
    });

    // First navigate to set up auth/theme
    try {
      await page.goto(`${BASE_URL}/audit-management/reports`, {
        waitUntil: 'domcontentloaded',
        timeout: 20000,
      });
      await page.waitForTimeout(1500);
      await ensureAdminRole(page);
    } catch (e) {
      console.error('Initial navigation failed:', e.message);
    }

    for (const pg of PAGES) {
      for (const mode of MODES) {
        const filename = `${pg.slug}_${mode}_${vp.name}.png`;
        const filepath = path.join(OUT_DIR, filename);
        const consoleKey = `${pg.slug}_${mode}_${vp.name}`;

        try {
          await setMode(page, mode);
          await page.goto(`${BASE_URL}${pg.route}`, {
            waitUntil: 'networkidle',
            timeout: 20000,
          });
          await page.waitForTimeout(1200);

          // Collect any console errors on this page
          const errorsThisPage = [...pageErrors];
          pageErrors.length = 0;

          // Check for visible error text in the page
          const bodyText = await page.evaluate(() => document.body?.innerText ?? '');
          const hasVisibleError = /error|404|not found|undefined|null|exception/i.test(bodyText.slice(0, 500));

          await page.screenshot({
            path: filepath,
            fullPage: true,
            timeout: 15000,
          });

          const result = {
            page: pg.slug,
            route: pg.route,
            mode,
            viewport: vp.name,
            file: filename,
            status: 'ok',
            consoleErrors: errorsThisPage,
            hasVisibleError,
          };

          // Special checks
          if (pg.slug === 'new-audit') {
            const selectEl = await page.$('select');
            if (selectEl) {
              const optionCount = await page.evaluate(() => {
                const sel = document.querySelector('select');
                return sel ? sel.options.length : 0;
              });
              result.divisionPickerOptionCount = optionCount;
              result.divisionPickerNote = optionCount <= 1
                ? 'DEF-0001-CHECK: Only 1 option (disabled placeholder) — division list may be empty'
                : `OK: ${optionCount - 1} division(s) available`;
            }
          }

          if (pg.slug === 'report-composer') {
            const tiptapEl = await page.$('.ProseMirror, .rte-content, [class*="tiptap"]');
            result.tiptapPresent = !!tiptapEl;
            result.tiptapNote = tiptapEl ? 'OK: Tiptap editor found in DOM' : 'DEF-0010-CHECK: No Tiptap editor found';
          }

          if (pg.slug === 'reports-dashboard') {
            const kpiCards = await page.$$('[class*="kpi"], [class*="stat-card"], [data-testid*="kpi"]');
            result.kpiCardCount = kpiCards.length;
          }

          if (pg.slug === 'admin-templates') {
            const controls = await page.$$('select, button, input');
            result.templateManagerControls = controls.length;
            result.templateManagerNote = controls.length > 2
              ? `OK: ${controls.length} controls found`
              : 'DEF-0003-CHECK: Very few controls — may be placeholder';
          }

          results.push(result);
          console.log(`  OK  ${filename}`);
        } catch (e) {
          results.push({
            page: pg.slug,
            route: pg.route,
            mode,
            viewport: vp.name,
            file: filename,
            status: 'error',
            error: e.message,
          });
          console.error(`  ERR ${filename}: ${e.message}`);
        }
      }
    }

    await context.close();
  }

  // Write JSON report
  const reportPath = path.join(__dirname, 'sweep-results.json');
  fs.writeFileSync(reportPath, JSON.stringify(results, null, 2));
  console.log('\nResults written to:', reportPath);

  // Print summary
  const ok = results.filter(r => r.status === 'ok').length;
  const err = results.filter(r => r.status === 'error').length;
  const hasConsoleErrors = results.filter(r => r.consoleErrors && r.consoleErrors.length > 0);
  console.log(`\nSummary: ${ok} ok, ${err} errors, ${hasConsoleErrors.length} pages with console errors`);

  // Print defect checks
  console.log('\n--- Defect Checks ---');
  for (const r of results) {
    if (r.divisionPickerNote) console.log(`[new-audit] ${r.divisionPickerNote}`);
    if (r.tiptapNote) console.log(`[composer] ${r.tiptapNote}`);
    if (r.templateManagerNote) console.log(`[templates] ${r.templateManagerNote}`);
  }

  await browser.close();
  console.log('\nDone.');
})();
