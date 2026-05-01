using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stronghold.AppDashboard.Api.Domain.Audit.Audits;

namespace Stronghold.AppDashboard.Api.Controllers;

/// <summary>
/// Unauthenticated endpoints accessed via secure expiring token links.
/// These routes are intentionally outside V1ControllerBase and carry [AllowAnonymous].
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("v1/public")]
public class PublicController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicController(IMediator mediator) => _mediator = mediator;

    /// <summary>Fetch corrective action details by public token.</summary>
    [HttpGet("ca/{token}")]
    public async Task<ActionResult<CaPublicAccessDto>> GetCa(string token)
    {
        try
        {
            var result = await _mediator.Send(new GetCaByPublicToken { Token = token });
            return Ok(result);
        }
        catch (ArgumentException ex)       { return BadRequest(new ProblemDetails { Detail = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new ProblemDetails { Detail = ex.Message }); }
        catch (InvalidOperationException ex)   { return BadRequest(new ProblemDetails { Detail = ex.Message }); }
    }

    /// <summary>Update corrective action status via public token (mark as InProgress or Submitted).</summary>
    [HttpPut("ca/{token}")]
    public async Task<IActionResult> UpdateCa(string token, [FromBody] PublicCaUpdateRequest body)
    {
        try
        {
            await _mediator.Send(new UpdateCaByPublicToken
            {
                Token         = token,
                NewStatus     = body.NewStatus,
                Notes         = body.Notes,
                UpdatedByName = body.UpdatedByName,
            });
            return NoContent();
        }
        catch (ArgumentException ex)           { return BadRequest(new ProblemDetails { Detail = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new ProblemDetails { Detail = ex.Message }); }
        catch (InvalidOperationException ex)   { return BadRequest(new ProblemDetails { Detail = ex.Message }); }
    }

    /// <summary>Upload a closure photo via public token.</summary>
    [HttpPost("ca/{token}/photos")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CorrectiveActionPhotoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadCaPhoto(string token, IFormFile file, [FromForm] string? caption)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ProblemDetails { Detail = "No file provided." });
        try
        {
            using var ms = new System.IO.MemoryStream();
            await file.CopyToAsync(ms);
            var result = await _mediator.Send(new UploadCaPhotoByPublicToken
            {
                Token    = token,
                FileName = file.FileName,
                FileData = ms.ToArray(),
                Caption  = caption,
            });
            return CreatedAtAction(nameof(GetCa), new { token }, result);
        }
        catch (ArgumentException ex)           { return BadRequest(new ProblemDetails { Detail = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new ProblemDetails { Detail = ex.Message }); }
        catch (InvalidOperationException ex)   { return BadRequest(new ProblemDetails { Detail = ex.Message }); }
    }
}

public class PublicCaUpdateRequest
{
    public string  NewStatus     { get; set; } = null!;
    public string? Notes         { get; set; }
    public string? UpdatedByName { get; set; }
}
