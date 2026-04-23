namespace SmartEstate.Web.Models.Admin;

public class TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string? Plan { get; init; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; init; }
}
