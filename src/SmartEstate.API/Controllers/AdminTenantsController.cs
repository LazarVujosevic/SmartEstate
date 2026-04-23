using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEstate.API.Models.Requests;
using SmartEstate.Application.Common.Models;
using SmartEstate.Application.Features.Admin.Tenants.Commands.CreateTenant;
using SmartEstate.Application.Features.Admin.Tenants.Commands.SetTenantActive;
using SmartEstate.Application.Features.Admin.Tenants.DTOs;
using SmartEstate.Application.Features.Admin.Users.Commands.CreateTenantUser;
using SmartEstate.Application.Features.Admin.Users.DTOs;
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
            errors => MapErrors(errors));
    }

    [HttpPost("{tenantId:guid}/users")]
    public async Task<IActionResult> CreateUser(
        Guid tenantId,
        [FromBody] CreateTenantUserRequest body,
        CancellationToken ct)
    {
        var command = new CreateTenantUserCommand(tenantId, body.Email, body.Password, body.FirstName, body.LastName);
        var result = await mediator.Send(command, ct);
        return result.Match(
            dto => CreatedAtAction(nameof(CreateUser), new { tenantId, id = dto.Id }, ApiResponse<UserDto>.Ok(dto)),
            errors => MapErrors(errors));
    }

    [HttpPatch("{tenantId:guid}/activate")]
    public async Task<IActionResult> SetActive(
        Guid tenantId,
        [FromBody] SetTenantActiveRequest body,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SetTenantActiveCommand(tenantId, body.IsActive), ct);
        return result.Match(
            dto => Ok(ApiResponse<TenantDto>.Ok(dto)),
            errors => MapErrors(errors));
    }

    private IActionResult MapErrors(List<Error> errors)
    {
        if (errors.Any(e => e.Type == ErrorType.NotFound))
            return NotFound(ApiResponse.Fail(errors.First(e => e.Type == ErrorType.NotFound).Description));
        if (errors.Any(e => e.Type == ErrorType.Conflict))
            return Conflict(ApiResponse.Fail(errors.First(e => e.Type == ErrorType.Conflict).Description));
        if (errors.Any(e => e.Type == ErrorType.Validation))
        {
            var messages = errors.Where(e => e.Type == ErrorType.Validation).Select(e => e.Description).ToList();
            return BadRequest(ApiResponse.Fail("Validation failed.", messages));
        }
        return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.Fail("An unexpected error occurred."));
    }
}
