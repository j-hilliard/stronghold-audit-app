namespace Stronghold.AppDashboard.Api.Models.Audit;

// ── List item (for the draft picker) ─────────────────────────────────────────

public class ReportDraftListItemDto
{
    public int Id { get; set; }
    public int DivisionId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Period { get; set; } = null!;
    /// <summary>"YYYY-MM-DD" or null</summary>
    public string? DateFrom { get; set; }
    /// <summary>"YYYY-MM-DD" or null</summary>
    public string? DateTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
}

// ── Full draft (includes BlocksJson) ─────────────────────────────────────────

public class ReportDraftDto
{
    public int Id { get; set; }
    public int DivisionId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Period { get; set; } = null!;
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    /// <summary>Serialized ReportBlock[] JSON. Only useReportDraft.ts may deserialize this.</summary>
    public string BlocksJson { get; set; } = null!;
    /// <summary>Base64-encoded row version. Must be round-tripped on every PUT.</summary>
    public string RowVersion { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
}

// ── Create ────────────────────────────────────────────────────────────────────

public class CreateReportDraftRequest
{
    public int DivisionId { get; set; }
    public string Title { get; set; } = null!;
    public string Period { get; set; } = null!;
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    public string BlocksJson { get; set; } = "[]";
}

// ── Update ────────────────────────────────────────────────────────────────────

public class UpdateReportDraftRequest
{
    public string Title { get; set; } = null!;
    public string Period { get; set; } = null!;
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    public string BlocksJson { get; set; } = null!;
    /// <summary>Base64-encoded row version from the last GET. Required for optimistic concurrency.</summary>
    public string RowVersion { get; set; } = null!;
}
