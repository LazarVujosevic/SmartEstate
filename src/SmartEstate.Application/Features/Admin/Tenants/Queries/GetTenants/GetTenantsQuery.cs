using ErrorOr;
using MediatR;
using SmartEstate.Application.Features.Admin.Tenants.DTOs;

namespace SmartEstate.Application.Features.Admin.Tenants.Queries.GetTenants;

public record GetTenantsQuery : IRequest<ErrorOr<List<TenantDto>>>;
