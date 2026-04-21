using SmartEstate.Domain.Common;

namespace SmartEstate.Domain.Entities;

public class InAppNotification : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public Guid? UserId { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
