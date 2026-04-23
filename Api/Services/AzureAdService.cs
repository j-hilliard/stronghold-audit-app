using Azure.Identity;
using Microsoft.Graph;
using Stronghold.AppDashboard.Api.Models;

namespace Stronghold.AppDashboard.Api.Services;

public class AzureAdHelper
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<AzureAdHelper> _logger;

    public AzureAdHelper(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<AzureAdHelper> logger
    )
    {
        _logger = logger;

        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var appRegSecret = configuration["AppRegSecretForGraphAccess"];

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
        {
            if (environment.IsEnvironment("Local"))
            {
                _logger.LogWarning(
                    "AzureAdHelper: TenantId/ClientId missing in Local environment. Graph lookups are disabled."
                );
                _graphClient = null!;
                return;
            }

            throw new ArgumentException(
                "TenantId/ClientId are not configured correctly. Please check your AzureAd configuration."
            );
        }

        if (string.IsNullOrWhiteSpace(appRegSecret))
        {
            // Local dev — Graph access not configured; methods will return null.
            _logger.LogInformation(
                "AzureAdHelper: AppRegSecretForGraphAccess is not set. Graph lookups are disabled."
            );
            _graphClient = null!;
            return;
        }

        var credentials = new ClientSecretCredential(tenantId, clientId, appRegSecret);
        _graphClient = new GraphServiceClient(credentials);
    }

    public async Task<AdUserInfo?> GetAdUserInfoByObjectIdAsync(Guid objectId)
    {
        if (objectId == Guid.Empty || _graphClient == null)
            return null;

        try
        {
            var user = await _graphClient
                .Users[objectId.ToString()]
                .Request()
                .Select(
                    "id,displayName,givenName,surname,jobTitle,mail,userPrincipalName,department,companyName,employeeId,mailNickname,accountEnabled,createdDateTime"
                )
                .GetAsync();

            if (user != null)
            {
                return new AdUserInfo
                {
                    AzureAdObjectId = Guid.Parse(user.Id),
                    DisplayName = user.DisplayName,
                    FirstName = user.GivenName,
                    LastName = user.Surname,
                    Title = user.JobTitle,
                    Email = user.Mail,
                    UserPrincipalName = user.UserPrincipalName,
                    Department = user.Department,
                    Company = user.CompanyName,
                    EmployeeId = user.EmployeeId,
                    MailNickName = user.MailNickname,
                    AccountCreated = user.CreatedDateTime?.ToString("o"),
                    SamAccountName = user.OnPremisesSamAccountName,
                    CN = user.DisplayName,
                    Name = user.DisplayName,
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching user info: {ex.Message}");
        }

        return null;
    }

    public async Task<AdGroupInfo?> GetAdGroupInfoByGroupNameAsync(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName) || _graphClient == null)
            return null;

        try
        {
            var groups = await _graphClient
                .Groups.Request()
                .Filter($"displayName eq '{groupName}'")
                .GetAsync();

            var group = groups?.CurrentPage?[0];

            if (group != null)
            {
                return new AdGroupInfo
                {
                    GroupAzureAdObjectId = Guid.Parse(group.Id),
                    CN = group.DisplayName,
                    Description = group.Description,
                    SamAccountName = group.MailNickname,
                    WhenCreated = group.CreatedDateTime?.ToString("o"),
                    GroupType =
                        group.SecurityEnabled.HasValue && group.SecurityEnabled.Value
                            ? "Security Group"
                        : group.MailEnabled.HasValue && group.MailEnabled.Value
                            ? "Mail-Enabled Group"
                        : "Unknown",
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching group info: {ex.Message}");
        }

        return null;
    }

    public async Task<List<AdUserInfo>> GetAdGroupMembersByGroupObjectIdAsync(Guid groupObjectId)
    {
        var users = new List<AdUserInfo>();

        if (groupObjectId == Guid.Empty || _graphClient == null)
            return users;

        try
        {
            var members = await _graphClient
                .Groups[groupObjectId.ToString()]
                .Members.Request()
                .Select(
                    "id,displayName,givenName,surname,mail,userPrincipalName,department,companyName,employeeId,mailNickname,jobTitle,createdDateTime"
                )
                .GetAsync();

            foreach (var member in members.CurrentPage)
            {
                if (member is Microsoft.Graph.User user)
                {
                    users.Add(
                        new AdUserInfo
                        {
                            AzureAdObjectId = Guid.Parse(user.Id),
                            DisplayName = user.DisplayName,
                            FirstName = user.GivenName,
                            LastName = user.Surname,
                            Title = user.JobTitle,
                            Email = user.Mail,
                            UserPrincipalName = user.UserPrincipalName,
                            Department = user.Department,
                            Company = user.CompanyName,
                            EmployeeId = user.EmployeeId,
                            MailNickName = user.MailNickname,
                            AccountCreated = user.CreatedDateTime?.ToString("o"),
                            SamAccountName = user.UserPrincipalName,
                            Name = user.DisplayName,
                        }
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching group members: {ex.Message}");
        }

        return users;
    }
}
