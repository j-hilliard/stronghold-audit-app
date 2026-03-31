# ADR-006: Soft Deletes — HR Data Is Never Hard Deleted

**Status:** Accepted  
**Date:** 2026-03-31  

## Decision

No `DELETE` SQL statements are ever executed on audit data tables.
Records are soft-deleted via `IsDeleted = 1`, `DeletedAt`, and `DeletedBy` columns.

## Context

This application handles HR-sensitive data: compliance findings, non-conformance notes,
corrective actions, and audit history that may reference individual employees or job performance.

## Reasoning

- **Legal and HR defensibility:** If a finding or corrective action is disputed, the original
  record must be retrievable. Hard deletion destroys evidence.
- **Audit trail integrity:** Knowing that a record was deleted, when, and by whom is itself
  important information — as important as the record's content.
- **Accident recovery:** Soft delete allows an admin to recover accidentally deleted records
  without a database restore.
- **Regulatory compliance:** Many compliance frameworks require data retention for audit records.
  Soft delete ensures data is never prematurely destroyed.

## Implementation

- Every table (except pure lookup/log tables) has `IsDeleted bit NOT NULL DEFAULT 0`,
  `DeletedAt datetime2 NULL`, `DeletedBy nvarchar NULL`
- EF Core global query filters are applied to all entities: `.HasQueryFilter(e => !e.IsDeleted)`
  — soft-deleted records are invisible to all normal queries
- A `SystemAdmin`-only endpoint can list and restore soft-deleted records if needed
- The API has no DELETE endpoints for audit data — only soft-delete actions

## Consequences

- Database storage grows over time (acceptable — audit records are not large)
- Reporting queries automatically exclude deleted records via EF Core global filters
- Hard deletion is only permitted for non-audit data (e.g., temp/test records by SystemAdmin via direct DB access)
