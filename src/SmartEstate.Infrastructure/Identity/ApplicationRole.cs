using Microsoft.AspNetCore.Identity;

namespace SmartEstate.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string AgencyManager = "AgencyManager";
    public const string Agent = "Agent";

    public static readonly string[] All = [Administrator, AgencyManager, Agent];
}
