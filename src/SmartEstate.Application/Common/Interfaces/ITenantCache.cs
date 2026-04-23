namespace SmartEstate.Application.Common.Interfaces;

public interface ITenantCache
{
    void InvalidateTenant(Guid tenantId);
}
