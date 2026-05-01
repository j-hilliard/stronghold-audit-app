namespace Stronghold.AppDashboard.Api.Domain.Audit;

/// <summary>
/// Single source of truth for audit status transitions and allowed actions.
/// Use these constants instead of scattering string arrays across handlers and controllers.
/// </summary>
public static class AuditWorkflowPolicy
{
    // ── Status constants ──────────────────────────────────────────────────────

    public const string Draft       = "Draft";
    public const string Submitted   = "Submitted";
    public const string Reopened    = "Reopened";
    public const string UnderReview = "UnderReview";
    public const string Approved    = "Approved";
    public const string Distributed = "Distributed";
    public const string Closed      = "Closed";

    // ── Edit windows ──────────────────────────────────────────────────────────

    /// <summary>Statuses where an auditor-only user may save responses.</summary>
    public static readonly string[] AuditorEditableStatuses = [Draft, Reopened];

    /// <summary>Statuses where an admin/reviewer may save responses.</summary>
    public static readonly string[] ReviewerEditableStatuses = [Draft, Reopened, UnderReview];

    // ── Workflow transition gates ─────────────────────────────────────────────

    /// <summary>Statuses from which StartReview is valid.</summary>
    public static readonly string[] ReviewableStatuses = [Submitted];

    /// <summary>Statuses from which ApproveAudit is valid.</summary>
    public static readonly string[] ApprovableStatuses = [UnderReview];

    /// <summary>Statuses from which SendDistributionEmail is valid.</summary>
    public static readonly string[] DistributableStatuses = [Approved, Distributed];

    /// <summary>Statuses from which ReopenAudit is valid.</summary>
    public static readonly string[] ReopenableStatuses = [Submitted, UnderReview, Approved, Distributed, Closed];

    /// <summary>Statuses from which CloseAudit is valid (only after workflow completes).</summary>
    public static readonly string[] ClosableStatuses = [Approved, Distributed];

    /// <summary>Statuses where SaveReviewSummary is valid.</summary>
    public static readonly string[] ReviewSummaryEditableStatuses = [UnderReview, Approved];

    // ── CA status constants ───────────────────────────────────────────────────

    public const string CaOpen       = "Open";
    public const string CaInProgress = "InProgress";
    public const string CaSubmitted  = "Submitted";
    public const string CaClosed     = "Closed";
    public const string CaVoided     = "Voided";
    public const string CaOverdue    = "Overdue";

    /// <summary>CA statuses that are considered "resolved" (closed-out).</summary>
    public static readonly string[] CaTerminalStatuses = [CaClosed, CaVoided];

    /// <summary>CA statuses that are considered "open" for distribution filtering.</summary>
    public static readonly string[] CaOpenStatuses = [CaOpen, CaInProgress, CaSubmitted, CaOverdue];

    /// <summary>External assignees may set these statuses via public token.</summary>
    public static readonly string[] CaExternalAllowedStatuses = [CaInProgress, CaSubmitted];
}
