using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Common.Models;

namespace SmartEstate.Infrastructure.MultiTenancy;

public class InactiveTenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        IApplicationDbContext db,
        IMemoryCache cache,
        ILogger<InactiveTenantMiddleware> logger)
    {
        // Skip: unauthenticated, Administrator (no TenantId), or health/ping endpoints
        if (tenantContext.TenantId is null
            || context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.StartsWithSegments("/ping", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var tenantId = tenantContext.TenantId.Value;
        var cacheKey = $"tenant_active_{tenantId}";

        if (!cache.TryGetValue(cacheKey, out bool isActive))
        {
            isActive = await db.Tenants
                .AsNoTracking()
                .Where(t => t.Id == tenantId)
                .Select(t => t.IsActive)
                .FirstOrDefaultAsync(context.RequestAborted);

            cache.Set(cacheKey, isActive, TimeSpan.FromSeconds(60));
        }

        if (!isActive)
        {
            logger.LogWarning("Blocked request from inactive tenant {TenantId} to {Path}", tenantId, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(
                ApiResponse.Fail("Tenant account is inactive. Please contact your administrator."),
                context.RequestAborted);
            return;
        }

        await next(context);
    }
}
