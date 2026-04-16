using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Newsletter;

[AllowedAuthorizationRole(
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class SaveNewsletterTemplate : IRequest<NewsletterTemplateDto>
{
    public SaveNewsletterTemplateRequest Payload { get; set; } = null!;
    public string SavedBy { get; set; } = null!;
}

public class SaveNewsletterTemplateHandler : IRequestHandler<SaveNewsletterTemplate, NewsletterTemplateDto>
{
    private readonly AppDbContext _context;

    public SaveNewsletterTemplateHandler(AppDbContext context) => _context = context;

    public async Task<NewsletterTemplateDto> Handle(SaveNewsletterTemplate request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var now = DateTime.UtcNow;
        var visibleJson = payload.VisibleSections == null
            ? null
            : JsonSerializer.Serialize(payload.VisibleSections);

        // If IsDefault, clear any existing default for this division first
        if (payload.IsDefault)
        {
            var existing = await _context.NewsletterTemplates
                .Where(t => t.DivisionId == payload.DivisionId && t.IsDefault && !t.IsDeleted)
                .ToListAsync(cancellationToken);
            foreach (var t in existing)
            {
                t.IsDefault = false;
                t.UpdatedAt = now;
                t.UpdatedBy = request.SavedBy;
            }
        }

        // Upsert — one default template per division
        var template = await _context.NewsletterTemplates
            .FirstOrDefaultAsync(t =>
                t.DivisionId == payload.DivisionId && !t.IsDeleted,
                cancellationToken);

        if (template == null)
        {
            template = new NewsletterTemplate
            {
                DivisionId = payload.DivisionId,
                CreatedAt = now,
                CreatedBy = request.SavedBy,
            };
            _context.NewsletterTemplates.Add(template);
        }
        else
        {
            template.UpdatedAt = now;
            template.UpdatedBy = request.SavedBy;
        }

        template.Name = payload.Name;
        template.PrimaryColor = payload.PrimaryColor;
        template.AccentColor = payload.AccentColor;
        template.CoverImageUrl = payload.CoverImageUrl;
        template.VisibleSectionsJson = visibleJson;
        template.IsDefault = payload.IsDefault;

        await _context.SaveChangesAsync(cancellationToken);

        return new NewsletterTemplateDto
        {
            Id = template.Id,
            DivisionId = template.DivisionId,
            Name = template.Name,
            PrimaryColor = template.PrimaryColor,
            AccentColor = template.AccentColor,
            CoverImageUrl = template.CoverImageUrl,
            IsDefault = template.IsDefault,
            VisibleSections = payload.VisibleSections,
        };
    }
}
