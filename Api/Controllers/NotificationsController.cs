using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stronghold.AppDashboard.Api.Domain.Audit.Notifications;
using Stronghold.AppDashboard.Api.Helpers;

namespace Stronghold.AppDashboard.Api.Controllers;

[Route(Constants.Routes.ApiTemplate)]
public class NotificationsController : V1ControllerBase
{
    public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
        : base(mediator, logger) { }

    /// <summary>
    /// Returns the most recent notifications for the authenticated user.
    /// </summary>
    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(NotificationsResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationsResult>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int pageSize = 20)
    {
        return await TryExecuteAsync<ActionResult<NotificationsResult>>(
            async () =>
            {
                var user = await GetUser();
                var result = await Mediator.Send(new GetNotifications
                {
                    RecipientEmail = user.Email!,
                    UnreadOnly     = unreadOnly,
                    PageSize       = Math.Min(pageSize, 50),
                });
                return Ok(result);
            },
            ex => Error<NotificationsResult>(ex)
        );
    }

    /// <summary>
    /// Marks one notification (by id) or all notifications as read.
    /// PATCH /v1/notifications/read — marks all unread as read.
    /// PATCH /v1/notifications/{id}/read — marks a single notification as read.
    /// </summary>
    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPatch("notifications/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead()
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new MarkNotificationsRead
                {
                    RecipientEmail = user.Email!,
                    NotificationId = null,
                });
                return NoContent();
            },
            ex => Error(ex)
        );
    }

    [MapToApiVersion(Constants.ApiVersions.V1)]
    [HttpPatch("notifications/{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkOneRead([FromRoute] int id)
    {
        return await TryExecuteAsync<IActionResult>(
            async () =>
            {
                var user = await GetUser();
                await Mediator.Send(new MarkNotificationsRead
                {
                    RecipientEmail = user.Email!,
                    NotificationId = id,
                });
                return NoContent();
            },
            ex => Error(ex)
        );
    }
}
