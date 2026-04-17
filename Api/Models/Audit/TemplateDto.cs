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
    /// <summary>Section-level skip-logic rules evaluated client-side while filling the audit form.</summary>
    public List<LogicRuleDto> LogicRules { get; set; } = new();
}

public class LogicRuleDto
{
    public int Id { get; set; }
    public int TriggerVersionQuestionId { get; set; }
    /// <summary>"NonConforming" | "Conforming" | "Warning" | "NA" | "AnyAnswer"</summary>
    public string TriggerResponse { get; set; } = null!;
    /// <summary>"HideSection" | "ShowSection"</summary>
    public string Action { get; set; } = null!;
    public int? TargetSectionId { get; set; }
}

public class SaveLogicRuleRequest
{
    public int? Id { get; set; }
    public int TemplateVersionId { get; set; }
    public int TriggerVersionQuestionId { get; set; }
    public string TriggerResponse { get; set; } = null!;
    public string Action { get; set; } = null!;
    public int? TargetSectionId { get; set; }
}

public class TemplateSectionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsOptional { get; set; }
    public string? OptionalGroupKey { get; set; }
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
    /// <summary>When true a NonConforming answer auto-fails the whole audit.</summary>
    public bool IsLifeCritical { get; set; }
    /// <summary>When true, auditor must attach at least one photo when marking NonConforming.</summary>
    public bool RequirePhotoOnNc { get; set; }
    /// <summary>When true, a NonConforming response auto-creates a CorrectiveAction at submit time.</summary>
    public bool AutoCreateCa { get; set; }
}
