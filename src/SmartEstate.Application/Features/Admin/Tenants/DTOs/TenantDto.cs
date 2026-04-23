namespace SmartEstate.Application.Features.Admin.Tenants.DTOs;

public record TenantDto(Guid Id, string Name, string ContactEmail, string? Plan, bool IsActive, DateTime CreatedAt);
