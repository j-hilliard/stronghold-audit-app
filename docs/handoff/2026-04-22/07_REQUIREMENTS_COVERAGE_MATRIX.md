# 07 - Requirements Coverage Matrix
**Purpose:** Fast visibility into requirement completion vs outstanding work.

## A. Template Engine Requirements (`docs/requirements/audit-template-engine-requirements.md`)
| Requirement | Description | Status | Notes |
|---|---|---|---|
| R-001 | Versioned template engine | Implemented | Draft/publish/clone paths present |
| R-002 | Section/question admin and reorder | Implemented | Drag/drop + admin handlers present |
| R-003 | Stable reporting taxonomy | Partial | Requires continued reporting contract validation |
| R-004 | Audit version lock | Implemented | Snapshot model in place |
| R-005 | Response/rule behavior | Implemented | Conforming/NC/Warning/NA and rule flags present |
| R-006 | Snapshot integrity | Implemented | Snapshot fields stored on responses |
| R-007 | Role + scope security | Partial | Framework present; full matrix verification still required |
| R-008 | Role set | Partial | Roles exist; some UX and assignment governance still evolving |
| R-009 | KPI/reporting dashboard | Partial | Rich reporting exists; UX clarity improvements still needed |
| R-010 | Admin isolation | Implemented | Admin screens and routes are separate |
| R-011 | Workflow baseline | Implemented | Draft/submit/review/print/email paths present |
| R-012 | Audit logging | Implemented | Action log + audit trail present |
| R-013 | Composer sticky side rails | Partial | Implemented in part, requires usability hardening |
| R-014 | Rich text controls | Implemented | Rich text components and formatting controls present |
| R-015 | Composer usability baseline | Partial | Needs further simplification and stronger save/feedback ergonomics |
| R-016 | Layout customization engine | Partial | Current block model exists; advanced zone/page-template model remains open |

## B. Business Requirement List (Role/Scoring/Sections/etc.)
| Requirement | Status | Current Read |
|---|---|---|
| Access controls and role views | Partial | Built, needs complete verification matrix |
| Weighted scoring by topic/group | Implemented | Snapshot weights + weighted calculations present |
| Optional sections/toggles | Partial | Data model and handlers present, UX acceptance pass still needed |
| Attachments upload | Implemented | Endpoints + UI present |
| CA due reminder emails (3-day) | Implemented | Background service present |
| Submission auto-email recipients with link | Implemented/Partial | Logic present; content/operational validation pending |
| Mobile view | Partial | Needs dedicated pass and remediation |
| Auto-generated reports and easier composer | Partial | Exists; still needs usability improvements |
| Excel export outputs | Implemented | Parity verification still needed |

## C. Explicitly Deferred
| Item | Decision |
|---|---|
| E-Charts integration | Deferred to final phase until access is granted |

## D. What Must Be Proven Next (Not Assumed)
1. role/scope restrictions in live routes and direct API access
2. phase2c contract suite fully green in stable environment
3. dashboard KPI labels/numbers trust under all filter combinations
4. composer print/export visual parity and chart reliability

