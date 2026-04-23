using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SmartEstate.Web.Models.Common;

namespace SmartEstate.Web.Services;

public class ApiClient(HttpClient http, ILocalStorageService localStorage, NavigationManager nav)
{
    private const string TokenKey = "smartestate_jwt";

    public Task<ApiResponse<T>?> GetAsync<T>(string url) =>
        SendAsync<T>(HttpMethod.Get, url, body: null);

    public Task<ApiResponse<T>?> PostAsync<T>(string url, object body) =>
        SendAsync<T>(HttpMethod.Post, url, body);

    public Task<ApiResponse<T>?> PutAsync<T>(string url, object body) =>
        SendAsync<T>(HttpMethod.Put, url, body);

    public Task<ApiResponse<T>?> PatchAsync<T>(string url, object body) =>
        SendAsync<T>(HttpMethod.Patch, url, body);

    public Task<ApiResponse<T>?> DeleteAsync<T>(string url) =>
        SendAsync<T>(HttpMethod.Delete, url, body: null);

    private async Task<ApiResponse<T>?> SendAsync<T>(HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);

        var token = await localStorage.GetItemAsStringAsync(TokenKey);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        try
        {
            var response = await http.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await localStorage.RemoveItemAsync(TokenKey);
                nav.NavigateTo("/login", forceLoad: false);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }
        catch
        {
            return null;
        }
    }
}
