namespace SmartEstate.Web.Models.Auth;

public record LoginResponseDto(string Token, DateTimeOffset ExpiresAt, string Role, Guid? TenantId);
