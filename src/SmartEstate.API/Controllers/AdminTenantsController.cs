using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEstate.Application.Common.Models;
using SmartEstate.Application.Features.Admin.Tenants.Commands.CreateTenant;
using SmartEstate.Application.Features.Admin.Tenants.DTOs;
using SmartEstate.Infrastructure.Identity;

namespace SmartEstate.API.Controllers;

[ApiController]
[Route("api/admin/tenants")]
[Authorize(Roles = AppRoles.Administrator)]
public class AdminTenantsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.Match(
            dto => CreatedAtAction(nameof(Create), new { id = dto.Id }, ApiResponse<TenantDto>.Ok(dto)),
            errors => errors.Any(e => e.Type == ErrorType.Conflict)
                ? Conflict(ApiResponse.Fail(errors.First().Description))
                : StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.Fail("An unexpected error occurred.")));
    }
}
