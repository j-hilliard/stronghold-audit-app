namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// In-app notification for audit lifecycle and CA events.
/// Type: "AuditSubmitted" | "AuditApproved" | "AuditDistributed" | "CaAssigned" | "CaCompleted"
/// </summary>
public class AppNotification
{
    public int Id { get; set; }

    /// <summary>Email address of the recipient user.</summary>
    public string RecipientEmail { get; set; } = null!;

    /// <summary>"AuditSubmitted" | "AuditApproved" | "AuditDistributed" | "CaAssigned" | "CaCompleted"</summary>
    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;

    /// <summary>"Audit" | "CorrectiveAction"</summary>
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }

    /// <summary>Frontend deep link, e.g. /audit-management/audits/{id}/review</summary>
    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
