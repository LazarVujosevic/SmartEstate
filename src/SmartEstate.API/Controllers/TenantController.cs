using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Common.Models;

namespace SmartEstate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantController(ITenantContext tenantContext, IApplicationDbContext db) : ControllerBase
{
    [HttpGet("info")]
    public async Task<IActionResult> GetInfo(CancellationToken ct)
    {
        if (tenantContext.TenantId is null)
            return Forbid();

        var tenant = await db.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenantContext.TenantId.Value)
            .Select(t => new { t.Id, t.Name })
            .FirstOrDefaultAsync(ct);

        if (tenant is null)
            return NotFound(ApiResponse.Fail("Tenant not found."));

        return Ok(ApiResponse<object>.Ok(tenant));
    }
}
