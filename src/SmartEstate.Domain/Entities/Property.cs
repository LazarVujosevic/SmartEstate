using SmartEstate.Domain.Common;
using SmartEstate.Domain.Enums;

namespace SmartEstate.Domain.Entities;

public class Property : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public decimal PriceEur { get; set; }
    public double AreaSqm { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
    public List<string> AiTags { get; set; } = [];
    public string? AiSummary { get; set; }
    public DateTime? LastAiProcessedAt { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Available;
    public Guid ListedByAgentId { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<MatchReport> MatchReports { get; set; } = [];
}
