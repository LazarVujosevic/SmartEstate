namespace SmartEstate.Web.Models.Buyers;

public class CreateBuyerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string LifestyleDescription { get; set; } = string.Empty;
    public decimal? BudgetMinEur { get; set; }
    public decimal? BudgetMaxEur { get; set; }
    public List<string> PreferredLocations { get; set; } = [];
}
