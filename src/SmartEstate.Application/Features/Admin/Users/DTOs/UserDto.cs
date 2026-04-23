namespace SmartEstate.Application.Features.Admin.Users.DTOs;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, string Role, Guid? TenantId);
