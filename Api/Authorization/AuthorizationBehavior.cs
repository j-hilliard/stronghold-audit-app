using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Stronghold.AppDashboard.Api.Models;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Authorization;

/// <summary>Cached entry stored per user — avoids DB round trips on every request.</summary>
internal sealed record UserAuthCacheEntry(int UserId, List<string> Roles, List<int> DivisionIds);

/// <summary>
/// Roles that bypass the division-scope filter — these users see all divisions.
/// </summary>
internal static class GlobalAuditRoles
{
    public static readonly HashSet<string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        AuthorizationRoles.Administrator,
        AuthorizationRoles.TemplateAdmin,
        AuthorizationRoles.AuditManager,
        AuthorizationRoles.ExecutiveViewer,
        // Official roles — these bypass division filter
        AuthorizationRoles.AuditAdmin,
        AuthorizationRoles.Executive,
    };
}

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _appDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;
    private readonly AzureAdHelper _azureAdHeler;
    private readonly IWebHostEnvironment _env;
    private readonly IAuditUserContext _auditUserContext;

    public AuthorizationBehavior(
        IMapper mapper,
        IMemoryCache cache,
        AzureAdHelper azureAdHelper,
        AppDbContext appDbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger,
        IWebHostEnvironment env,
        IAuditUserContext auditUserContext
    )
    {
        _mapper = mapper;
        _cache = cache;
        _azureAdHeler = azureAdHelper;
        _appDbContext = appDbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _env = env;
        _auditUserContext = auditUserContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (_env.EnvironmentName == "Local")
        {
            var httpCtx = _httpContextAccessor.HttpContext;
            var devRoleHeader = httpCtx?.Request.Headers["X-Dev-Role-Override"].ToString();

            if (string.IsNullOrWhiteSpace(devRoleHeader))
                return await next(); // No override — bypass all checks (default Local behavior)

            // Role override active: enforce the AllowedAuthorizationRole attribute
            var overrideRoles = new List<string> { devRoleHeader.Trim(), AuthorizationRoles.User };

            var localUser = await _appDbContext.Users.FirstOrDefaultAsync(
                u => u.AzureAdObjectId == Guid.Empty, cancellationToken);
            var localIsGlobal = overrideRoles.Any(r => GlobalAuditRoles.Names.Contains(r));
            _auditUserContext.Initialize(localUser?.UserId ?? 0, localIsGlobal, []);

            var devAttribute = Attribute.GetCustomAttribute(request.GetType(), typeof(AllowedAuthorizationRole))
                as AllowedAuthorizationRole;

            if (devAttribute == null || devAttribute.IsAllowed(overrideRoles))
                return await next();

            throw new UnauthorizedAccessException(
                $"[DevRoleOverride] Roles [{string.Join(", ", overrideRoles)}] cannot perform '{typeof(TRequest).Name}'");
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (
            httpContext == null
            || httpContext.User == null
            || httpContext.User.Identity?.IsAuthenticated != true
        )
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var azureAdObjectId = httpContext
            .User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?.Value;
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            throw new UnauthorizedAccessException("User ID not valid");
        }
        if (!Guid.TryParse(azureAdObjectId, out Guid guidAzureAdObjectId))
        {
            throw new UnauthorizedAccessException("User ID not valid");
        }

        // ── Resolve roles + division scope (60-min memory cache per user) ──────
        var cacheKey = $"AuditAuth_{azureAdObjectId}";
        if (!_cache.TryGetValue(cacheKey, out UserAuthCacheEntry? authEntry) || authEntry == null)
        {
            // Ensure the user account exists in the database
            var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(
                u => u.AzureAdObjectId == guidAzureAdObjectId,
                cancellationToken
            );
            if (existingUser == null)
            {
                var adUserInfo = await _azureAdHeler.GetAdUserInfoByObjectIdAsync(guidAzureAdObjectId);
                if (adUserInfo != null && adUserInfo.AzureAdObjectId == guidAzureAdObjectId)
                {
                    var newUser = new User
                    {
                        AzureAdObjectId = guidAzureAdObjectId,
                        FirstName = adUserInfo.FirstName,
                        LastName = adUserInfo.LastName,
                        Email = adUserInfo.Email,
                        Company = adUserInfo.Company,
                        Department = adUserInfo.Department,
                        Title = adUserInfo.Title,
                        Active = true,
                        CreatedOn = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow,
                    };

                    var user = _mapper.Map<Data.Models.User>(newUser);
                    await _appDbContext.Users.AddAsync(user, cancellationToken);
                    await _appDbContext.SaveChangesAsync(cancellationToken);

                    if (user.UserId > 0)
                    {
                        var defaultRole = await _appDbContext.Roles.FirstOrDefaultAsync(
                            r => r.Name == AuthorizationRoles.User,
                            cancellationToken
                        );
                        if (defaultRole != null)
                        {
                            await _appDbContext.UserRoles.AddAsync(
                                _mapper.Map<Data.Models.UserRole>(new UserRole
                                {
                                    UserId = user.UserId,
                                    RoleId = defaultRole.RoleId,
                                }),
                                cancellationToken
                            );
                            await _appDbContext.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
            }

            var currentUser = await _appDbContext.Users
                .Where(u => u.AzureAdObjectId == guidAzureAdObjectId)
                .Select(u => new { u.UserId })
                .FirstOrDefaultAsync(cancellationToken);
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not found in the database");

            var roles = await _appDbContext.UserRoles
                .Where(ur => ur.UserId == currentUser.UserId)
                .Select(ur => ur.Role.Name)
                .ToListAsync(cancellationToken);

            if (!roles.Contains(AuthorizationRoles.User))
                roles.Add(AuthorizationRoles.User);

            // Load division scope (empty list = all divisions allowed for scoped roles)
            var divisionIds = await _appDbContext.UserDivisions
                .Where(ud => ud.UserId == currentUser.UserId)
                .Select(ud => ud.DivisionId)
                .ToListAsync(cancellationToken);

            authEntry = new UserAuthCacheEntry(currentUser.UserId, roles, divisionIds);
            _cache.Set(cacheKey, authEntry, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
        }

        // Populate the scoped IAuditUserContext for this request
        var isGlobal = authEntry.Roles.Any(r => GlobalAuditRoles.Names.Contains(r));
        _auditUserContext.Initialize(authEntry.UserId, isGlobal, authEntry.DivisionIds);

        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null");

        var attribute =
            Attribute.GetCustomAttribute(request.GetType(), typeof(AllowedAuthorizationRole))
            as AllowedAuthorizationRole;
        if (attribute == null)
        {
            throw new UnauthorizedAccessException(
                $"Allowed Authorization Role is not specified on the requested Api function called '{typeof(TRequest).Name}'"
            );
        }

        if (attribute.IsAllowed(authEntry.Roles))
        {
            return await next();
        }

        throw new UnauthorizedAccessException(
            $"User is not authorized to perform '{typeof(TRequest).Name}'"
        );
    }
}

public interface IRoleRequiredRequest
{
    string RequiredRole { get; }
}
