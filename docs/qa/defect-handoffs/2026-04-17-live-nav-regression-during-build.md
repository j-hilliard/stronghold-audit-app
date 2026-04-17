# Active Regression During Build (2026-04-17)

## Command
- `cmd /c npm run test:e2e:audit:live-guard`

## Result
- Failed: `tests/e2e/audit-live-navigation-stress.spec.ts`

## Failing assertion
- File: [audit-live-navigation-stress.spec.ts:27](/c:/Users/joseph.hilliard/OneDrive%20-%20Quanta%20Services%20Management%20Partnership,%20L.P/Desktop/Stronghold%20Audit%20App/webapp/tests/e2e/audit-live-navigation-stress.spec.ts:27)
- Expected URL: `/audit-management/audits/new`
- Actual URL: `/audit-management/audits`

Error snippet:
```text
Expected pattern: /\/audit-management\/audits\/new$/
Received string:  "http://127.0.0.1:7308/audit-management/audits"
```

## Additional instability
- `cmd /c npm run test:e2e:core` failed with:
```text
Error: spawn EPERM
```

## QA interpretation
- App behavior is still changing under active development.
- Route behavior for New Audit is not stable under live stress flow.
- Test runner process stability is intermittent and still needs hardening.
