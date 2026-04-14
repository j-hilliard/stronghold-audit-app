# DEF-0009 Handoff: Report Composer Newsletter Template Bootstrap Returns 404

## Summary
- Area: Audit Management -> Reports -> Report Composer (`/audit-management/reports/composer`)
- Severity: Medium
- Symptom: Composer load triggers API `404` for newsletter template bootstrap request.
- API result: `GET /v1/audits/newsletter-template?divisionId={id} -> 404 Not Found`

## Reproduction (Validated)
1. Ensure API is running on `http://localhost:5221`.
2. Run:
   - `PW_REQUIRE_AUDIT_API=true`
   - `PLAYWRIGHT_API_BASE_URL=http://localhost:5221`
   - `npx playwright test tests/e2e/audit-report-composer-live-manage-drafts.spec.ts`
3. Open composer and trigger generation/save path.
4. Observe failing assertion with captured API errors:
   - `http://localhost:5221/v1/audits/newsletter-template?divisionId=8` (404, twice)

## Evidence
- Live E2E failure:
  - `webapp/tests/e2e/audit-report-composer-live-manage-drafts.spec.ts`
- Live guard suite status:
  - `audit-live-api-guard`: pass
  - `audit-live-blank-screen-guard`: pass
  - `audit-live-navigation-stress`: pass
  - `audit-report-composer-live-manage-drafts`: fail (this defect)
- Manual probe:
  - `GET http://localhost:5221/v1/audits/newsletter-template?divisionId=8` -> `404`

## Root Cause
`GetNewsletterTemplate` currently returns `NotFound()` when a division has no saved template row.  
The frontend treats missing template as a normal condition (`null` fallback), but browser still logs network `404` errors and strict live QA gate flags API 4xx.

Location:
- `Api/Controllers/AuditController.cs`
  - `GetNewsletterTemplate([FromQuery] int divisionId)`

## Recommended Fix
Change missing-template behavior from `404` to `200` with `null` payload.

Suggested controller behavior:
1. Keep auth/user checks.
2. Query template by division.
3. Return `Ok(result)` even when `result == null`.

Why this is better here:
- Missing template is an expected bootstrap state, not an exceptional error.
- Eliminates noisy console/network errors in normal composer usage.
- Aligns with existing frontend behavior and contract mocks (already using `200 null`).

## Regression Test Coverage
- Existing strict live test now captures API-level 4xx:
  - `webapp/tests/e2e/audit-report-composer-live-manage-drafts.spec.ts`
- Pass condition after fix:
  - no API `>=400` responses during composer bootstrap + save flow
  - no runtime errors

