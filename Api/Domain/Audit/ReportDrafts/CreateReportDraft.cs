using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.ReportDrafts;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class CreateReportDraft : IRequest<int>
{
    public CreateReportDraftRequest Payload { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
}

public class CreateReportDraftHandler : IRequestHandler<CreateReportDraft, int>
{
    private readonly AppDbContext _context;

    public CreateReportDraftHandler(AppDbContext context) => _context = context;

    public async Task<int> Handle(CreateReportDraft request, CancellationToken cancellationToken)
    {
        var p = request.Payload;

        await DivisionAuth.AssertAccessAsync(_context, request.CreatedBy, p.DivisionId, cancellationToken);

        _ = await _context.Divisions.FirstOrDefaultAsync(d => d.Id == p.DivisionId, cancellationToken)
            ?? throw new ArgumentException($"Division {p.DivisionId} not found.");

        BlocksJsonValidator.Validate(p.BlocksJson);

        var (dateFrom, dateTo) = ParseDates(p.DateFrom, p.DateTo);

        var now = DateTime.UtcNow;
        var draft = new ReportDraft
        {
            DivisionId = p.DivisionId,
            Title = p.Title.Trim(),
            Period = p.Period.Trim(),
            DateFrom = dateFrom,
            DateTo = dateTo,
            BlocksJson = p.BlocksJson,
            CreatedAt = now,
            CreatedBy = request.CreatedBy,
        };

        _context.ReportDrafts.Add(draft);
        await _context.SaveChangesAsync(cancellationToken);
        return draft.Id;
    }

    internal static (DateTime? from, DateTime? to) ParseDates(string? dateFromIso, string? dateToIso)
    {
        var from = dateFromIso == null ? (DateTime?)null
            : DateTime.SpecifyKind(DateOnly.Parse(dateFromIso).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var to = dateToIso == null ? (DateTime?)null
            : DateTime.SpecifyKind(DateOnly.Parse(dateToIso).ToDateTime(new TimeOnly(23, 59, 59, 999)), DateTimeKind.Utc);

        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new ArgumentException("DateFrom must be on or before DateTo.");

        return (from, to);
    }
}
