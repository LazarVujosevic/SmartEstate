namespace SmartEstate.Web.Models.Buyers;

public class BuyerDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string LifestyleDescription { get; init; } = string.Empty;
    public decimal? BudgetMinEur { get; init; }
    public decimal? BudgetMaxEur { get; init; }
    public List<string> PreferredLocations { get; init; } = [];
    public Guid AssignedAgentId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
