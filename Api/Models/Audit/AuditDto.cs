namespace Stronghold.AppDashboard.Api.Models.Audit;

// ── List view ─────────────────────────────────────────────────────────────────

public class AuditListItemDto
{
    public int Id { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public string AuditType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? Auditor { get; set; }
    public string? AuditDate { get; set; }  // ISO date string from DateOnly
    public string? JobNumber { get; set; }
    public string? Location { get; set; }
}

// ── Full detail ───────────────────────────────────────────────────────────────

public class AuditDetailDto
{
    public int Id { get; set; }
    public int DivisionId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public int TemplateVersionId { get; set; }
    public string AuditType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime? SubmittedAt { get; set; }
    public AuditHeaderDto? Header { get; set; }
    public List<AuditResponseDto> Responses { get; set; } = new();
}

// ── Header ────────────────────────────────────────────────────────────────────

public class AuditHeaderDto
{
    public int? Id { get; set; }
    public string? JobNumber { get; set; }
    public string? Client { get; set; }
    public string? PM { get; set; }
    public string? Unit { get; set; }
    public string? Time { get; set; }
    public string? Shift { get; set; }
    public string? WorkDescription { get; set; }
    public string? Company1 { get; set; }
    public string? Company2 { get; set; }
    public string? Company3 { get; set; }
    public string? ResponsibleParty { get; set; }
    public string? Location { get; set; }

    /// <summary>ISO date string "YYYY-MM-DD" from the form date picker</summary>
    public string? AuditDate { get; set; }

    public string? Auditor { get; set; }
}

// ── Response ──────────────────────────────────────────────────────────────────

public class AuditResponseDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string QuestionTextSnapshot { get; set; } = null!;

    /// <summary>"Conforming" | "NonConforming" | "Warning" | "NA" | null (unanswered)</summary>
    public string? Status { get; set; }

    public string? Comment { get; set; }
    public bool CorrectedOnSite { get; set; }
}

// ── Create request ────────────────────────────────────────────────────────────

public class CreateAuditRequest
{
    public int DivisionId { get; set; }
}

// ── Save request ──────────────────────────────────────────────────────────────

public class SaveResponsesRequest
{
    public AuditHeaderDto? Header { get; set; }
    public List<AuditResponseUpsertDto> Responses { get; set; } = new();
}

public class AuditResponseUpsertDto
{
    public int QuestionId { get; set; }
    public string QuestionTextSnapshot { get; set; } = null!;
    public string? Status { get; set; }
    public string? Comment { get; set; }
    public bool CorrectedOnSite { get; set; }
}

// ── Review page ───────────────────────────────────────────────────────────────

public class AuditReviewDto
{
    public int Id { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public string AuditType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public AuditHeaderDto? Header { get; set; }
    public int ConformingCount { get; set; }
    public int NonConformingCount { get; set; }
    public int WarningCount { get; set; }
    public int NaCount { get; set; }
    public int UnansweredCount { get; set; }

    /// <summary>Conforming / (Conforming + NonConforming + Warning) * 100. Null if no scored items.</summary>
    public double? ScorePercent { get; set; }

    public List<AuditFindingDto> NonConformingItems { get; set; } = new();
    /// <summary>Warning responses (not corrected on-site) — from Responses, not Findings</summary>
    public List<ReviewResponseItemDto> WarningItems { get; set; } = new();
    /// <summary>All responses grouped by section — for full audit record / print view</summary>
    public List<ReviewSectionDto> Sections { get; set; } = new();
    public List<EmailRoutingDto> ReviewEmailRouting { get; set; } = new();
}

public class AuditFindingDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? Comment { get; set; }
    public bool CorrectedOnSite { get; set; }
    public List<CorrectiveActionDto> CorrectiveActions { get; set; } = new();
}

public class ReviewResponseItemDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    /// <summary>"Conforming" | "NonConforming" | "Warning" | "NA" | null</summary>
    public string? Status { get; set; }
    public string? Comment { get; set; }
    public bool CorrectedOnSite { get; set; }
    public int SortOrder { get; set; }
}

public class ReviewSectionDto
{
    public string SectionName { get; set; } = null!;
    public List<ReviewResponseItemDto> Items { get; set; } = new();
}

public class EmailRoutingDto
{
    public string EmailAddress { get; set; } = null!;
}

// ── Corrective Actions ────────────────────────────────────────────────────────

