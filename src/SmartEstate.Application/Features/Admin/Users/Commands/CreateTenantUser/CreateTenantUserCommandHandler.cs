using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Admin.Users.DTOs;

namespace SmartEstate.Application.Features.Admin.Users.Commands.CreateTenantUser;

public class CreateTenantUserCommandHandler(IApplicationDbContext db, IUserManagementService userService)
    : IRequestHandler<CreateTenantUserCommand, ErrorOr<UserDto>>
{
    public async Task<ErrorOr<UserDto>> Handle(CreateTenantUserCommand request, CancellationToken ct)
    {
        var tenantExists = await db.Tenants.AnyAsync(t => t.Id == request.TenantId, ct);
        if (!tenantExists)
            return Error.NotFound(description: $"Tenant {request.TenantId} not found.");

        return await userService.CreateAgencyManagerAsync(
            request.TenantId, request.Email, request.Password,
            request.FirstName, request.LastName, ct);
    }
}
