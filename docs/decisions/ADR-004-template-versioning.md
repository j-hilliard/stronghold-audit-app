# ADR-004: Template Versioning — Audits Lock to Their Template Version

**Status:** Accepted  
**Date:** 2026-03-31  

## Decision

When an audit is created, it locks to the currently active `AuditTemplateVersion`.
That version never changes for the life of the audit, even if new versions are published.

## Context

Checklist questions change over time as safety standards evolve. Audits completed under the
old checklist must remain historically accurate — they cannot retroactively reflect questions
that did not exist when they were taken.

## Reasoning

- An audit is a legal and HR record. Its score and findings must reflect exactly what was
  asked at the time of the audit, not a later revision.
- If a question is removed from the template after 200 audits, those 200 audits still
  answered that question and their responses are valid records.
- If the scoring criteria change (e.g., a question becomes non-scoreable), existing audits
  must not have their scores retroactively recalculated.
- Regulators or HR may compare audits across time — this is only meaningful if the baseline
  checklist is consistent within each audit.

## Implementation

- `Audit.TemplateVersionId` is set at creation and is immutable
- The API enforces this: no endpoint allows changing `TemplateVersionId` after creation
- `AuditResponse.QuestionTextSnapshot` stores the exact question text at time of audit
  as a secondary protection — even if question text is later edited, historical responses
  show what was literally asked

## Consequences

- Admins cannot edit the live active version — they must clone → draft → publish
- Old audits always render correctly regardless of how many template versions follow
- Reports that compare audits across versions need to account for version differences
  (Phase 7 reports will expose version metadata)
