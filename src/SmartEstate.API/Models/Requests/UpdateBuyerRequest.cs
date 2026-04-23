namespace SmartEstate.API.Models.Requests;

public record UpdateBuyerRequest(
    string FullName,
    string LifestyleDescription,
    string? Email,
    string? Phone,
    decimal? BudgetMinEur,
    decimal? BudgetMaxEur,
    List<string>? PreferredLocations);
