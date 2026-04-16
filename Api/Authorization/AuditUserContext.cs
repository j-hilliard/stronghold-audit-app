namespace Stronghold.AppDashboard.Api.Authorization;

/// <summary>
/// Scoped per HTTP request. Populated by AuthorizationBehavior after roles are resolved.
/// Provides division-scope filtering info to audit query handlers.
///
/// When not yet initialized (local dev bypass or anonymous endpoints),
/// IsGlobal defaults to true so handlers return all data without restriction.
/// </summary>
public interface IAuditUserContext
{
    int UserId { get; }
    /// <summary>True when this user can see all divisions (no scope restriction).</summary>
    bool IsGlobal { get; }
    /// <summary>
    /// Divisions this user is allowed to see. Null means all divisions.
    /// Only populated when IsGlobal is false.
    /// </summary>
    IReadOnlyList<int>? AllowedDivisionIds { get; }
    void Initialize(int userId, bool isGlobal, IReadOnlyList<int> allowedDivisionIds);
}

public sealed class AuditUserContext : IAuditUserContext
{
    private bool _initialized;
    private int _userId;
    private bool _isGlobal = true;
    private IReadOnlyList<int>? _allowedDivisionIds;

    public int UserId => _userId;

    /// <summary>Returns true when uninitialised (local dev) or user role bypasses division filter.</summary>
    public bool IsGlobal => !_initialized || _isGlobal;

    public IReadOnlyList<int>? AllowedDivisionIds =>
        (_initialized && !_isGlobal) ? _allowedDivisionIds : null;

    public void Initialize(int userId, bool isGlobal, IReadOnlyList<int> allowedDivisionIds)
    {
        _initialized = true;
        _userId      = userId;
        _isGlobal    = isGlobal;
        _allowedDivisionIds = allowedDivisionIds;
    }
}
