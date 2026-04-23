using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartEstate.Application.Common.Constants;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Auth.DTOs;

namespace SmartEstate.Infrastructure.Identity;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtOptions,
    ILogger<AuthService> logger)
    : IAuthService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<ErrorOr<LoginResponseDto>> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            logger.LogWarning("Failed login attempt for {Email}", email);
            return Error.Unauthorized(description: "Invalid credentials.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (string.IsNullOrEmpty(role))
        {
            logger.LogWarning("User {UserId} has no assigned role", user.Id);
            return Error.Unauthorized(description: "Invalid credentials.");
        }

        var (token, expiresAt) = GenerateJwt(user, role);

        logger.LogInformation("User {UserId} logged in with role {Role}", user.Id, role);

        return new LoginResponseDto(token, expiresAt, role, user.TenantId);
    }

    private (string Token, DateTimeOffset ExpiresAt) GenerateJwt(ApplicationUser user, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (user.TenantId.HasValue)
            claims.Add(new(AppClaims.TenantId, user.TenantId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds);

        return (TokenHandler.WriteToken(token), expiresAt);
    }
}
