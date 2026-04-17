using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class CorrectiveActionPhotoDto
{
    public int    Id                  { get; set; }
    public int    CorrectiveActionId  { get; set; }
    public string FileName            { get; set; } = null!;
    public long   FileSizeBytes       { get; set; }
    public string UploadedBy          { get; set; } = null!;
    public DateTime UploadedAt        { get; set; }
    public string? Caption            { get; set; }
}

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class UploadCaClosurePhoto : IRequest<CorrectiveActionPhotoDto>
{
    public int    CorrectiveActionId { get; set; }
    public string FileName           { get; set; } = null!;
    public byte[] FileData           { get; set; } = Array.Empty<byte>();
    public string UploadedBy         { get; set; } = null!;
    public string? Caption           { get; set; }
}

public class UploadCaClosurePhotoHandler : IRequestHandler<UploadCaClosurePhoto, CorrectiveActionPhotoDto>
{
    private readonly AppDbContext _db;
    private readonly IAuditUserContext _userContext;
    private readonly IConfiguration _config;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".heic", ".gif", ".webp", ".bmp"
    };
    private const long MaxBytes = 25 * 1024 * 1024; // 25 MB

    public UploadCaClosurePhotoHandler(AppDbContext db, IAuditUserContext userContext, IConfiguration config)
    {
        _db          = db;
        _userContext = userContext;
        _config      = config;
    }

    public async Task<CorrectiveActionPhotoDto> Handle(UploadCaClosurePhoto request, CancellationToken ct)
    {
        var ext = Path.GetExtension(request.FileName);
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException(
                $"File type '{ext}' is not allowed. Accepted: jpg, jpeg, png, heic, gif, webp, bmp.");

        if (request.FileData.Length > MaxBytes)
            throw new InvalidOperationException("Photo exceeds the 25 MB limit.");

        var ca = await _db.CorrectiveActions
            .Include(c => c.Audit)
            .FirstOrDefaultAsync(c => c.Id == request.CorrectiveActionId, ct)
            ?? throw new KeyNotFoundException($"Corrective action {request.CorrectiveActionId} not found.");

        // Division scope enforcement (prevents IDOR across division boundaries)
        if (!_userContext.IsGlobal
            && _userContext.AllowedDivisionIds is { Count: > 0 } allowed
            && ca.Audit != null
            && !allowed.Contains(ca.Audit.DivisionId))
            throw new UnauthorizedAccessException("You do not have access to this corrective action.");

        var basePath = _config.GetValue<string>("Attachments:BasePath")
            ?? Path.Combine(Path.GetTempPath(), "stronghold-attachments");

        var photoDir = Path.Combine(basePath, "ca-photos", request.CorrectiveActionId.ToString());
        Directory.CreateDirectory(photoDir);

        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath   = Path.Combine(photoDir, storedName);
        await File.WriteAllBytesAsync(filePath, request.FileData, ct);

        var now = DateTime.UtcNow;
        var photo = new CorrectiveActionPhoto
        {
            CorrectiveActionId = request.CorrectiveActionId,
            FileName           = request.FileName,
            FilePath           = filePath,
            FileSizeBytes      = request.FileData.Length,
            UploadedAt         = now,
            UploadedBy         = request.UploadedBy,
            Caption            = request.Caption,
            CreatedAt          = now,
            CreatedBy          = request.UploadedBy,
        };

        _db.CorrectiveActionPhotos.Add(photo);
        await _db.SaveChangesAsync(ct);

        return new CorrectiveActionPhotoDto
        {
            Id                 = photo.Id,
            CorrectiveActionId = photo.CorrectiveActionId,
            FileName           = photo.FileName,
            FileSizeBytes      = photo.FileSizeBytes,
            UploadedBy         = photo.UploadedBy,
            UploadedAt         = photo.UploadedAt,
            Caption            = photo.Caption,
        };
    }
}
