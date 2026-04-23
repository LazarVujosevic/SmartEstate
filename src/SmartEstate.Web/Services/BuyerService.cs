using SmartEstate.Web.Models.Buyers;
using SmartEstate.Web.Models.Common;

namespace SmartEstate.Web.Services;

public class BuyerService(ApiClient api)
{
    public async Task<PagedResult<BuyerListItemDto>?> GetListAsync(int pageNumber, int pageSize, string? search = null)
    {
        var url = $"api/buyers?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await api.GetAsync<PagedResult<BuyerListItemDto>>(url);
        return response?.Success == true ? response.Data : null;
    }

    public async Task<BuyerDto?> GetByIdAsync(Guid id)
    {
        var response = await api.GetAsync<BuyerDto>($"api/buyers/{id}");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<(BuyerDto? buyer, string? error)> CreateAsync(CreateBuyerRequest request)
    {
        var response = await api.PostAsync<BuyerDto>("api/buyers", request);
        if (response?.Success == true && response.Data is not null)
            return (response.Data, null);

        return (null, response?.Message ?? "Failed to create buyer.");
    }

    public async Task<(BuyerDto? buyer, string? error)> UpdateAsync(Guid id, UpdateBuyerRequest request)
    {
        var response = await api.PutAsync<BuyerDto>($"api/buyers/{id}", request);
        if (response?.Success == true && response.Data is not null)
            return (response.Data, null);

        return (null, response?.Message ?? "Failed to update buyer.");
    }

    public async Task<(bool success, string? error)> DeleteAsync(Guid id)
    {
        var response = await api.DeleteAsync<object>($"api/buyers/{id}");
        if (response?.Success == true)
            return (true, null);

        return (false, response?.Message ?? "Failed to delete buyer.");
    }
}
