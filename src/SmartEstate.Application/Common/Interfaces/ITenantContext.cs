namespace SmartEstate.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    bool IsAdministrator { get; }
}
