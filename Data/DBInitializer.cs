using Stronghold.AppDashboard.Data.Models;

namespace Stronghold.AppDashboard.Data;

public static class DbInitializer
{
    /// <summary>
    /// Seeds system reference data safe for ALL environments: roles and audit reference data.
    /// Never seeds local-only users or demo audits — those belong in the Local Program.cs block.
    /// </summary>
    public static void Initialize(AppDbContext dbContext)
    {
        SeedUserRoles(dbContext);
        SeedAuditRoles(dbContext);
        AuditDbInitializer.SeedAuditData(dbContext);
    }

    /// <summary>
    /// Seeds the default LocalDevTesting admin user. Local environment only.
    /// </summary>
    public static void SeedDefaultLocalUser(AppDbContext dbContext)
    {
        SeedDefaultLocalUserInternal(dbContext);
    }

    private const string AdministratorRoleDescription =
        "Users in this group are given administrator level access to the application. They can perform all actions within the application.  Administrators can manage all application listings as well as grant access within the Application Dashboard for other users.";
    private const string UserRoleDescription =
        "Users in this group are given general basic level access to the application. Users in this group are granted the lowest level of access within the application.  Users in this group can also authenticate via SSO to the application.";
    private const string ApplicationDirectoryManagerDescription =
        "Users in this group can manage the application directory. They can create, update, and delete application listings. They can also disable application listings.";
    private const string IntegratedApplicationManagerDescription =
        "Users in this group can manage the integrated applications list. They can create, update, and delete integrated applications. They can also disable integrated applications.";

    private static readonly List<(string RoleName, string Description)> Roles =
        new()
        {
            (Shared.Enumerations.AuthorizationRoles.Administrator, AdministratorRoleDescription),
            (Shared.Enumerations.AuthorizationRoles.User, UserRoleDescription),
            (
                Shared.Enumerations.AuthorizationRoles.ApplicationDirectoryManager,
                ApplicationDirectoryManagerDescription
            ),
            (
                Shared.Enumerations.AuthorizationRoles.IntegratedApplicationManager,
                IntegratedApplicationManagerDescription
            ),
        };

    private static void SeedUserRoles(AppDbContext dbContext)
    {
        // Per-role idempotent — never bail on Any(). New roles can be added to the list
        // and will be seeded even when other roles already exist in the database.
        foreach (var roleWithDescription in Roles)
        {
            if (!dbContext.Roles.Any(r => r.Name == roleWithDescription.RoleName))
            {
                dbContext.Roles.Add(new Role
                {
                    Name = roleWithDescription.RoleName,
                    Description = roleWithDescription.Description,
                });
            }
        }
        dbContext.SaveChanges();
    }

    /// <summary>
    /// Idempotent upsert of audit-specific roles. Safe to run after initial seeding because
    /// it checks per-role rather than using an Any() bail-out.
    /// </summary>
    private static void SeedAuditRoles(AppDbContext dbContext)
    {
        var auditRoles = new[]
        {
            (Shared.Enumerations.AuthorizationRoles.TemplateAdmin,          "Audit Template Admin — create, edit, and publish audit templates. Cannot manage users."),
            (Shared.Enumerations.AuthorizationRoles.AuditManager,           "Audit Manager — view and finalize audits in assigned divisions, manage corrective actions, run reports."),
            (Shared.Enumerations.AuthorizationRoles.AuditReviewer,          "Audit Reviewer — reviews submitted audits: can edit responses on UnderReview audits, write review summaries, approve, distribute, and view reports. Cannot manage templates or users."),
            (Shared.Enumerations.AuthorizationRoles.CorrectiveActionOwner,  "Corrective Action Owner — update and close assigned corrective actions only."),
            (Shared.Enumerations.AuthorizationRoles.ReadOnlyViewer,         "Read-Only Viewer — search, view, and export audits. No edit rights."),
            (Shared.Enumerations.AuthorizationRoles.ExecutiveViewer,        "Executive Viewer — access KPI dashboards and summary reports only."),
            // Official user-facing roles
            (Shared.Enumerations.AuthorizationRoles.ITAdmin,    "IT Admin — user management only: add, suspend, and delete users."),
            (Shared.Enumerations.AuthorizationRoles.Auditor,    "Auditor — field auditor: create and submit their own audits, manage CAs they assigned."),
            (Shared.Enumerations.AuthorizationRoles.AuditAdmin, "Audit Admin — full access except user management. Dashboard global/own toggle."),
            (Shared.Enumerations.AuthorizationRoles.Executive,  "Executive — read-only: dashboard, audits, CAs, and reports. No editing."),
            (Shared.Enumerations.AuthorizationRoles.NormalUser, "Normal User — sees only corrective actions assigned to them."),
        };

        foreach (var (name, description) in auditRoles)
        {
            if (!dbContext.Roles.Any(r => r.Name == name))
            {
                dbContext.Roles.Add(new Role { Name = name, Description = description });
            }
        }
        dbContext.SaveChanges();
    }

