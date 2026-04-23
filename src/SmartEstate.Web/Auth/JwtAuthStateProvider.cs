using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace SmartEstate.Web.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "smartestate_jwt";
    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ILocalStorageService _localStorage;

    public JwtAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync(TokenKey);
        if (string.IsNullOrWhiteSpace(token))
            return AnonymousState;

        var claims = ParseClaimsFromJwt(token);
        if (IsTokenExpired(claims))
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            return AnonymousState;
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task Login(string token)
    {
        await _localStorage.SetItemAsStringAsync(TokenKey, token);
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = DecodeBase64Url(payload);
        var kvp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)!;

        var claims = new List<Claim>();
        foreach (var (key, value) in kvp)
        {
            var type = key switch
            {
                "sub" => ClaimTypes.NameIdentifier,
                "email" => ClaimTypes.Email,
                "role" => ClaimTypes.Role,
                _ => key
            };

            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in value.EnumerateArray())
                    claims.Add(new Claim(type, el.GetString() ?? ""));
            }
            else
            {
                claims.Add(new Claim(type, value.GetString() ?? value.GetRawText()));
            }
        }

        return claims;
    }

    private static bool IsTokenExpired(IEnumerable<Claim> claims)
    {
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is null || !long.TryParse(expClaim.Value, out var exp))
            return true;

        return DateTimeOffset.FromUnixTimeSeconds(exp) < DateTimeOffset.UtcNow;
    }

    private static byte[] DecodeBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
