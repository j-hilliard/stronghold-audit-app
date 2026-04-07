# Release Readiness Checklist

## Gate Results
- [ ] Pre-change baseline evidence recorded.
- [ ] PR full gate passed.
- [ ] Pre-merge full gate rerun passed.
- [ ] Pre-release full gate rerun passed.

## Regression Coverage
- [ ] Smoke tests passed.
- [ ] Visual regression tests passed (manual diff review complete).
- [ ] Button contract coverage passed (strict scope).
- [ ] Core E2E workflow tests passed.
- [ ] Audit core flow tests passed (`new`, `in-progress navigation`, `submit path`).
- [ ] Template admin contract tests passed (drag/drop reorder, publish/versioning) or formally deferred.
- [ ] KPI/reporting contract tests passed (filtering, scope visibility, trend cards) or formally deferred.
- [ ] Live DB/integration gate passed (if required for release).
- [ ] Logging validation completed (`Verify-ProcessLogs.sql` or equivalent).

## Documentation
- [ ] Button coverage matrix updated.
- [ ] Defect log updated with open/closed status.
- [ ] Migration QA impact log updated.
- [ ] Diagram set updated if architecture/workflow changed.

## Sign-Off
- QA Owner:
- Date:
- Release Candidate:
- Decision: Approve / Reject
