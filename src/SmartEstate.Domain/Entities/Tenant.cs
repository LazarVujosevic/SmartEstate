using SmartEstate.Domain.Common;

namespace SmartEstate.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? Plan { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime? ActivatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}
