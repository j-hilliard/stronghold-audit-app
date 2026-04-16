namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Defines a conditional skip-logic rule on a template version.
/// Phase 1 scope: section-level show/hide only.
///
/// Rule semantics: When <see cref="TriggerVersionQuestionId"/> is answered with
/// <see cref="TriggerResponse"/>, apply <see cref="Action"/> to <see cref="TargetSectionId"/>.
/// </summary>
public class QuestionLogicRule
{
    public int Id { get; set; }
    public int TemplateVersionId { get; set; }

    /// <summary>AuditVersionQuestion.Id — the question whose answer triggers this rule.</summary>
    public int TriggerVersionQuestionId { get; set; }

    /// <summary>
    /// The response value that fires the rule.
    /// Allowed values: "NonConforming" | "Conforming" | "Warning" | "NA" | "AnyAnswer"
    /// </summary>
    public string TriggerResponse { get; set; } = null!;

    /// <summary>
    /// Effect to apply when triggered.
    /// Phase 1: "HideSection" | "ShowSection"
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>The section to show or hide. Required for HideSection/ShowSection actions.</summary>
    public int? TargetSectionId { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public AuditTemplateVersion TemplateVersion { get; set; } = null!;
    public AuditVersionQuestion TriggerVersionQuestion { get; set; } = null!;
    public AuditSection? TargetSection { get; set; }
}
