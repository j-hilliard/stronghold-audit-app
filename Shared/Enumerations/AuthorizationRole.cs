namespace Stronghold.AppDashboard.Shared.Enumerations;

public enum AuthorizationRole
{
    User,
    ApplicationDirectoryManager,
    IntegratedApplicationManager,
    Administrator,
    AuthenticatedUser,

    // ── Audit-specific roles ──────────────────────────────────────────────────
    TemplateAdmin,
    AuditManager,
    AuditReviewer,
    CorrectiveActionOwner,
    ReadOnlyViewer,
    ExecutiveViewer,
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

    // ── Audit-specific roles ──────────────────────────────────────────────────
    public const string TemplateAdmin         = nameof(AuthorizationRole.TemplateAdmin);
    public const string AuditManager         = nameof(AuthorizationRole.AuditManager);
    public const string AuditReviewer        = nameof(AuthorizationRole.AuditReviewer);
    public const string CorrectiveActionOwner = nameof(AuthorizationRole.CorrectiveActionOwner);
    public const string ReadOnlyViewer       = nameof(AuthorizationRole.ReadOnlyViewer);
    public const string ExecutiveViewer      = nameof(AuthorizationRole.ExecutiveViewer);
}
