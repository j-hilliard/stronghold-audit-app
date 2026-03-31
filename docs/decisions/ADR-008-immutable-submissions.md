# ADR-008: Submitted Audits Are Immutable Without an Explicit Logged Unlock

**Status:** Accepted  
**Date:** 2026-03-31  

## Decision

Once an audit's status is `Submitted`, no response, finding, or header field can be modified
except through an explicit "Reopen" action performed by an `AuditManager` or `SystemAdmin`,
which is logged to `ProcessLog`.

## Context

Compliance audits are HR and safety records. The integrity of findings and non-conformance
notes must be guaranteed. A finding that was recorded on-site must not be quietly modified
after the fact.

## Reasoning

- **Legal defensibility:** If a finding is later disputed ("that was never marked non-conforming"),
  the immutable record shows what was submitted, when, and by whom.
- **Prevents accidental edits:** A submitted audit appearing in a review should not be editable
  by the auditor who submitted it — they could be tempted to "fix" a finding before the manager sees it.
- **Audit trail for reopens:** If a legitimate correction is needed (e.g., a typo in a job number),
  the reopen action is logged with user, timestamp, and reason — so the change is transparent.
- **Score integrity:** Changing a response after submission would change the score retroactively.
  The submitted score is the official record.

## Implementation

- `SaveAuditResponsesCommand` checks `Audit.Status` before processing — rejects with 409 Conflict
  if status is not `Draft` or `Reopened`
- A `ReopenAuditCommand` handler (AuditManager+ only) transitions status to `Reopened` and writes
  a `ProcessLog` entry with reason
- After editing, a re-submit transitions back to `Submitted` (new `ProcessLog` entry)
- The UI shows submitted audits as read-only; the "Edit" button only appears for `AuditManager+`

## Consequences

- Auditors cannot edit their own submissions — by design
- All legitimate post-submission changes have a full audit trail
- The `ProcessLog` table (existing in the template) records every reopen and re-submit event
