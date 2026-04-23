using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Admin.Tenants.DTOs;

namespace SmartEstate.Application.Features.Admin.Tenants.Commands.SetTenantActive;

public class SetTenantActiveCommandHandler(
    IApplicationDbContext db,
    ITenantCache tenantCache,
    ILogger<SetTenantActiveCommandHandler> logger)
    : IRequestHandler<SetTenantActiveCommand, ErrorOr<TenantDto>>
{
    public async Task<ErrorOr<TenantDto>> Handle(SetTenantActiveCommand request, CancellationToken ct)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, ct);

        if (tenant is null)
            return Error.NotFound(description: $"Tenant {request.TenantId} not found.");

        tenant.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);

        tenantCache.InvalidateTenant(request.TenantId);

        logger.LogInformation("Tenant {TenantId} {Action}",
            request.TenantId, request.IsActive ? "activated" : "deactivated");

        return new TenantDto(tenant.Id, tenant.Name, tenant.ContactEmail, tenant.Plan, tenant.IsActive, tenant.CreatedAt);
    }
}
