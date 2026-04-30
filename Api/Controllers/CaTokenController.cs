using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stronghold.AppDashboard.Api.Domain.Audit.Audits;
using Stronghold.AppDashboard.Api.Helpers;

namespace Stronghold.AppDashboard.Api.Controllers;

/// <summary>
/// Authenticated admin endpoints for managing corrective action public access tokens.
/// </summary>
[ApiVersion(Constants.ApiVersions.V1)]
[Route("v1/corrective-actions/{caId:int}/tokens")]
public class CaTokenController : V1ControllerBase
{
    public CaTokenController(IMediator mediator, ILogger<V1ControllerBase> logger)
        : base(mediator, logger) { }

    /// <summary>Generate a public access token for the given corrective action.</summary>
    [HttpPost]
    public async Task<ActionResult<CreateCaPublicTokenResult>> CreateToken(
        int caId,
        [FromBody] CreateCaTokenRequest body)
    {
        try
        {
            var user = await GetUser();
            var result = await Mediator.Send(new CreateCaPublicToken
            {
                CorrectiveActionId = caId,
                SentToName         = body.SentToName,
                SentToEmail        = body.SentToEmail,
                ExpiresAt          = body.ExpiresAt,
                CreatedBy          = $"{user.FirstName} {user.LastName}".Trim() is { Length: > 0 } n ? n : (user.Email ?? "Unknown"),
            });
            return Ok(result);
        }
        catch (ArgumentException ex)           { return BadRequest(new ProblemDetails { Detail = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new ProblemDetails { Detail = ex.Message }); }
        catch (InvalidOperationException ex)   { return BadRequest(new ProblemDetails { Detail = ex.Message }); }
    }
}

public class CreateCaTokenRequest
{
    public string?   SentToName  { get; set; }
    public string?   SentToEmail { get; set; }
    public DateTime? ExpiresAt   { get; set; }
}
