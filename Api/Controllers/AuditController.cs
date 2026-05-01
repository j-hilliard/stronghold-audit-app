using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stronghold.AppDashboard.Api.Domain.Audit.Admin;
using Stronghold.AppDashboard.Api.Domain.Audit.Audits;
using Stronghold.AppDashboard.Api.Domain.Audit.Divisions;
using Stronghold.AppDashboard.Api.Domain.Audit.Newsletter;
using Stronghold.AppDashboard.Api.Domain.Audit.ReportDrafts;
using Stronghold.AppDashboard.Api.Domain.Audit.Export;
using Stronghold.AppDashboard.Api.Domain.Audit.Reports;
using Stronghold.AppDashboard.Api.Domain.Audit.Templates;
using Stronghold.AppDashboard.Api.Helpers;
using Stronghold.AppDashboard.Api.Models.Audit;

namespace Stronghold.AppDashboard.Api.Controllers;

public record ReopenAuditRequest(string? Reason);
public record CloseAuditRequest(string? Notes);
public record ApproveAuditRequest(string? Notes);
public record SetScoreTargetRequest(decimal? ScoreTarget);
public record SetAuditFrequencyRequest(int? AuditFrequencyDays);
public record SetRequireClosurePhotoRequest(bool RequireClosurePhoto);
public record SetDivisionSlaRequest(int? SlaNormalDays, int? SlaUrgentDays, int? SlaEscalateAfterDays, string? EscalationEmail);

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

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("divisions/{id:int}/job-prefixes")]
    [ProducesResponseType(typeof(List<DivisionJobPrefixDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DivisionJobPrefixDto>>> GetDivisionJobPrefixes([FromRoute] int id)
    {
        return await TryExecuteAsync<ActionResult<List<DivisionJobPrefixDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetDivisionJobPrefixes { DivisionId = id }));
            },
            ex => Error<List<DivisionJobPrefixDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/divisions/{id:int}/job-prefixes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SaveDivisionJobPrefixes([FromRoute] int id, [FromBody] SaveDivisionJobPrefixesRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                await Mediator.Send(new SaveDivisionJobPrefixes { DivisionId = id, Prefixes = body.Prefixes });
                return NoContent();
            },
            ex => Error(ex)
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
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? sectionFilter)
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
                    SectionFilter = sectionFilter,
                }));
            },
            ex => Error<AuditReportDto>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/section-trends")]
    [ProducesResponseType(typeof(SectionTrendsReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SectionTrendsReportDto>> GetSectionTrends(
        [FromQuery] int? divisionId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        return await TryExecuteAsync<ActionResult<SectionTrendsReportDto>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetSectionTrends
                {
                    DivisionId = divisionId,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                }));
            },
            ex => Error<SectionTrendsReportDto>(ex)
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
                    CreatedBy = user.Email!,
                    EnabledOptionalGroupKeys = body.EnabledOptionalGroupKeys,
                    JobPrefixId = body.JobPrefixId,
                    SiteCode = body.SiteCode
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
                    Responses = body.Responses,
                    SectionNaOverrides = body.SectionNaOverrides
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
                var result = await Mediator.Send(new SubmitAudit { AuditId = id, SubmittedBy = user.Email! });
                return Ok(result);
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReopenAudit([FromRoute] int id, [FromBody] ReopenAuditRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new ReopenAudit { AuditId = id, ReopenedBy = user.Email!, Reason = body.Reason });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloseAudit([FromRoute] int id, [FromBody] CloseAuditRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new CloseAudit { AuditId = id, ClosedBy = user.Email!, Notes = body.Notes });
                return NoContent();
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartAuditReview([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new StartReview { AuditId = id, ReviewStartedBy = user.Email! });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : ex is InvalidOperationException
                    ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                    : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveAuditAction([FromRoute] int id, [FromBody] ApproveAuditRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new ApproveAudit { AuditId = id, ApprovedBy = user.Email! });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : ex is InvalidOperationException
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
    [HttpPut("audits/{id:int}/review-summary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveReviewSummary([FromRoute] int id, [FromBody] SaveReviewSummaryRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                await Mediator.Send(new SaveReviewSummary { AuditId = id, Summary = body.Summary });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/distribution-recipients")]
    [ProducesResponseType(typeof(DistributionRecipientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddDistributionRecipient([FromRoute] int id, [FromBody] AddDistributionRecipientRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                if (string.IsNullOrWhiteSpace(body.Email))
                    return BadRequest("Email is required.");
                var result = await Mediator.Send(new AddDistributionRecipient
                {
                    AuditId = id,
                    Email = body.Email,
                    Name = body.Name,
                    AddedBy = user.Email!,
                });
                return Created(string.Empty, result);
            },
            ex => Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("audits/{id:int}/distribution-recipients/{recipientId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDistributionRecipient([FromRoute] int id, [FromRoute] int recipientId)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                await Mediator.Send(new RemoveDistributionRecipient { AuditId = id, RecipientId = recipientId });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/{id:int}/distribution-preview")]
    [ProducesResponseType(typeof(DistributionPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DistributionPreviewDto>> GetDistributionPreview([FromRoute] int id, [FromQuery] string? attachmentIds)
    {
        return await TryExecuteAsync<ActionResult<DistributionPreviewDto>>(
            async () =>
            {
                await GetUser();
                var ids = attachmentIds?.Split(',').Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n > 0).ToList() ?? new List<int>();
                var result = await Mediator.Send(new GetDistributionPreview { AuditId = id, AttachmentIds = ids });
                return Ok(result);
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<ActionResult<DistributionPreviewDto>>(NotFound(ex.Message))
                : Error<DistributionPreviewDto>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/send-distribution")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendDistributionEmail([FromRoute] int id, [FromBody] SendDistributionEmailRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new SendDistributionEmail
                {
                    AuditId                  = id,
                    AttachmentIds            = body.AttachmentIds,
                    SubjectOverride          = body.SubjectOverride,
                    IncludeCorrectiveActions = body.IncludeCorrectiveActions,
                    IncludeOpenCasOnly       = body.IncludeOpenCasOnly,
                    Message                  = body.Message,
                    SentBy                   = user.Email!,
                });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
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
    [HttpGet("audits/corrective-actions")]
    [ProducesResponseType(typeof(PagedCorrectiveActionsResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedCorrectiveActionsResult>> GetCorrectiveActions(
        [FromQuery] int?    divisionId,
        [FromQuery] string? status,
        [FromQuery] string? searchText,
        [FromQuery] string? assignedTo,
        [FromQuery] string? source,
        [FromQuery] string? priority,
        [FromQuery] bool    overdueOnly  = false,
        [FromQuery] int     pageNumber   = 1,
        [FromQuery] int     pageSize     = 25)
    {
        return await TryExecuteAsync<ActionResult<PagedCorrectiveActionsResult>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetCorrectiveActions
                {
                    DivisionId  = divisionId,
                    Status      = status,
                    SearchText  = searchText,
                    AssignedTo  = assignedTo,
                    Source      = source,
                    Priority    = priority,
                    OverdueOnly = overdueOnly,
                    PageNumber  = pageNumber,
                    PageSize    = pageSize,
                }));
            },
            ex => Error<PagedCorrectiveActionsResult>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("audits/corrective-actions/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCorrectiveAction([FromRoute] int id, [FromBody] UpdateCorrectiveActionRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new UpdateCorrectiveAction
                {
                    CorrectiveActionId = id,
                    Description        = body.Description,
                    AssignedTo         = body.AssignedTo,
                    AssignedToUserId   = body.AssignedToUserId,
                    DueDate            = body.DueDate,
                    RootCause          = body.RootCause,
                    UpdatedBy          = user.Email!,
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(ex is KeyNotFoundException ? NotFound(ex.Message) : BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/corrective-actions/bulk")]
    [ProducesResponseType(typeof(BulkUpdateCorrectiveActionsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkUpdateCorrectiveActionsResult>> BulkUpdateCorrectiveActions(
        [FromBody] BulkUpdateCorrectiveActionsRequest body)
    {
        return await TryExecuteAsync<ActionResult<BulkUpdateCorrectiveActionsResult>>(
            async () =>
            {
                var user = await GetUser();
                var result = await Mediator.Send(new BulkUpdateCorrectiveActions
                {
                    CorrectiveActionIds = body.CorrectiveActionIds,
                    Action              = body.Action,
                    NewStatus           = body.NewStatus,
                    ClosureNotes        = body.ClosureNotes,
                    NewAssignee         = body.NewAssignee,
                    NewAssigneeUserId   = body.NewAssigneeUserId,
                    UpdatedBy           = user.Email!,
                });
                return Ok(result);
            },
            ex => ex is ArgumentException
                ? Task.FromResult<ActionResult<BulkUpdateCorrectiveActionsResult>>(BadRequest(ex.Message))
                : Error<BulkUpdateCorrectiveActionsResult>(ex)
        );
    }

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

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/corrective-actions/{id:int}/photos")]
    [ProducesResponseType(typeof(CorrectiveActionPhotoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadCaClosurePhoto([FromRoute] int id, IFormFile file, [FromForm] string? caption)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user  = await GetUser();
                using var ms = new System.IO.MemoryStream();
                await file.CopyToAsync(ms);
                var result = await Mediator.Send(new UploadCaClosurePhoto
                {
                    CorrectiveActionId = id,
                    FileName           = file.FileName,
                    FileData           = ms.ToArray(),
                    UploadedBy         = user.Email!,
                    Caption            = caption,
                });
                return Ok(result);
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : ex is KeyNotFoundException
                    ? Task.FromResult<IActionResult>(NotFound(ex.Message))
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
                    Weight = body.Weight,
                    IsLifeCritical = body.IsLifeCritical,
                    AllowNA = body.AllowNA,
                    RequireCommentOnNC = body.RequireCommentOnNC,
                    IsScoreable = body.IsScoreable,
                    RequirePhotoOnNc = body.RequirePhotoOnNc,
                    AutoCreateCa = body.AutoCreateCa,
                    UpdatedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is InvalidOperationException or ArgumentException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/versions/{draftId:int}/questions/weights")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchUpdateQuestionWeights([FromRoute] int draftId, [FromBody] BatchUpdateQuestionWeightsRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new BatchUpdateQuestionWeights
                {
                    DraftVersionId = draftId,
                    Weights = body.Weights.Select(w => (w.VersionQuestionId, w.Weight)).ToList(),
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

    // ── Admin — Skip-Logic Rules ──────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/templates/{versionId:int}/logic-rules")]
    [ProducesResponseType(typeof(List<LogicRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogicRules([FromRoute] int versionId)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new GetLogicRules { TemplateVersionId = versionId });
                return Ok(result);
            },
            ex => Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/templates/logic-rules")]
    [ProducesResponseType(typeof(LogicRuleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertLogicRule([FromBody] SaveLogicRuleRequest req)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new UpsertLogicRule { Rule = req });
                return Ok(result);
            },
            ex => Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("admin/templates/logic-rules/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteLogicRule([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                await Mediator.Send(new DeleteLogicRule { Id = id });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
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

    // ── Newsletter Templates ──────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/newsletter-template")]
    [ProducesResponseType(typeof(NewsletterTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NewsletterTemplateDto>> GetNewsletterTemplate([FromQuery] int divisionId)
    {
        return await TryExecuteAsync<ActionResult<NewsletterTemplateDto>>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new GetNewsletterTemplate { DivisionId = divisionId });
                return result == null ? NotFound() : Ok(result);
            },
            ex => Error<NewsletterTemplateDto>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("audits/newsletter-template")]
    [ProducesResponseType(typeof(NewsletterTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NewsletterTemplateDto>> SaveNewsletterTemplate([FromBody] SaveNewsletterTemplateRequest body)
    {
        return await TryExecuteAsync<ActionResult<NewsletterTemplateDto>>(
            async () =>
            {
                var user = await GetUser();
                var result = await Mediator.Send(new SaveNewsletterTemplate
                {
                    Payload = body,
                    SavedBy = user.Email!,
                });
                return Ok(result);
            },
            ex => ex is ArgumentException
                ? Task.FromResult<ActionResult<NewsletterTemplateDto>>(BadRequest(ex.Message))
                : Error<NewsletterTemplateDto>(ex)
        );
    }

    // ── Newsletter ─────────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/newsletter/ai-summary")]
    [ProducesResponseType(typeof(NewsletterAiSummaryResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<NewsletterAiSummaryResult>> GenerateNewsletterSummary(
        [FromBody] GenerateNewsletterSummaryRequest body)
    {
        return await TryExecuteAsync<ActionResult<NewsletterAiSummaryResult>>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new GenerateNewsletterSummary
                {
                    DivisionCode = body.DivisionCode,
                    Quarter = body.Quarter,
                    Year = body.Year,
                    AvgScore = body.AvgScore,
                    TotalAudits = body.TotalAudits,
                    TotalNcs = body.TotalNcs,
                    TopSections = body.TopSections
                        .Select(s => new SectionNcItem { SectionName = s.SectionName, NcCount = s.NcCount })
                        .ToList<SectionNcItem>(),
                    OpenCaCount = body.OpenCaCount,
                    OverdueCaCount = body.OverdueCaCount,
                });
                return Ok(result);
            },
            ex => Error<NewsletterAiSummaryResult>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/newsletter/send")]
    [ProducesResponseType(typeof(NewsletterSendResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<NewsletterSendResult>> SendNewsletter(
        [FromBody] SendNewsletterRequest body)
    {
        return await TryExecuteAsync<ActionResult<NewsletterSendResult>>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new SendNewsletter
                {
                    DivisionId = body.DivisionId,
                    Subject = body.Subject,
                    HtmlBody = body.HtmlBody,
                });
                return Ok(result);
            },
            ex => Error<NewsletterSendResult>(ex)
        );
    }

    // ── Report Drafts ──────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/report-drafts")]
    [ProducesResponseType(typeof(List<ReportDraftListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReportDraftListItemDto>>> GetReportDrafts(
        [FromQuery] int? divisionId)
    {
        return await TryExecuteAsync<ActionResult<List<ReportDraftListItemDto>>>(
            async () =>
            {
                var user = await GetUser();
                return Ok(await Mediator.Send(new GetReportDrafts
                {
                    DivisionId = divisionId,
                    RequestedBy = user.Email!,
                }));
            },
            ex => Error<List<ReportDraftListItemDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/report-drafts/{id:int}")]
    [ProducesResponseType(typeof(ReportDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDraftDto>> GetReportDraft([FromRoute] int id)
    {
        return await TryExecuteAsync<ActionResult<ReportDraftDto>>(
            async () =>
            {
                var user = await GetUser();
                var result = await Mediator.Send(new GetReportDraft
                {
                    DraftId = id,
                    RequestedBy = user.Email!,
                });
                return result == null ? NotFound() : Ok(result);
            },
            ex => Error<ReportDraftDto>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/report-drafts")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> CreateReportDraft([FromBody] CreateReportDraftRequest body)
    {
        return await TryExecuteAsync<ActionResult<int>>(
            async () =>
            {
                var user = await GetUser();
                var newId = await Mediator.Send(new CreateReportDraft
                {
                    Payload = body,
                    CreatedBy = user.Email!,
                });
                return CreatedAtAction(nameof(GetReportDraft), new { id = newId }, newId);
            },
            ex => ex is ArgumentException
                ? Task.FromResult<ActionResult<int>>(BadRequest(ex.Message))
                : Error<int>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("audits/report-drafts/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateReportDraft([FromRoute] int id, [FromBody] UpdateReportDraftRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new UpdateReportDraft
                {
                    DraftId = id,
                    Payload = body,
                    UpdatedBy = user.Email!,
                });
                return NoContent();
            },
            ex => ex is ArgumentException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : ex is KeyNotFoundException
                    ? Task.FromResult<IActionResult>(NotFound())
                    : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("audits/report-drafts/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReportDraft([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new DeleteReportDraft
                {
                    DraftId = id,
                    DeletedBy = user.Email!,
                });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound())
                : Error(ex)
        );
    }

    // ── Attachments ──────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/{id:int}/attachments")]
    [ProducesResponseType(typeof(List<AuditAttachmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditAttachmentDto>>> GetAttachments([FromRoute] int id)
    {
        return await TryExecuteAsync<ActionResult<List<AuditAttachmentDto>>>(
            async () =>
            {
                await GetUser();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                return Ok(await Mediator.Send(new GetAttachments { AuditId = id, BaseUrl = baseUrl }));
            },
            ex => Error<List<AuditAttachmentDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/attachments")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AuditAttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuditAttachmentDto>> UploadAttachment([FromRoute] int id, IFormFile file)
    {
        return await TryExecuteAsync<ActionResult<AuditAttachmentDto>>(
            async () =>
            {
                var user = await GetUser();
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided.");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var fileData = ms.ToArray();

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var result = await Mediator.Send(new UploadAttachment
                {
                    AuditId = id,
                    FileName = file.FileName,
                    FileData = fileData,
                    UploadedBy = user.Email!,
                    BaseUrl = baseUrl,
                });
                return StatusCode(StatusCodes.Status201Created, result);
            },
            ex => ex is InvalidOperationException or ArgumentException
                ? Task.FromResult<ActionResult<AuditAttachmentDto>>(BadRequest(ex.Message))
                : Error<AuditAttachmentDto>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/{id:int}/attachments/{aid:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAttachment([FromRoute] int id, [FromRoute] int aid)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new DownloadAttachment { AuditId = id, AttachmentId = aid });
                return File(result.FileStream, result.ContentType, result.FileName);
            },
            ex => ex is KeyNotFoundException or FileNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("audits/{id:int}/attachments/{aid:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachment([FromRoute] int id, [FromRoute] int aid)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new DeleteAttachment
                {
                    AuditId = id,
                    AttachmentId = aid,
                    DeletedBy = user.Email!
                });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
        );
    }

    // ── Reports — Compliance Status ──────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("reports/compliance-status")]
    [ProducesResponseType(typeof(List<ComplianceStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ComplianceStatusDto>>> GetComplianceStatus()
    {
        return await TryExecuteAsync<ActionResult<List<ComplianceStatusDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetComplianceStatus()));
            },
            ex => Error<List<ComplianceStatusDto>>(ex)
        );
    }

    // ── Reports — Generate PDF ────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("reports/generate")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                if (string.IsNullOrWhiteSpace(body.TemplateId))
                    return BadRequest("templateId is required.");

                var pdfBytes = await Mediator.Send(new GenerateReport { Payload = body });

                var fileName = $"report-{body.TemplateId}-{DateTime.UtcNow:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    // ── Reports — Generate Structured PDF ────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("reports/generate-structured")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateStructuredReport([FromBody] GenerateStructuredReportRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                if (string.IsNullOrWhiteSpace(body.StructuredReportJson))
                    return BadRequest("structuredReportJson is required.");

                var pdfBytes = await Mediator.Send(new GenerateStructuredReportCommand { Payload = body });

                var fileName = $"structured-report-{DateTime.UtcNow:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            },
            ex => ex is InvalidOperationException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    // ── Reports — Scheduled Delivery ─────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("reports/scheduled")]
    [ProducesResponseType(typeof(List<ScheduledReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ScheduledReportDto>>> GetScheduledReports([FromQuery] int? divisionId)
    {
        return await TryExecuteAsync<ActionResult<List<ScheduledReportDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetScheduledReports { DivisionId = divisionId }));
            },
            ex => Error<List<ScheduledReportDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("reports/scheduled")]
    [ProducesResponseType(typeof(ScheduledReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScheduledReportDto>> SaveScheduledReport([FromBody] SaveScheduledReportRequest body)
    {
        return await TryExecuteAsync<ActionResult<ScheduledReportDto>>(
            async () =>
            {
                var user = await GetUser();
                var result = await Mediator.Send(new SaveScheduledReport { Payload = body, SavedBy = user.Email! });
                return Ok(result);
            },
            ex => ex is ArgumentException
                ? Task.FromResult<ActionResult<ScheduledReportDto>>(BadRequest(ex.Message))
                : Error<ScheduledReportDto>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("reports/scheduled/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScheduledReport([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                await Mediator.Send(new DeleteScheduledReport { Id = id });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
        );
    }

    // ── Audits by Employee ────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/by-employee")]
    [ProducesResponseType(typeof(List<AuditsByEmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditsByEmployee(
        [FromQuery] int? divisionId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new GetAuditsByEmployee
                {
                    DivisionId = divisionId,
                    DateFrom   = dateFrom,
                    DateTo     = dateTo,
                });
                return Ok(result);
            },
            ex => Error(ex)
        );
    }

    // ── Excel Exports ─────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/export/quarterly-summary")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportQuarterlySummary(
        [FromQuery] int? divisionId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var bytes = await Mediator.Send(new ExportQuarterlySummary
                {
                    DivisionId = divisionId,
                    DateFrom   = dateFrom,
                    DateTo     = dateTo,
                });
                var fileName = $"quarterly-summary-{DateTime.UtcNow:yyyy-MM}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            },
            ex => Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/export/ncr-report")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportNcrReport(
        [FromQuery] int? divisionId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var bytes = await Mediator.Send(new ExportNcrReport
                {
                    DivisionId = divisionId,
                    DateFrom   = dateFrom,
                    DateTo     = dateTo,
                });
                var fileName = $"ncr-report-{DateTime.UtcNow:yyyy-MM}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            },
            ex => Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("corrective-actions/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCorrectiveActions(
        [FromQuery] int? divisionId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? status,
        [FromQuery] string? source,
        [FromQuery] string? priority,
        [FromQuery] bool overdueOnly = false,
        [FromQuery] string? searchText = null)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var bytes = await Mediator.Send(new ExportCorrectiveActions
                {
                    DivisionId  = divisionId,
                    DateFrom    = dateFrom,
                    DateTo      = dateTo,
                    Status      = status,
                    Source      = source,
                    Priority    = priority,
                    OverdueOnly = overdueOnly,
                    SearchText  = searchText,
                });
                var fileName = $"corrective-actions-{DateTime.UtcNow:yyyy-MM}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            },
            ex => Error(ex)
        );
    }

    // ── Admin — Review Group ──────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/review-group")]
    [ProducesResponseType(typeof(List<ReviewGroupMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewGroupMemberDto>>> GetReviewGroup()
    {
        return await TryExecuteAsync<ActionResult<List<ReviewGroupMemberDto>>>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetReviewGroup()));
            },
            ex => Error<List<ReviewGroupMemberDto>>(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/review-group")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SaveReviewGroup([FromBody] SaveReviewGroupRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new SaveReviewGroup { Payload = body, SavedBy = user.Email! });
                return NoContent();
            },
            ex => Error(ex)
        );
    }

    // ── Finding Photos ────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/{id:int}/questions/{qid:int}/photos")]
    [ProducesResponseType(typeof(List<FindingPhotoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFindingPhotos([FromRoute] int id, [FromRoute] int qid)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                return Ok(await Mediator.Send(new GetFindingPhotos { AuditId = id, QuestionId = qid, BaseUrl = baseUrl }));
            },
            ex => Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPost("audits/{id:int}/questions/{qid:int}/photos")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FindingPhotoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFindingPhoto([FromRoute] int id, [FromRoute] int qid, IFormFile file, [FromForm] string? caption)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided.");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var result = await Mediator.Send(new UploadFindingPhoto
                {
                    AuditId = id,
                    QuestionId = qid,
                    FileName = file.FileName,
                    FileData = ms.ToArray(),
                    Caption = caption,
                    UploadedBy = user.Email!,
                    BaseUrl = baseUrl,
                });
                return StatusCode(StatusCodes.Status201Created, result);
            },
            ex => ex is InvalidOperationException or KeyNotFoundException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/{id:int}/questions/{qid:int}/photos/{pid:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFindingPhoto([FromRoute] int id, [FromRoute] int qid, [FromRoute] int pid)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new DownloadFindingPhoto { AuditId = id, QuestionId = qid, PhotoId = pid });
                return File(result.FileStream, result.ContentType, result.FileName);
            },
            ex => ex is KeyNotFoundException or FileNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpDelete("audits/{id:int}/questions/{qid:int}/photos/{pid:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFindingPhoto([FromRoute] int id, [FromRoute] int qid, [FromRoute] int pid)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new DeleteFindingPhoto { AuditId = id, QuestionId = qid, PhotoId = pid, DeletedBy = user.Email });
                return NoContent();
            },
            ex => ex is KeyNotFoundException
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
        );
    }

    // ── Repeat Findings ───────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/{id:int}/repeat-findings")]
    [ProducesResponseType(typeof(List<RepeatFindingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRepeatFindings([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetRepeatFindings { AuditId = id }));
            },
            ex => Error(ex)
        );
    }

    // ── Question History ──────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/question-history")]
    [ProducesResponseType(typeof(List<QuestionHistoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionHistory(
        [FromQuery] int questionId,
        [FromQuery] int divisionId,
        [FromQuery] int limit = 3)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetQuestionHistory
                {
                    QuestionId = questionId,
                    DivisionId = divisionId,
                    Limit      = limit,
                }));
            },
            ex => Error(ex)
        );
    }

    // ── Prior Audit Prefill ───────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("audits/prior-prefill")]
    [ProducesResponseType(typeof(PriorAuditPrefillDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPriorAuditPrefill(
        [FromQuery] int divisionId,
        [FromQuery] int templateVersionId)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetPriorAuditPrefill
                {
                    DivisionId        = divisionId,
                    TemplateVersionId = templateVersionId,
                }));
            },
            ex => Error(ex)
        );
    }

    // ── Division Score Targets ────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/divisions/score-targets")]
    [ProducesResponseType(typeof(List<DivisionScoreTargetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDivisionScoreTargets()
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetDivisionScoreTargets()));
            },
            ex => Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/divisions/{id:int}/score-target")]
    [ProducesResponseType(typeof(DivisionScoreTargetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDivisionScoreTarget([FromRoute] int id, [FromBody] SetScoreTargetRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                var result = await Mediator.Send(new SetDivisionScoreTarget { DivisionId = id, ScoreTarget = body.ScoreTarget });
                return Ok(result);
            },
            ex => ex is ArgumentException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : ex is KeyNotFoundException
                    ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                    : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/divisions/{id:int}/audit-frequency")]
    [ProducesResponseType(typeof(DivisionScoreTargetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDivisionAuditFrequency([FromRoute] int id, [FromBody] SetAuditFrequencyRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new SetDivisionAuditFrequency
                {
                    DivisionId = id,
                    AuditFrequencyDays = body.AuditFrequencyDays,
                }));
            },
            ex => ex.Message.Contains("not found")
                    ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                    : ex is ArgumentException
                        ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                        : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("audits/divisions/{id:int}/require-closure-photo")]
    [ProducesResponseType(typeof(DivisionScoreTargetDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDivisionRequireClosurePhoto([FromRoute] int id, [FromBody] SetRequireClosurePhotoRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new SetDivisionClosurePhotoRequirement
                {
                    DivisionId          = id,
                    RequireClosurePhoto = body.RequireClosurePhoto,
                }));
            },
            ex => ex.Message.Contains("not found")
                ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPut("admin/divisions/{id:int}/sla")]
    [ProducesResponseType(typeof(DivisionScoreTargetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDivisionSla([FromRoute] int id, [FromBody] SetDivisionSlaRequest body)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new SetDivisionSla
                {
                    DivisionId           = id,
                    SlaNormalDays        = body.SlaNormalDays,
                    SlaUrgentDays        = body.SlaUrgentDays,
                    SlaEscalateAfterDays = body.SlaEscalateAfterDays,
                    EscalationEmail      = body.EscalationEmail,
                }));
            },
            ex => ex is ArgumentException
                ? Task.FromResult<IActionResult>(BadRequest(ex.Message))
                : ex is KeyNotFoundException
                    ? Task.FromResult<IActionResult>(NotFound(ex.Message))
                    : Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("divisions/{id:int}/benchmark")]
    [ProducesResponseType(typeof(DivisionBenchmarkDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDivisionBenchmark([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetDivisionBenchmark { DivisionId = id }));
            },
            ex => Error(ex)
        );
    }

    // ── Audit logs ────────────────────────────────────────────────────────────

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("admin/audit-logs")]
    [ProducesResponseType(typeof(AuditLogsResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? userEmail,
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                await GetUser();
                return Ok(await Mediator.Send(new GetAuditLogs
                {
                    DateFrom   = dateFrom,
                    DateTo     = dateTo,
                    UserEmail  = userEmail,
                    EntityType = entityType,
                    Action     = action,
                    Search     = search,
                    Page       = page,
                    PageSize   = pageSize,
                }));
            },
            ex => Error(ex)
        );
    }
}
