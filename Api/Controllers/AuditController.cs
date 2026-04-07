using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stronghold.AppDashboard.Api.Domain.Audit.Admin;
using Stronghold.AppDashboard.Api.Domain.Audit.Audits;
using Stronghold.AppDashboard.Api.Domain.Audit.Divisions;
using Stronghold.AppDashboard.Api.Domain.Audit.Templates;
using Stronghold.AppDashboard.Api.Helpers;
using Stronghold.AppDashboard.Api.Models.Audit;

namespace Stronghold.AppDashboard.Api.Controllers;

[Route(Constants.Routes.ApiTemplate)]
public class AuditController : V1ControllerBase
{
    public AuditController(IMediator mediator, ILogger<AuditController> logger)
        : base(mediator, logger) { }

    // ── Divisions ─────────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("divisions")]
    [ProducesResponseType(typeof(List<DivisionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DivisionDto>>> GetDivisions()
    {
        return await TryExecuteAsync<ActionResult<List<DivisionDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetDivisions()));
            },
            ex => Error<List<DivisionDto>>(ex)
        );
    }

    // ── Templates ─────────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("templates/active")]
    [ProducesResponseType(typeof(TemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateDto>> GetActiveTemplate([FromQuery] int divisionId)
    {
        return await TryExecuteAsync<ActionResult<TemplateDto>>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new GetActiveTemplate { DivisionId = divisionId });
                return result == null ? NotFound() : Ok(result);
            },
            ex => Error<TemplateDto>(ex)
        );
    }

    // ── Audits ────────────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits")]
    [ProducesResponseType(typeof(List<AuditListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditListItemDto>>> GetAuditList(
        [FromQuery] int? divisionId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string? auditor)
    {
        return await TryExecuteAsync<ActionResult<List<AuditListItemDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetAuditList
                {
                    DivisionId = divisionId,
                    Status = status,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Auditor = auditor
                }));
            },
            ex => Error<List<AuditListItemDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/report")]
    [ProducesResponseType(typeof(AuditReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditReportDto>> GetAuditReport(
        [FromQuery] int? divisionId,
        [FromQuery] string? status,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        return await TryExecuteAsync<ActionResult<AuditReportDto>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetAuditReport
                {
                    DivisionId = divisionId,
                    Status = status,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                }));
            },
            ex => Error<AuditReportDto>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/{id:int}")]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditDetailDto>> GetAudit([FromRoute] int id)
    {
        return await TryExecuteAsync<ActionResult<AuditDetailDto>>(
            async () =>
            {
                var user = await GetUser();
                var result = await Mediator.Send(new GetAudit { AuditId = id, RequestedBy = user.Email! });
                return result == null ? NotFound() : Ok(result);
            },
            ex => Error<AuditDetailDto>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> CreateAudit([FromBody] CreateAuditRequest body)
    {
        return await TryExecuteAsync<ActionResult<int>>(
            async () =>
            {
                var user = await GetUser();
                var auditId = await Mediator.Send(new CreateAudit
                {
                    DivisionId = body.DivisionId,
                    CreatedBy = user.Email!
                });
                return CreatedAtAction(nameof(GetAudit), new { id = auditId }, auditId);
            },
            ex => Error<int>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("audits/{id:int}/responses")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveAuditResponses([FromRoute] int id, [FromBody] SaveResponsesRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new SaveAuditResponses
                {
                    AuditId = id,
                    SavedBy = user.Email!,
                    Header = body.Header,
                    Responses = body.Responses
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitAudit([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new SubmitAudit { AuditId = id, SubmittedBy = user.Email! });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("audits/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAudit([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new DeleteAudit { AuditId = id, DeletedBy = user.Email! });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/{id:int}/review")]
    [ProducesResponseType(typeof(AuditReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditReviewDto>> GetAuditReview([FromRoute] int id)
    {
        return await TryExecuteAsync<ActionResult<AuditReviewDto>>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new GetAuditReview { AuditId = id });
                return result == null ? NotFound() : Ok(result);
            },
            ex => Error<AuditReviewDto>(ex)
        );
    }

    // ── Corrective Actions ────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/corrective-actions")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> AssignCorrectiveAction([FromBody] AssignCorrectiveActionRequest body)
    {
        return await TryExecuteAsync<ActionResult<int>>(
            async () =>
            {
                var user = await GetUser();
                var id = await Mediator.Send(new AssignCorrectiveAction
                {
                    Payload = body,
                    AssignedBy = user.Email!
                });
                return StatusCode(StatusCodes.Status201Created, id);
            },
            ex => ex is ArgumentException
                ? Task.FromResult<ActionResult<int>>(BadRequest(ex.Message))
                : Error<int>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("audits/corrective-actions/{id:int}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseCorrectiveAction([FromRoute] int id, [FromBody] CloseCorrectiveActionRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new CloseCorrectiveAction
                {
                    CorrectiveActionId = id,
                    Payload = body,
                    ClosedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException || ex is ArgumentException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    // ── Admin — Templates ─────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/templates")]
    [ProducesResponseType(typeof(List<TemplateVersionListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TemplateVersionListItemDto>>> GetTemplates()
    {
        return await TryExecuteAsync<ActionResult<List<TemplateVersionListItemDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetTemplates()));
            },
            ex => Error<List<TemplateVersionListItemDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("admin/templates/{versionId:int}/clone")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> CloneTemplateVersion([FromRoute] int versionId)
    {
        return await TryExecuteAsync<ActionResult<int>>(
            async () =>
            {
                var user = await GetUser();
                var newVersionId = await Mediator.Send(new CloneTemplateVersion
                {
                    VersionId = versionId,
                    ClonedBy = user.Email!
                });
                return CreatedAtAction(nameof(GetTemplates), new { }, newVersionId);
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<ActionResult<int>>(BadRequest(ex.Message))
                : Error<int>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/versions/{draftId:int}")]
    [ProducesResponseType(typeof(DraftVersionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DraftVersionDetailDto>> GetDraftVersionDetail([FromRoute] int draftId)
    {
        return await TryExecuteAsync<ActionResult<DraftVersionDetailDto>>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new GetDraftVersionDetail { DraftVersionId = draftId });
                return result == null ? NotFound() : Ok(result);
            },
            ex => Error<DraftVersionDetailDto>(ex)
        );
    }

    // ── Admin — Questions ─────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("admin/versions/{draftId:int}/questions")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> AddQuestion([FromRoute] int draftId, [FromBody] AddQuestionRequest body)
    {
        return await TryExecuteAsync<ActionResult<int>>(
            async () =>
            {
                var user = await GetUser();
                var vqId = await Mediator.Send(new AddQuestion
                {
                    DraftVersionId = draftId,
                    Payload = body,
                    AddedBy = user.Email!
                });
                return CreatedAtAction(nameof(GetTemplates), new { }, vqId);
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<ActionResult<int>>(BadRequest(ex.Message))
                : Error<int>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("admin/versions/{draftId:int}/questions/{versionQuestionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveQuestion([FromRoute] int draftId, [FromRoute] int versionQuestionId)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new RemoveQuestion
                {
                    DraftVersionId = draftId,
                    VersionQuestionId = versionQuestionId,
                    RemovedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/versions/{draftId:int}/questions/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderQuestions([FromRoute] int draftId, [FromBody] ReorderQuestionsRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new ReorderQuestions
                {
                    DraftVersionId = draftId,
                    Payload = body,
                    ReorderedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/versions/{draftId:int}/questions/{versionQuestionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuestion([FromRoute] int draftId, [FromRoute] int versionQuestionId, [FromBody] UpdateQuestionRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new UpdateQuestion
                {
                    DraftVersionId = draftId,
                    VersionQuestionId = versionQuestionId,
                    QuestionText = body.QuestionText,
                    UpdatedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException or ArgumentException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    // ── Admin — Section Library ───────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/section-library")]
    [ProducesResponseType(typeof(List<SectionLibraryItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SectionLibraryItemDto>>> GetSectionLibrary()
    {
        return await TryExecuteAsync<ActionResult<List<SectionLibraryItemDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetSectionLibrary()));
            },
            ex => Error<List<SectionLibraryItemDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("admin/versions/{draftId:int}/sections/copy")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CopySection([FromRoute] int draftId, [FromBody] CopySectionRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                var newId = await Mediator.Send(new CopySection
                {
                    DraftVersionId = draftId,
                    Payload = body,
                    CopiedBy = user.Email!
                });
                return CreatedAtAction(nameof(GetDraftVersionDetail), new { draftId }, newId);
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    // ── Admin — Sections ──────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("admin/versions/{draftId:int}/sections")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSection([FromRoute] int draftId, [FromBody] AddSectionRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                var newId = await Mediator.Send(new AddSection
                {
                    DraftVersionId = draftId,
                    Payload = body,
                    AddedBy = user.Email!
                });
                return CreatedAtAction(nameof(GetDraftVersionDetail), new { draftId }, newId);
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/versions/{draftId:int}/sections/{sectionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSection([FromRoute] int draftId, [FromRoute] int sectionId, [FromBody] UpdateSectionRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new UpdateSection
                {
                    DraftVersionId = draftId,
                    SectionId = sectionId,
                    Payload = body,
                    UpdatedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("admin/versions/{draftId:int}/sections/{sectionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveSection([FromRoute] int draftId, [FromRoute] int sectionId)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new RemoveSection
                {
                    DraftVersionId = draftId,
                    SectionId = sectionId,
                    RemovedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/versions/{draftId:int}/sections/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderSections([FromRoute] int draftId, [FromBody] ReorderSectionsRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new ReorderSections
                {
                    DraftVersionId = draftId,
                    Payload = body,
                    ReorderedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/versions/{draftId:int}/publish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishTemplateVersion([FromRoute] int draftId)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new PublishTemplateVersion
                {
                    DraftVersionId = draftId,
                    PublishedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/questions/archived")]
    [ProducesResponseType(typeof(List<ArchivedQuestionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ArchivedQuestionDto>>> GetArchivedQuestions()
    {
        return await TryExecuteAsync<ActionResult<List<ArchivedQuestionDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetArchivedQuestions()));
            },
            ex => Error<List<ArchivedQuestionDto>>(ex)
        );
    }

    // ── Admin — User Audit Roles ──────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/users/audit-roles")]
    [ProducesResponseType(typeof(List<UserAuditRoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserAuditRoleDto>>> GetUsersWithAuditRoles()
    {
        return await TryExecuteAsync<ActionResult<List<UserAuditRoleDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetUsersWithAuditRoles()));
            },
            ex => Error<List<UserAuditRoleDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/users/{userId:int}/audit-role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetUserAuditRole([FromRoute] int userId, [FromBody] SetUserAuditRoleRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                await Mediator.Send(new SetUserAuditRole { UserId = userId, RoleName = body.RoleName });
                return NoContent();
            },
            ex => ex is ArgumentException or InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    // ── Admin — Email Routing ─────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/email-routing")]
    [ProducesResponseType(typeof(List<EmailRoutingRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmailRoutingRuleDto>>> GetEmailRouting()
    {
        return await TryExecuteAsync<ActionResult<List<EmailRoutingRuleDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetEmailRouting()));
            },
            ex => Error<List<EmailRoutingRuleDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/email-routing")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEmailRouting([FromBody] UpdateEmailRoutingRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new UpdateEmailRouting
                {
                    Payload = body,
                    UpdatedBy = user.Email!
                });
                return NoContent();
            },
            ex => Error(ex)
        );
    }
}
