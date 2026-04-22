using Microsoft.AspNetCore.Http;
using SmartEstate.Application.Common.Constants;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Infrastructure.Identity;

namespace SmartEstate.Infrastructure.Services;

public class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid? TenantId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst(AppClaims.TenantId);
            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public bool IsAdministrator =>
        httpContextAccessor.HttpContext?.User.IsInRole(AppRoles.Administrator) ?? false;
}
