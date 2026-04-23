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
    public string? TrackingNumber { get; set; }
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

    /// <summary>Optional section group keys enabled at creation (immutable).</summary>
    public List<string> EnabledOptionalGroupKeys { get; set; } = new();
    public string? TrackingNumber { get; set; }
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
    public string? SiteCode { get; set; }
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

    /// <summary>
    /// Optional section groups to enable for this audit (e.g. ["RADIOGRAPHY", "ROPE_ACCESS"]).
    /// Sections whose OptionalGroupKey matches are included; unmatched optional sections are excluded.
    /// </summary>
    public List<string> EnabledOptionalGroupKeys { get; set; } = new();

    /// <summary>
    /// FK to DivisionJobPrefix. Required when division has multiple prefixes (e.g. ETS: H vs B).
    /// Null = use the division's default prefix.
    /// </summary>
    public int? JobPrefixId { get; set; }

    /// <summary>3-char site code for the tracking number suffix (e.g. "IPT" = INEOS Pasadena TX). Optional.</summary>
    public string? SiteCode { get; set; }
}

// ── Division job prefix ────────────────────────────────────────────────────────

public class DivisionJobPrefixDto
{
    public int Id { get; set; }
    public string Prefix { get; set; } = "";
    public string Label { get; set; } = null!;
    public bool IsDefault { get; set; }
}

public class SaveDivisionJobPrefixesRequest
{
    public List<DivisionJobPrefixUpsertDto> Prefixes { get; set; } = new();
}

public class DivisionJobPrefixUpsertDto
{
    public string Prefix { get; set; } = "";
    public string Label { get; set; } = null!;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
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

    /// <summary>
    /// True if any Life Critical question was answered NonConforming.
    /// When true the audit is considered an automatic fail regardless of score.
    /// </summary>
    public bool HasLifeCriticalFailure { get; set; }

    /// <summary>Question texts for every life-critical NC in this audit.</summary>
    public List<string> LifeCriticalFailures { get; set; } = new();

    /// <summary>AI-generated plain-language summary, null when not available.</summary>
    public string? AiSummary { get; set; }

    /// <summary>Average score across the last 10 submitted audits for this division. Null if insufficient data.</summary>
    public double? DivisionAvgScore { get; set; }

    /// <summary>Division compliance target (0–100), null if not set by admin.</summary>
    public decimal? DivisionScoreTarget { get; set; }

    /// <summary>IDs of questions that are repeat findings (NonConforming in 2+ of the last 180 days).</summary>
    public List<int> RepeatFindingQuestionIds { get; set; } = new();

    public List<AuditFindingDto> NonConformingItems { get; set; } = new();
    /// <summary>Warning responses (not corrected on-site) — from Responses, not Findings</summary>
    public List<ReviewResponseItemDto> WarningItems { get; set; } = new();
    /// <summary>All responses grouped by section — for full audit record / print view</summary>
    public List<ReviewSectionDto> Sections { get; set; } = new();
    public List<EmailRoutingDto> ReviewEmailRouting { get; set; } = new();

    public string? TrackingNumber { get; set; }

    // ── Review / Distribution fields ─────────────────────────────────────────
    public string? ReviewSummary { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public List<DistributionRecipientDto> DistributionRecipients { get; set; } = new();

    /// <summary>Attachments available for selection when sending the distribution email.</summary>
    public List<AuditAttachmentRefDto> Attachments { get; set; } = new();
}

public class DistributionRecipientDto
{
    public int Id { get; set; }
    public string EmailAddress { get; set; } = null!;
    public string? Name { get; set; }
}

public class AuditAttachmentRefDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public bool HasFile { get; set; }
}

public class SaveReviewSummaryRequest
{
    public string? Summary { get; set; }
}

public class AddDistributionRecipientRequest
{
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
}

public class SendDistributionEmailRequest
{
    /// <summary>IDs of AuditAttachments to include as SMTP attachments in the distribution email.</summary>
    public List<int> AttachmentIds { get; set; } = new();
    /// <summary>Optional override for the email subject line.</summary>
    public string? SubjectOverride { get; set; }
}

public class AuditFindingDto
{
    public int Id { get; set; }
    /// <summary>AuditQuestion.Id — used to cross-reference repeat finding badges.</summary>
    public int QuestionId { get; set; }
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
    /// <summary>Nullable — auto-generated CAs may not be linked to a specific finding.</summary>
    public int? FindingId { get; set; }
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
    /// <summary>"Normal" (default) or "Urgent". Drives SLA-based due date when DueDate is not supplied.</summary>
    public string Priority { get; set; } = "Normal";
    public string? RootCause { get; set; }
}

public class CloseCorrectiveActionRequest
{
    public string Notes { get; set; } = null!;
    public string? CompletedDate { get; set; }
    public string? RootCause { get; set; }
}

public class UpdateCorrectiveActionRequest
{
    public string? Description      { get; set; }
    public string? AssignedTo       { get; set; }
    public int?    AssignedToUserId { get; set; }
    /// <summary>ISO "YYYY-MM-DD". Send empty string to clear the due date.</summary>
    public string? DueDate          { get; set; }
    public string? RootCause        { get; set; }
}

public class BulkUpdateCorrectiveActionsRequest
{
    public List<int> CorrectiveActionIds { get; set; } = new();
    /// <summary>"status" | "reassign"</summary>
    public string Action { get; set; } = null!;
    /// <summary>For action="status": "InProgress" | "Closed" | "Voided"</summary>
    public string? NewStatus { get; set; }
    /// <summary>Required when NewStatus="Closed"</summary>
    public string? ClosureNotes { get; set; }
    /// <summary>For action="reassign"</summary>
    public string? NewAssignee { get; set; }
    public int?    NewAssigneeUserId { get; set; }
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
