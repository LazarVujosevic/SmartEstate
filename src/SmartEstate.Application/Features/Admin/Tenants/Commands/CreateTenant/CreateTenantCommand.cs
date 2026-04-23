using ErrorOr;
using MediatR;
using SmartEstate.Application.Features.Admin.Tenants.DTOs;

namespace SmartEstate.Application.Features.Admin.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(string Name, string ContactEmail, string Plan) : IRequest<ErrorOr<TenantDto>>;
