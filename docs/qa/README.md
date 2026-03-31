# QA Governance

This folder defines the QA-only operating model for the Stronghold Audit App.

## Core Documents
- `qa-operating-model.md`: Responsibility split, test gates, and release criteria.
- `button-coverage-matrix.md`: Button-by-button coverage map for shared shell and current module scope.
- `defect-log.md`: Defect tracking format with severity and evidence requirements.
- `migration-qa-impact-log.md`: QA review log for schema/migration impacts.
- `release-readiness-checklist.md`: Sign-off checklist for pre-merge and pre-release.
- `visual-approval-policy.md`: Manual snapshot-review policy.
- `claude-defect-handoff-template.md`: Problem + solution handoff format for Claude.

## Mermaid Diagrams
- `diagrams/test-architecture.md`
- `diagrams/workflow-swimlane.md`
- `diagrams/coverage-map.md`
- `diagrams/db-validation-map.md`

## Scripts
- `Scripts/qa/Invoke-QAGate.ps1`
- `Scripts/qa/Verify-ProcessLogs.sql`
