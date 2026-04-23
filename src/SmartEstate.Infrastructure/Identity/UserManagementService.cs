using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Admin.Users.DTOs;

namespace SmartEstate.Infrastructure.Identity;

public class UserManagementService(
    UserManager<ApplicationUser> userManager,
    ILogger<UserManagementService> logger)
    : IUserManagementService
{
    public async Task<ErrorOr<UserDto>> CreateAgencyManagerAsync(
        Guid tenantId, string email, string password,
        string firstName, string lastName, CancellationToken ct)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return Error.Conflict(description: $"Email '{email}' is already registered.");

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            TenantId = tenantId,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => Error.Validation(code: e.Code, description: e.Description))
                .ToList();
            return errors;
        }

        await userManager.AddToRoleAsync(user, AppRoles.AgencyManager);

        logger.LogInformation("AgencyManager {UserId} ({Email}) created for tenant {TenantId}",
            user.Id, email, tenantId);

        return new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, AppRoles.AgencyManager, user.TenantId);
    }
}
