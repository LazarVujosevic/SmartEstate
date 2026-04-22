using Microsoft.AspNetCore.Http;
using SmartEstate.Application.Common.Interfaces;

namespace SmartEstate.Infrastructure.Services;

public class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid? TenantId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst("tenant_id");
            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public bool IsAdministrator =>
        httpContextAccessor.HttpContext?.User.IsInRole("Administrator") ?? false;
}
