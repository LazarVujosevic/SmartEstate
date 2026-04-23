using SmartEstate.Web.Models.Admin;
using SmartEstate.Web.Models.Common;

namespace SmartEstate.Web.Services;

public class TenantAdminService(ApiClient api)
{
    public async Task<List<TenantDto>?> GetAllAsync()
    {
        var response = await api.GetAsync<List<TenantDto>>("api/admin/tenants");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<(TenantDto? tenant, string? error)> CreateAsync(string name, string contactEmail, string plan)
    {
        var response = await api.PostAsync<TenantDto>("api/admin/tenants",
            new { name, contactEmail, plan });

        if (response?.Success == true && response.Data is not null)
            return (response.Data, null);

        return (null, response?.Message ?? "Failed to create tenant.");
    }

    public async Task<(TenantDto? tenant, string? error)> SetActiveAsync(Guid id, bool isActive)
    {
        var response = await api.PatchAsync<TenantDto>($"api/admin/tenants/{id}/activate",
            new { isActive });

        if (response?.Success == true && response.Data is not null)
            return (response.Data, null);

        return (null, response?.Message ?? "Failed to update tenant status.");
    }
}
