namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A time-limited, unauthenticated access token for a single CorrectiveAction.
/// Allows external parties (contractors, subcontractors) to update CA status
/// without creating an application account.
///
/// Token is a GUID — unguessable, single-use per CA by convention.
/// Expires when the CA is closed OR after ExpiresAt (whichever is first).
/// </summary>
public class CaPublicToken
{
    public int Id { get; set; }
    public int CorrectiveActionId { get; set; }

    /// <summary>Secure random token (GUID without dashes) used in the public URL.</summary>
    public string Token { get; set; } = null!;

    /// <summary>Display name of who this link was sent to (audit record only, not enforced).</summary>
    public string? SentToName { get; set; }

    /// <summary>Email address the link was sent to (audit record only).</summary>
    public string? SentToEmail { get; set; }

    /// <summary>UTC timestamp when this token was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Created by (display name of the admin who generated this link).</summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>UTC expiry. Null = only expires when the CA is closed.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Manually revoked by an admin before expiry.</summary>
    public bool IsRevoked { get; set; }

    // Navigation
    public CorrectiveAction CorrectiveAction { get; set; } = null!;
}
