# Visual Approval Policy

## Rule
- Snapshot updates are never auto-approved.
- Any visual diff requires manual QA review before merge.

## Procedure
1. Run visual tests and collect snapshot diffs.
2. Inspect changed images in Playwright HTML report.
3. Confirm diff is expected and tied to an approved change.
4. Record approval note in PR comments.
5. Update snapshots only after approval.

## Reject Conditions
- Unexpected movement of controls or text.
- Regression in spacing/readability.
- Color/contrast regressions.
- Missing components or clipped content.
