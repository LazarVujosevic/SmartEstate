using System.Net;
using System.Net.Http.Json;
using SmartEstate.Web.Auth;
using SmartEstate.Web.Models.Auth;
using SmartEstate.Web.Models.Common;

namespace SmartEstate.Web.Services;

public enum LoginResult { Success, Unauthorized, Error }

public class AuthService(HttpClient http, JwtAuthStateProvider authProvider)
{
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await http.PostAsJsonAsync("api/auth/login", new { email, password });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return LoginResult.Unauthorized;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
            if (apiResponse?.Success != true || apiResponse.Data is null)
                return LoginResult.Error;

            await authProvider.Login(apiResponse.Data.Token);
            return LoginResult.Success;
        }
        catch
        {
            return LoginResult.Error;
        }
    }

    public Task LogoutAsync() => authProvider.Logout();
}
