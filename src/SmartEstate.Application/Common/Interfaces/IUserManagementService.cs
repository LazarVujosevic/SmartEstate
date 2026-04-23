using ErrorOr;
using SmartEstate.Application.Features.Admin.Users.DTOs;

namespace SmartEstate.Application.Common.Interfaces;

public interface IUserManagementService
{
    Task<ErrorOr<UserDto>> CreateAgencyManagerAsync(
        Guid tenantId, string email, string password,
        string firstName, string lastName, CancellationToken ct);
}
