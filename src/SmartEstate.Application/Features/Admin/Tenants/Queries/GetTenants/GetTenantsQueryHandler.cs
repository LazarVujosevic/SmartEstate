using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Admin.Tenants.DTOs;

namespace SmartEstate.Application.Features.Admin.Tenants.Queries.GetTenants;

public class GetTenantsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTenantsQuery, ErrorOr<List<TenantDto>>>
{
    public async Task<ErrorOr<List<TenantDto>>> Handle(GetTenantsQuery request, CancellationToken ct)
    {
        var tenants = await db.Tenants
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TenantDto(t.Id, t.Name, t.ContactEmail, t.Plan, t.IsActive, t.CreatedAt))
            .ToListAsync(ct);

        return tenants;
    }
}
