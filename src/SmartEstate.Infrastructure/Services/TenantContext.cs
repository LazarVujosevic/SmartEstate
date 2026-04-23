using SmartEstate.Application.Common.Interfaces;

namespace SmartEstate.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public bool IsAdministrator { get; set; }
}
