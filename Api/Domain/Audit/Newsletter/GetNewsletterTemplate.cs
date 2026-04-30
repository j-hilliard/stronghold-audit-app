using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Newsletter;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class GetNewsletterTemplate : IRequest<NewsletterTemplateDto?>
{
    public int DivisionId { get; set; }
}

public class GetNewsletterTemplateHandler : IRequestHandler<GetNewsletterTemplate, NewsletterTemplateDto?>
{
    private readonly AppDbContext _context;

    public GetNewsletterTemplateHandler(AppDbContext context) => _context = context;

    public async Task<NewsletterTemplateDto?> Handle(GetNewsletterTemplate request, CancellationToken cancellationToken)
    {
        var template = await _context.NewsletterTemplates
            .FirstOrDefaultAsync(t =>
                t.DivisionId == request.DivisionId && t.IsDefault && !t.IsDeleted,
                cancellationToken);

        if (template == null) return null;

        return new NewsletterTemplateDto
        {
            Id = template.Id,
            DivisionId = template.DivisionId,
            Name = template.Name,
            PrimaryColor = template.PrimaryColor,
            AccentColor = template.AccentColor,
            CoverImageUrl = template.CoverImageUrl,
            IsDefault = template.IsDefault,
            VisibleSections = template.VisibleSectionsJson == null
                ? null
                : JsonSerializer.Deserialize<List<string>>(template.VisibleSectionsJson),
        };
    }
}
