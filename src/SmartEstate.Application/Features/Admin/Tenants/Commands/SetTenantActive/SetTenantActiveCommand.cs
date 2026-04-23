using ErrorOr;
using MediatR;
using SmartEstate.Application.Features.Admin.Tenants.DTOs;

namespace SmartEstate.Application.Features.Admin.Tenants.Commands.SetTenantActive;

public record SetTenantActiveCommand(Guid TenantId, bool IsActive) : IRequest<ErrorOr<TenantDto>>;