public class CorrectiveActionDto
{
    public int Id { get; set; }
    public int FindingId { get; set; }
    public int? AuditId { get; set; }
    public string Description { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public string? DueDate { get; set; }
    public string? CompletedDate { get; set; }
    /// <summary>"Open" | "InProgress" | "Closed"</summary>
    public string Status { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class AssignCorrectiveActionRequest
{
    public int FindingId { get; set; }
    public string Description { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public string? DueDate { get; set; }
}

public class CloseCorrectiveActionRequest
{
    public string Notes { get; set; } = null!;
    public string? CompletedDate { get; set; }
}

// ── Report / KPI ──────────────────────────────────────────────────────────────

public class AuditReportDto
{
    // KPI cards
    public int TotalAudits { get; set; }
    public double? AvgScorePercent { get; set; }
    public int TotalNonConforming { get; set; }
    public int TotalWarnings { get; set; }
    /// <summary>NC responses where the issue was corrected immediately on site.</summary>
    public int CorrectedOnSiteCount { get; set; }

    // Trend — one point per week (last 12 weeks)
    public List<AuditTrendPointDto> Trend { get; set; } = new();

    // Section-level NC breakdown
    public List<SectionNcBreakdownDto> SectionBreakdown { get; set; } = new();

    // Open corrective actions across filtered audits
    public List<OpenCorrectiveActionSummaryDto> OpenCorrectiveActions { get; set; } = new();

    // Table rows
    public List<AuditReportRowDto> Rows { get; set; } = new();
}

public class SectionTrendsReportDto
{
    /// <summary>Ordered quarter labels for chart X-axis, e.g. "2026 Q1"</summary>
    public List<string> Quarters { get; set; } = new();
    public List<SectionTrendDto> Sections { get; set; } = new();
}

public class SectionTrendDto
{
    public string SectionName { get; set; } = null!;
    /// <summary>Series for the selected division filter</summary>
    public List<SectionTrendPointDto> DivisionTrend { get; set; } = new();
    /// <summary>Series for all divisions combined</summary>
    public List<SectionTrendPointDto> CompanyTrend { get; set; } = new();
}

public class SectionTrendPointDto
{
    public string Quarter { get; set; } = null!;
    /// <summary>NC findings per audit for this section and quarter</summary>
    public double FindingsPerAudit { get; set; }
    public int AuditCount { get; set; }
    public int NcCount { get; set; }
}

public class SectionNcBreakdownDto
{
    public string SectionName { get; set; } = null!;
    public int NcCount { get; set; }
}

public class OpenCorrectiveActionSummaryDto
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public string Description { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public string? DueDate { get; set; }
    /// <summary>"Open" | "InProgress" | "Closed"</summary>
    public string Status { get; set; } = null!;
    public bool IsOverdue { get; set; }
    /// <summary>Calendar days since this CA was created.</summary>
    public int DaysOpen { get; set; }
}

public class AuditTrendPointDto
{
    /// <summary>ISO week label e.g. "2026-W13"</summary>
    public string Week { get; set; } = null!;
    public double? AvgScore { get; set; }
    public int AuditCount { get; set; }
}

public class AuditReportRowDto
{
    public int Id { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? AuditDate { get; set; }
    public string? Auditor { get; set; }
    public string? JobNumber { get; set; }
    public string? Location { get; set; }
    public double? ScorePercent { get; set; }
    public int NonConformingCount { get; set; }
    public int WarningCount { get; set; }
}

// ── Newsletter DTOs ────────────────────────────────────────────────────────────

public class NewsletterAiSummaryResult
{
    public bool Success { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class NewsletterSendResult
{
    public int Sent { get; set; }
    public bool DryRun { get; set; }
    public List<string> Recipients { get; set; } = new();
}

public class GenerateNewsletterSummaryRequest
{
    public string DivisionCode { get; set; } = null!;
    public int Quarter { get; set; }
    public int Year { get; set; }
    public double? AvgScore { get; set; }
    public int TotalAudits { get; set; }
    public int TotalNcs { get; set; }
    public List<SectionNcItemDto> TopSections { get; set; } = new();
    public int OpenCaCount { get; set; }
    public int OverdueCaCount { get; set; }
}

public class SectionNcItemDto
{
    public string SectionName { get; set; } = null!;
    public int NcCount { get; set; }
}

public class SendNewsletterRequest
{
    public int DivisionId { get; set; }
    public string Subject { get; set; } = null!;
    public string HtmlBody { get; set; } = null!;
}
