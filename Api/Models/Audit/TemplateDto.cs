namespace Stronghold.AppDashboard.Api.Models.Audit;

/// <summary>
/// The active checklist template for a division, structured for the audit form.
/// </summary>
public class TemplateDto
{
    public int VersionId { get; set; }
    public int VersionNumber { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public string AuditType { get; set; } = null!;
    public List<TemplateSectionDto> Sections { get; set; } = new();
}

public class TemplateSectionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public List<TemplateQuestionDto> Questions { get; set; } = new();
}

public class TemplateQuestionDto
{
    /// <summary>AuditVersionQuestion.Id — used when saving responses</summary>
    public int VersionQuestionId { get; set; }

    /// <summary>AuditQuestion.Id — stored on AuditResponse for history</summary>
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool AllowNA { get; set; }
    public bool RequireCommentOnNC { get; set; }
    public bool IsScoreable { get; set; }
}
