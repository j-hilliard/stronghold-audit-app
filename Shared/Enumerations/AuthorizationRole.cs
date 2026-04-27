namespace Stronghold.AppDashboard.Shared.Enumerations;

public enum AuthorizationRole
{
    User,
    ApplicationDirectoryManager,
    IntegratedApplicationManager,
    Administrator,
    AuthenticatedUser,

    // ── Audit-specific roles (legacy granular) ────────────────────────────────
    TemplateAdmin,
    AuditManager,
    AuditReviewer,
    CorrectiveActionOwner,
    ReadOnlyViewer,
    ExecutiveViewer,

    // ── Official user-facing roles (authoritative) ────────────────────────────
    ITAdmin,
    Auditor,
    AuditAdmin,
    Executive,
    NormalUser,
}

public static class AuthorizationRoles
{
    public const string Administrator = nameof(AuthorizationRole.Administrator);
    public const string ApplicationDirectoryManager = nameof(
        AuthorizationRole.ApplicationDirectoryManager
    );
    public const string IntegratedApplicationManager = nameof(
        AuthorizationRole.IntegratedApplicationManager
    );
    public const string User = nameof(AuthorizationRole.User);

    // ── Audit-specific roles (legacy granular) ────────────────────────────────
    public const string TemplateAdmin         = nameof(AuthorizationRole.TemplateAdmin);
    public const string AuditManager         = nameof(AuthorizationRole.AuditManager);
    public const string AuditReviewer        = nameof(AuthorizationRole.AuditReviewer);
    public const string CorrectiveActionOwner = nameof(AuthorizationRole.CorrectiveActionOwner);
    public const string ReadOnlyViewer       = nameof(AuthorizationRole.ReadOnlyViewer);
    public const string ExecutiveViewer      = nameof(AuthorizationRole.ExecutiveViewer);

    // ── Official user-facing roles (authoritative) ────────────────────────────
    public const string ITAdmin    = nameof(AuthorizationRole.ITAdmin);
    public const string Auditor    = nameof(AuthorizationRole.Auditor);
    public const string AuditAdmin = nameof(AuthorizationRole.AuditAdmin);
    public const string Executive  = nameof(AuthorizationRole.Executive);
    public const string NormalUser = nameof(AuthorizationRole.NormalUser);
}
