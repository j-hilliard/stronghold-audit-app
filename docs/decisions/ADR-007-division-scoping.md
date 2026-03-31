# ADR-007: Division Scoping Enforced at the API Layer, Not Just the UI

**Status:** Accepted  
**Date:** 2026-03-31  

## Decision

Division-based data access restrictions are enforced in the API (CQRS query handlers and
EF Core global query filters), not just hidden in the frontend UI.

## Context

Nine divisions exist (TKIE, STS, STG, SHI, SHI_RT, SHI_RA, ETS, CSL, FACILITY).
An auditor assigned to TKIE must not be able to see STS audit data — these contain
HR-sensitive findings that are division-confidential.

## Reasoning

UI-only restrictions are not security. Any user who knows the API endpoint structure
can call `GET /api/v1/audits?divisionId=STS` directly (via browser dev tools, Postman, etc.)
and retrieve data they should not see if the restriction only exists in the navigation menu.

Enforcing at the API layer means:
- The `GetAuditListQuery` handler reads the calling user's assigned divisions from their
  claims and appends a WHERE clause — regardless of what divisionId was passed in the request
- EF Core global query filters on `Audit` entities automatically scope results to the user's divisions
- An `AuditManager` or `SystemAdmin` bypasses this filter and sees all divisions

## Consequences

- Every query handler that returns audit data must check user divisions
- The `userStore` must expose the user's assigned division codes
- Phase 2 (Azure AD): division assignments come from AD group membership
- Phase 1 (auth bypass): all divisions are accessible (dev mode)
- Frontend navigation hides irrelevant divisions as a UX convenience, not as a security control
