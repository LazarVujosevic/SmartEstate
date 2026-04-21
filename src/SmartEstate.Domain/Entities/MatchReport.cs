using SmartEstate.Domain.Common;
using SmartEstate.Domain.Enums;

namespace SmartEstate.Domain.Entities;

public class MatchReport : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid PropertyId { get; set; }
    public double MatchScore { get; set; }
    public BuyerReaction? Reaction { get; set; }
    public string? AgentNotes { get; set; }
    public DateTime? ReactionRecordedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Buyer Buyer { get; set; } = null!;
    public Property Property { get; set; } = null!;
}
