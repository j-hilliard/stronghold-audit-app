# 2026-04-16 QA Defect Handoff — Composer Runtime + Gate Reliability

## Scope
- Playwright baseline gate (`qa:baseline`)
- Composer contract gate (`audit-report-composer-contract.spec.ts`)
- Reporting/composer gate command portability on Windows

## Findings

### 1) High — Composer narrative editor runtime crash (release blocker)
- **Symptom:** Composer contract fails when loading narrative block editor; runtime throws module export error.
- **Observed error:** `The requested module '/node_modules/.vite/deps/@tiptap_extension-text-style.js?...' does not provide an export named 'default'`
- **Impact:** Narrative editing, persistence checks, and print/export checks become unreliable or fail.
- **Evidence:** `webapp/test-results/artifacts/audit-report-composer-cont-ebc2a-ith-authored-content-intact-chromium/error-context.md`

**Likely root cause**
- `RichTextEditor.vue` imports `TextStyle` as a default export:
  - `import TextStyle from '@tiptap/extension-text-style'`
- Current package version expects a named export.

**Proposed fix**
- Update import usage in [RichTextEditor.vue](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/src/modules/audit-management/features/reports/components/RichTextEditor.vue):
  - Use the export shape supported by installed version (`{ TextStyle }` vs default).
- Re-run composer contract gate after import fix.

---

### 2) Medium — Incident cleanup delete did not remove row in baseline flow
- **Symptom:** `incident-form.spec.ts` failed in baseline at delete verification (`expected count 0, got 1`).
- **Impact:** Baseline gate remains red and regression signal is noisy.
- **Evidence:** `tests/e2e/incident-form.spec.ts:137` in `npm run qa:baseline` output on 2026-04-16.

**Proposed fix**
- Verify delete endpoint actually commits before table reload, or add UI refresh after delete completion.
- Add API response assertion in test to distinguish UI-delay vs backend failure.

---

### 3) Medium — Windows gate command portability (fixed in QA scripts)
- **Previous symptom:** inline env var assignment in npm scripts failed on Windows (`'PW_AUDIT_REPORTING_GATE' is not recognized`).
- **QA fix applied:**
  - Removed inline env assignment from [webapp/package.json](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/package.json).
  - Added composer gate env wiring in [Invoke-QAGate.ps1](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/Scripts/qa/Invoke-QAGate.ps1).
  - Updated usage docs in [Scripts/qa/README.md](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/Scripts/qa/README.md).

## QA Test Updates Completed
- Strengthened composer contract mocks to behave like persisted state across PUT/GET.
- Added regression case for save -> navigate away -> return -> reload draft state.
- Updated narrative assertions to rich-text editor semantics (contenteditable vs textarea).
- Added async-safe print assertion (`expect.poll`) for `window.print()` invocation.

## Current Gate Status Snapshot
- `npm run test:e2e:audit:live-guard`: **PASS** (3/3)
- `npm run qa:baseline`: **FAIL** (incident delete flow)
- Composer contract gate: **FAIL** due runtime editor import defect (high)
