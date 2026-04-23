using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartEstate.Application.Common.Constants;
using SmartEstate.Infrastructure.Identity;
using SmartEstate.Infrastructure.Services;

namespace SmartEstate.Infrastructure.MultiTenancy;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, ILogger<TenantMiddleware> logger)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst(AppClaims.TenantId);
            if (tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                tenantContext.TenantId = tenantId;
                logger.LogDebug("Resolved TenantId {TenantId} for request {Path}", tenantId, context.Request.Path);
            }

            tenantContext.IsAdministrator = context.User.IsInRole(AppRoles.Administrator);
        }

        await next(context);
    }
}
