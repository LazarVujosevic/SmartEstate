namespace SmartEstate.API.Models.Requests;

public record CreateTenantUserRequest(string Email, string Password, string FirstName, string LastName);
