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
public class GetNotifications : IRequest<NotificationsResult>
{
    public string RecipientEmail { get; set; } = null!;
    public bool UnreadOnly { get; set; } = false;
    public int PageSize { get; set; } = 20;
}

public record NotificationDto(
    int Id,
    string Type,
    string Title,
    string Body,
    string? EntityType,
    int? EntityId,
    string? LinkUrl,
    bool IsRead,
    DateTime CreatedAt
);

public record NotificationsResult(
    List<NotificationDto> Items,
    int UnreadCount
);

public class GetNotificationsHandler : IRequestHandler<GetNotifications, NotificationsResult>
{
    private readonly AppDbContext _context;

    public GetNotificationsHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationsResult> Handle(GetNotifications request, CancellationToken cancellationToken)
    {
        var query = _context.AppNotifications
            .Where(n => n.RecipientEmail == request.RecipientEmail);

        var unreadCount = await query.CountAsync(n => !n.IsRead, cancellationToken);

        if (request.UnreadOnly)
            query = query.Where(n => !n.IsRead);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(request.PageSize)
            .Select(n => new NotificationDto(
                n.Id, n.Type, n.Title, n.Body,
                n.EntityType, n.EntityId, n.LinkUrl,
                n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);

        return new NotificationsResult(items, unreadCount);
    }
}
