namespace SmartEstate.Application.Features.Buyers.DTOs;

public class BuyerListItemDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public decimal? BudgetMinEur { get; init; }
    public decimal? BudgetMaxEur { get; init; }
    public List<string> PreferredLocations { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}
