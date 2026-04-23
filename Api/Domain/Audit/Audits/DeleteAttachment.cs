using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin)]
public class DeleteAttachment : IRequest<Unit>
{
    public int AuditId { get; set; }
    public int AttachmentId { get; set; }
    public string DeletedBy { get; set; } = null!;
}

public class DeleteAttachmentHandler : IRequestHandler<DeleteAttachment, Unit>
{
    private readonly AppDbContext _context;

    public DeleteAttachmentHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteAttachment request, CancellationToken cancellationToken)
    {
        var attachment = await _context.AuditAttachments
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.AuditId == request.AuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attachment {request.AttachmentId} not found on audit {request.AuditId}.");

        // Delete the physical file
        if (!string.IsNullOrEmpty(attachment.BlobPath) && File.Exists(attachment.BlobPath))
        {
            try { File.Delete(attachment.BlobPath); }
            catch { /* Log but don't fail the delete if file is already gone */ }
        }

        // Soft-delete the record
        var now = DateTime.UtcNow;
        attachment.IsDeleted = true;
        attachment.DeletedAt = now;
        attachment.DeletedBy = request.DeletedBy;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
