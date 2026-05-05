/**
 * Contract drift guard for webapp/src/apiclient/auditClient.ts.
 *
 * Because the API client is handwritten (NSwag cannot run from the OneDrive path),
 * these tests call the live API and validate that each endpoint still returns the
 * fields the TypeScript interfaces declare. A failure here means a backend DTO
 * changed without the frontend client being updated.
 *
 * To regenerate the client: run Scripts/generate-api-client.ps1 (elevated PS, requires
 * the API to be running) and merge client.g.ts into auditClient.ts manually.
 *
 * Requires: ASPNETCORE_ENVIRONMENT=Local API running on $VITE_APP_API_BASE_URL.
 * Skip with: PW_REQUIRE_AUDIT_API=false
 */

import { APIRequestContext, test, expect } from '@playwright/test';

const requireLiveApi = process.env.PW_REQUIRE_AUDIT_API !== 'false';
const rawApiBase     = process.env.VITE_APP_API_BASE_URL || 'http://localhost:7221';
const apiBase        = rawApiBase.replace(/\/+$/, '');

async function getJson(request: APIRequestContext, path: string) {
    const res = await request.get(`${apiBase}${path}`, { failOnStatusCode: false });
    if (res.status() === 401 || res.status() === 403) return null; // auth-gated — skip shape check
    expect(res.status(), `GET ${path} status`).toBe(200);
    return res.json();
}

function assertHasFields(obj: Record<string, unknown>, fields: string[], label: string) {
    for (const field of fields) {
        expect(obj, `${label} missing field "${field}"`).toHaveProperty(field);
    }
}

test.describe('API client contract drift guard', () => {
    test.skip(!requireLiveApi, 'Skipped — set PW_REQUIRE_AUDIT_API=true to run live contract checks.');

    // ── DivisionDto ────────────────────────────────────────────────────────────
    test('GET /v1/divisions — DivisionDto shape', async ({ request }) => {
        const data = await getJson(request, '/v1/divisions');
        if (!data) return;
        expect(Array.isArray(data)).toBe(true);
        if (data.length > 0) {
            assertHasFields(data[0], ['id', 'code', 'name', 'auditType'], 'DivisionDto');
            expect(typeof data[0].id).toBe('number');
            expect(typeof data[0].code).toBe('string');
        }
    });

    // ── AuditListItemDto ───────────────────────────────────────────────────────
    test('GET /v1/audits — AuditListItemDto shape', async ({ request }) => {
        const data = await getJson(request, '/v1/audits');
        if (!data) return;
        expect(Array.isArray(data)).toBe(true);
        if (data.length > 0) {
            assertHasFields(data[0],
                ['id', 'status', 'divisionCode'],
                'AuditListItemDto',
            );
            expect(typeof data[0].id).toBe('number');
            expect(typeof data[0].status).toBe('string');
        }
    });

    // ── TemplateVersionListItemDto ─────────────────────────────────────────────
    test('GET /v1/admin/templates — TemplateVersionListItemDto shape', async ({ request }) => {
        const data = await getJson(request, '/v1/admin/templates');
        if (!data) return;
        expect(Array.isArray(data)).toBe(true);
        if (data.length > 0) {
            assertHasFields(data[0],
                ['versionId', 'versionNumber', 'divisionCode', 'isActive', 'publishedAt'],
                'TemplateVersionListItemDto',
            );
        }
    });

    // ── ComplianceStatusDto ────────────────────────────────────────────────────
    test('GET /v1/reports/compliance-status — ComplianceStatusDto shape', async ({ request }) => {
        const data = await getJson(request, '/v1/reports/compliance-status');
        if (!data) return;
        expect(Array.isArray(data)).toBe(true);
        if (data.length > 0) {
            assertHasFields(data[0],
                ['divisionId', 'divisionCode', 'totalAudits'],
                'ComplianceStatusDto',
            );
        }
    });

    // ── CorrectiveActionListItemDto ────────────────────────────────────────────
    test('GET /v1/corrective-actions — CorrectiveActionListItemDto shape', async ({ request }) => {
        const data = await getJson(request, '/v1/corrective-actions');
        if (!data) return;
        // Paged response: { items: [...], totalCount: N }
        if (data.items !== undefined) {
            expect(Array.isArray(data.items)).toBe(true);
            expect(typeof data.totalCount).toBe('number');
            if (data.items.length > 0) {
                assertHasFields(data.items[0],
                    ['id', 'auditId', 'status', 'assignedTo', 'dueDate'],
                    'CorrectiveActionListItemDto',
                );
            }
        } else {
            // fallback: flat array
            expect(Array.isArray(data)).toBe(true);
        }
    });

    // ── AuditReviewDto (first Submitted+ audit) ────────────────────────────────
    test('GET /v1/audits/:id/review — AuditReviewDto shape', async ({ request }) => {
        // Find a non-Draft audit to check the review DTO shape
        const list = await getJson(request, '/v1/audits');
        if (!list || !Array.isArray(list)) return;

        const reviewable = list.find((a: { status: string }) =>
            !['Draft', 'Reopened'].includes(a.status),
        );
        if (!reviewable) {
            test.skip(); // no reviewable audits in dev data
            return;
        }

        const data = await getJson(request, `/v1/audits/${reviewable.id}/review`);
        if (!data) return;

        assertHasFields(data,
            ['auditId', 'status', 'divisionCode', 'auditDate', 'sections'],
            'AuditReviewDto',
        );
        expect(Array.isArray(data.sections)).toBe(true);
    });

    // ── NotificationsResult ────────────────────────────────────────────────────
    test('GET /v1/notifications — NotificationsResult shape', async ({ request }) => {
        const data = await getJson(request, '/v1/notifications');
        if (!data) return;
        assertHasFields(data, ['items', 'unreadCount'], 'NotificationsResult');
        expect(Array.isArray(data.items)).toBe(true);
        expect(typeof data.unreadCount).toBe('number');
    });
});
