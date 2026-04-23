using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartEstate.Infrastructure.Identity;

namespace SmartEstate.Infrastructure.Persistence;

public class DataSeeder(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdministratorAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(new ApplicationRole(role));
            if (result.Succeeded)
                logger.LogInformation("Role {Role} created", role);
            else
                logger.LogError("Failed to create role {Role}: {Errors}", role,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedAdministratorAsync()
    {
        var email = configuration["ADMIN_EMAIL"];
        var password = configuration["ADMIN_PASSWORD"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "ADMIN_EMAIL or ADMIN_PASSWORD is not configured — skipping administrator seed. " +
                "Set these environment variables before first run in production.");
            return;
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            logger.LogDebug("Administrator user already exists, skipping seed");
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Administrator",
            LastName = string.Empty,
            TenantId = null,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create Administrator user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
        logger.LogInformation("Administrator user seeded: {Email}", email);
    }
}
