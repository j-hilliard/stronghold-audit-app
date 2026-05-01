/**
 * Single source of truth for audit status transitions and allowed actions.
 * Mirrors AuditWorkflowPolicy.cs — keep both in sync.
 */

export const AuditStatus = {
    Draft:       'Draft',
    Submitted:   'Submitted',
    Reopened:    'Reopened',
    UnderReview: 'UnderReview',
    Approved:    'Approved',
    Distributed: 'Distributed',
    Closed:      'Closed',
} as const;

export type AuditStatusValue = typeof AuditStatus[keyof typeof AuditStatus];

export const CaStatus = {
    Open:       'Open',
    InProgress: 'InProgress',
    Submitted:  'Submitted',
    Closed:     'Closed',
    Voided:     'Voided',
    Overdue:    'Overdue',
} as const;

export type CaStatusValue = typeof CaStatus[keyof typeof CaStatus];

export const AuditWorkflowPolicy = {
    // Edit windows
    auditorEditableStatuses:  [AuditStatus.Draft, AuditStatus.Reopened] as const,
    reviewerEditableStatuses: [AuditStatus.Draft, AuditStatus.Reopened, AuditStatus.UnderReview] as const,

    // Workflow transition gates
    approvableStatuses:   [AuditStatus.UnderReview] as const,
    distributableStatuses:[AuditStatus.Approved, AuditStatus.Distributed] as const,
    reopenableStatuses:   [AuditStatus.Submitted, AuditStatus.UnderReview, AuditStatus.Approved, AuditStatus.Distributed, AuditStatus.Closed] as const,
    closableStatuses:     [AuditStatus.Approved, AuditStatus.Distributed] as const, // UI gate (backend allows more)
    reviewSummaryEditableStatuses: [AuditStatus.UnderReview, AuditStatus.Approved] as const,

    // Statuses where the form is locked (non-reviewer users)
    lockedStatuses: [AuditStatus.Submitted, AuditStatus.UnderReview, AuditStatus.Approved, AuditStatus.Distributed, AuditStatus.Closed] as const,

    // CA status helpers
    caOpenStatuses:     [CaStatus.Open, CaStatus.InProgress, CaStatus.Submitted, CaStatus.Overdue] as const,
    caTerminalStatuses: [CaStatus.Closed, CaStatus.Voided] as const,
} as const;

/** Severity tag for a given audit status (PrimeVue Tag severity). */
export function auditStatusSeverity(status: string): string {
    const map: Record<string, string> = {
        Draft:       'warning',
        Reopened:    'warning',
        Submitted:   'info',
        UnderReview: 'info',
        Approved:    'success',
        Distributed: 'success',
        Closed:      'secondary',
    };
    return map[status] ?? 'secondary';
}

/** Severity for a CA status badge. */
export function caStatusSeverity(status: string): string {
    const map: Record<string, string> = {
        Open:       'danger',
        InProgress: 'warning',
        Submitted:  'info',
        Overdue:    'danger',
        Closed:     'success',
        Voided:     'secondary',
    };
    return map[status] ?? 'secondary';
}
