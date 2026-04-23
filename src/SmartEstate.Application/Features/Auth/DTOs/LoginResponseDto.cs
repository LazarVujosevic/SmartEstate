namespace SmartEstate.Application.Features.Auth.DTOs;

public record LoginResponseDto(string Token, DateTimeOffset ExpiresAt, string Role, Guid? TenantId);
