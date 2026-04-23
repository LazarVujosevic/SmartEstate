using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Admin.Tenants.DTOs;
using SmartEstate.Domain.Entities;

namespace SmartEstate.Application.Features.Admin.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler(IApplicationDbContext db, ILogger<CreateTenantCommandHandler> logger)
    : IRequestHandler<CreateTenantCommand, ErrorOr<TenantDto>>
{
    public async Task<ErrorOr<TenantDto>> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        var exists = await db.Tenants.AnyAsync(t => t.Name == request.Name, ct);
        if (exists)
            return Error.Conflict(description: $"A tenant with name '{request.Name}' already exists.");

        var tenant = new Tenant
        {
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            Plan = request.Plan,
            IsActive = false
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Tenant {TenantId} ({TenantName}) created with plan {Plan}",
            tenant.Id, tenant.Name, tenant.Plan);

        return new TenantDto(tenant.Id, tenant.Name, tenant.ContactEmail, tenant.Plan, tenant.IsActive, tenant.CreatedAt);
    }
}
