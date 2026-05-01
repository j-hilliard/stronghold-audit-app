using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class UploadCaPhotoByPublicToken : IRequest<CorrectiveActionPhotoDto>
{
    public string  Token    { get; set; } = null!;
    public string  FileName { get; set; } = null!;
    public byte[]  FileData { get; set; } = Array.Empty<byte>();
    public string? Caption  { get; set; }
}

public class UploadCaPhotoByPublicTokenHandler : IRequestHandler<UploadCaPhotoByPublicToken, CorrectiveActionPhotoDto>
{
    private readonly AppDbContext    _context;
    private readonly IConfiguration _config;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".heic", ".gif", ".webp", ".bmp"
    };
    private const long MaxBytes = 25 * 1024 * 1024;

    public UploadCaPhotoByPublicTokenHandler(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config  = config;
    }

    public async Task<CorrectiveActionPhotoDto> Handle(UploadCaPhotoByPublicToken request, CancellationToken ct)
    {
        var ext = Path.GetExtension(request.FileName);
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException(
                $"File type '{ext}' is not allowed. Accepted: jpg, jpeg, png, heic, gif, webp, bmp.");

        if (request.FileData.Length > MaxBytes)
            throw new InvalidOperationException("Photo exceeds the 25 MB limit.");

        var record = await _context.CaPublicTokens
            .Include(t => t.CorrectiveAction)
            .FirstOrDefaultAsync(t => t.Token == request.Token, ct)
            ?? throw new ArgumentException("Invalid or expired access link.");

        if (record.IsRevoked)
            throw new UnauthorizedAccessException("This access link has been revoked.");

        if (record.ExpiresAt.HasValue && record.ExpiresAt.Value < DateTime.UtcNow)
            throw new UnauthorizedAccessException("This access link has expired.");

        var ca = record.CorrectiveAction;
        if (ca.Status is "Closed" or "Voided")
            throw new InvalidOperationException("This corrective action is no longer open for updates.");

        var basePath = _config.GetValue<string>("Attachments:BasePath")
            ?? Path.Combine(Path.GetTempPath(), "stronghold-attachments");

        var photoDir = Path.Combine(basePath, "ca-photos", ca.Id.ToString());
        Directory.CreateDirectory(photoDir);

        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath   = Path.Combine(photoDir, storedName);
        await File.WriteAllBytesAsync(filePath, request.FileData, ct);

        var uploadedBy = record.SentToName ?? "External";
        var now        = DateTime.UtcNow;

        var photo = new CorrectiveActionPhoto
        {
            CorrectiveActionId = ca.Id,
            FileName           = request.FileName,
            FilePath           = filePath,
            FileSizeBytes      = request.FileData.Length,
            UploadedAt         = now,
            UploadedBy         = uploadedBy,
            Caption            = request.Caption,
            CreatedAt          = now,
            CreatedBy          = uploadedBy,
        };

        _context.CorrectiveActionPhotos.Add(photo);
        await _context.SaveChangesAsync(ct);

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
