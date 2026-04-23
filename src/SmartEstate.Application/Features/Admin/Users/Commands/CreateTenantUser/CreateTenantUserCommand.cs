using ErrorOr;
using MediatR;
using SmartEstate.Application.Features.Admin.Users.DTOs;

namespace SmartEstate.Application.Features.Admin.Users.Commands.CreateTenantUser;

public record CreateTenantUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<ErrorOr<UserDto>>;
