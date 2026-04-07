namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Defines the input type for a question (e.g., StatusChoice, YesNo, Text, Number).
/// Each AuditQuestion has a ResponseTypeId.
///
/// Standard seeded types:
///   StatusChoice — Conforming / NonConforming / Warning / NA (primary audit mode)
///   YesNo        — Yes / No
///   YesNoNA      — Yes / No / NA
///   Text         — Free-text entry only
///   Number       — Numeric entry
///   Date         — Date picker
/// </summary>
public class ResponseType : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation
    public ICollection<ResponseOption> Options { get; set; } = new List<ResponseOption>();
    public ICollection<AuditQuestion> Questions { get; set; } = new List<AuditQuestion>();
}
