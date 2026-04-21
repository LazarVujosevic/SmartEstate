using SmartEstate.Domain.Common;

namespace SmartEstate.Domain.Entities;

public class Buyer : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string LifestyleDescription { get; set; } = string.Empty;
    public List<string> AiTags { get; set; } = [];
    public string? AiProfile { get; set; }
    public DateTime? LastAiProcessedAt { get; set; }
    public Guid AssignedAgentId { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<MatchReport> MatchReports { get; set; } = [];
}