    private static void SeedDefaultLocalUserInternal(AppDbContext dbContext)
    {
        if (dbContext.Users.Any())
            return;

        var newUser = new User
        {
            AzureAdObjectId = new Guid("00000000-0000-0000-0000-000000000000"),
            FirstName = "Local",
            LastName = "Dev Testing",
            Email = "LocalDevTesting@DevTesting.com",
            Company = "Stronghold",
            Department = "IT - App Dev",
            Title = "Software Developer",
            Active = true,
        };
        
        dbContext.Users.Add(newUser);
        dbContext.SaveChanges();

        var adminRole = dbContext.Roles.FirstOrDefault(r =>
            r.Name == Shared.Enumerations.AuthorizationRoles.Administrator
        );
        
        if (adminRole == null)
        {
            throw new Exception("Admin role not found");
        }

        var userRole = new UserRole { User = newUser, Role = adminRole };
        
        dbContext.UserRoles.Add(userRole);
        dbContext.SaveChanges();
    }

    /// <summary>
    /// Seeds one dummy user per official role for Local dev testing only.
    /// Called from Program.cs Local block — NOT from Initialize() so it never runs in other environments.
    /// Each user gets its role + the base User role. Idempotent (guards per email).
    /// </summary>
    public static void SeedLocalTestUsers(AppDbContext dbContext)
    {
        var testUsers = new[]
        {
            (Email: "itadmin@local.dev",    First: "IT",    Last: "Admin",     Role: Shared.Enumerations.AuthorizationRoles.ITAdmin),
            (Email: "auditor@local.dev",    First: "Field", Last: "Auditor",   Role: Shared.Enumerations.AuthorizationRoles.Auditor),
            (Email: "auditadmin@local.dev", First: "Audit", Last: "Admin",     Role: Shared.Enumerations.AuthorizationRoles.AuditAdmin),
            (Email: "executive@local.dev",  First: "Exec",  Last: "Executive", Role: Shared.Enumerations.AuthorizationRoles.Executive),
            (Email: "normaluser@local.dev", First: "Normal",Last: "User",      Role: Shared.Enumerations.AuthorizationRoles.NormalUser),
        };

        var baseRole = dbContext.Roles.FirstOrDefault(r => r.Name == Shared.Enumerations.AuthorizationRoles.User);

        // Deterministic GUIDs so re-seeding doesn't create duplicates on a fresh DB
        var guids = new[]
        {
            new Guid("10000000-0000-0000-0000-000000000001"),
            new Guid("10000000-0000-0000-0000-000000000002"),
            new Guid("10000000-0000-0000-0000-000000000003"),
            new Guid("10000000-0000-0000-0000-000000000004"),
            new Guid("10000000-0000-0000-0000-000000000005"),
        };

        for (var i = 0; i < testUsers.Length; i++)
        {
            var (email, first, last, roleName) = testUsers[i];

            if (dbContext.Users.Any(u => u.Email == email || u.AzureAdObjectId == guids[i]))
                continue;

            var user = new User
            {
                AzureAdObjectId = guids[i],
                FirstName = first,
                LastName = last,
                Email = email,
                Company = "Stronghold",
                Department = "IT - App Dev",
                Title = roleName,
                Active = true,
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var roleEntity = dbContext.Roles.FirstOrDefault(r => r.Name == roleName);
            if (roleEntity != null)
                dbContext.UserRoles.Add(new UserRole { User = user, Role = roleEntity });

            if (baseRole != null)
                dbContext.UserRoles.Add(new UserRole { User = user, Role = baseRole });

            dbContext.SaveChanges();
        }
    }
}
