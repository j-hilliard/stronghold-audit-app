using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator)]
public class GetArchivedQuestions : IRequest<List<ArchivedQuestionDto>> { }

public class GetArchivedQuestionsHandler : IRequestHandler<GetArchivedQuestions, List<ArchivedQuestionDto>>
{
    private readonly AppDbContext _context;

    public GetArchivedQuestionsHandler(AppDbContext context) => _context = context;

    public async Task<List<ArchivedQuestionDto>> Handle(GetArchivedQuestions request, CancellationToken cancellationToken)
    {
        // AuditQuestion has no global IsDeleted filter — we query directly
        var questions = await _context.AuditQuestions
            .Where(q => q.IsArchived)
            .OrderByDescending(q => q.ArchivedAt)
            .ToListAsync(cancellationToken);

        return questions.Select(q => new ArchivedQuestionDto
        {
            QuestionId = q.Id,
            QuestionText = q.QuestionText,
            ArchivedAt = q.ArchivedAt,
            ArchivedBy = q.ArchivedBy
        }).ToList();
    }
}
