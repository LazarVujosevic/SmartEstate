using Microsoft.Extensions.Caching.Memory;
using SmartEstate.Application.Common.Interfaces;

namespace SmartEstate.Infrastructure.MultiTenancy;

public class TenantCache(IMemoryCache cache) : ITenantCache
{
    public void InvalidateTenant(Guid tenantId)
        => cache.Remove($"tenant_active_{tenantId}");
}
