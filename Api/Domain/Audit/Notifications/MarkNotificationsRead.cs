using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Notifications;

[AllowedAuthorizationRole(
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditReviewer, AuthorizationRole.TemplateAdmin, AuthorizationRole.AuditManager,
    AuthorizationRole.Executive, AuthorizationRole.ExecutiveViewer, AuthorizationRole.NormalUser)]
public class MarkNotificationsRead : IRequest<Unit>
{
    public string RecipientEmail { get; set; } = null!;
    /// <summary>Null means mark all unread as read.</summary>
    public int? NotificationId { get; set; }
}

public class MarkNotificationsReadHandler : IRequestHandler<MarkNotificationsRead, Unit>
{
    private readonly AppDbContext _context;

    public MarkNotificationsReadHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(MarkNotificationsRead request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        if (request.NotificationId.HasValue)
        {
            var notification = await _context.AppNotifications
                .FirstOrDefaultAsync(n =>
                    n.Id == request.NotificationId.Value
                    && n.RecipientEmail == request.RecipientEmail,
                    cancellationToken);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            await _context.AppNotifications
                .Where(n => n.RecipientEmail == request.RecipientEmail && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, now),
                    cancellationToken);
        }

        return Unit.Value;
    }
}
