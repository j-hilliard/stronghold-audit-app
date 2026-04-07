namespace Stronghold.AppDashboard.Api.Models.Audit;

// ── Template management ───────────────────────────────────────────────────────

public class TemplateVersionListItemDto
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public int VersionNumber { get; set; }

    /// <summary>"Draft" | "Active" | "Superseded"</summary>
    public string Status { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }
    public int? ClonedFromVersionId { get; set; }
    public int QuestionCount { get; set; }
}

public class DraftVersionDetailDto
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public List<DraftSectionDto> Sections { get; set; } = new();
}

public class DraftSectionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? SectionCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public int? ReportingCategoryId { get; set; }
    public string? ReportingCategoryName { get; set; }
    public List<DraftQuestionDto> Questions { get; set; } = new();
}

public class DraftQuestionDto
{
    public int VersionQuestionId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? ShortLabel { get; set; }
    public string? HelpText { get; set; }
    public int DisplayOrder { get; set; }
    public bool AllowNA { get; set; }
    public bool RequireCommentOnNC { get; set; }
    public bool IsScoreable { get; set; }
    public bool IsArchived { get; set; }
    public int? ResponseTypeId { get; set; }
    public string? ResponseTypeCode { get; set; }
    /// <summary>Effective weight: version-level override if set, else question default.</summary>
    public decimal Weight { get; set; }
}

// ── Section library ───────────────────────────────────────────────────────────

public class SectionLibraryItemDto
{
    /// <summary>AuditSection.Id from the source active template version</summary>
    public int SectionId { get; set; }
    public string Name { get; set; } = null!;
    public string? SectionCode { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public int QuestionCount { get; set; }
}

public class CopySectionRequest
{
    /// <summary>Id of the source AuditSection to copy into this draft</summary>
    public int SourceSectionId { get; set; }
}

// ── Section requests ──────────────────────────────────────────────────────────

public class AddSectionRequest
{
    public string Name { get; set; } = null!;
}

public class UpdateSectionRequest
{
    public string Name { get; set; } = null!;
    public bool IsRequired { get; set; }
    public int? ReportingCategoryId { get; set; }
}

public class ReorderSectionsRequest
{
    /// <summary>Ordered list of section Id values — new display order is their list position</summary>
    public List<int> SectionIds { get; set; } = new();
}

// ── Add / reorder requests ────────────────────────────────────────────────────

public class AddQuestionRequest
{
    public int SectionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public bool AllowNA { get; set; } = true;
    public bool RequireCommentOnNC { get; set; } = true;
    public bool IsScoreable { get; set; } = true;
}

public class UpdateQuestionRequest
{
    public string QuestionText { get; set; } = null!;
}

public class ReorderQuestionsRequest
{
    /// <summary>Ordered list of VersionQuestionId values — new display order is their list position</summary>
    public List<int> VersionQuestionIds { get; set; } = new();
}

// ── Archived questions ────────────────────────────────────────────────────────

public class ArchivedQuestionDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public DateTime? ArchivedAt { get; set; }
    public string? ArchivedBy { get; set; }
}

// ── User audit roles ──────────────────────────────────────────────────────────

public class UserAuditRoleDto
{
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    /// <summary>Current audit role name, or null if the user has no audit role.</summary>
    public string? AuditRole { get; set; }
}

public class SetUserAuditRoleRequest
{
    /// <summary>Audit role name to assign, or null to remove all audit roles.</summary>
    public string? RoleName { get; set; }
}

// ── Email routing ─────────────────────────────────────────────────────────────

public class EmailRoutingRuleDto
{
    public int Id { get; set; }
    public int DivisionId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class UpdateEmailRoutingRequest
{
    public List<EmailRoutingRuleUpsertDto> Rules { get; set; } = new();
}

public class EmailRoutingRuleUpsertDto
{
    public int? Id { get; set; }
    public int DivisionId { get; set; }
    public string EmailAddress { get; set; } = null!;
    public bool IsActive { get; set; }
}
