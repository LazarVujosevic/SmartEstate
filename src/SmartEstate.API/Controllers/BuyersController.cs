using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEstate.API.Models.Requests;
using SmartEstate.Application.Common.Models;
using SmartEstate.Application.Features.Buyers.Commands.CreateBuyer;
using SmartEstate.Application.Features.Buyers.DTOs;
using SmartEstate.Application.Features.Buyers.Commands.DeleteBuyer;
using SmartEstate.Application.Features.Buyers.Commands.UpdateBuyer;
using SmartEstate.Application.Features.Buyers.Queries.GetBuyer;
using SmartEstate.Application.Features.Buyers.Queries.GetBuyers;
using SmartEstate.Infrastructure.Identity;
using System.Security.Claims;

namespace SmartEstate.API.Controllers;

[ApiController]
[Route("api/buyers")]
[Authorize(Roles = $"{AppRoles.Agent},{AppRoles.AgencyManager}")]
public class BuyersController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBuyerRequest body, CancellationToken ct)
    {
        var agentId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateBuyerCommand(
            agentId,
            body.FullName,
            body.LifestyleDescription,
            body.Email,
            body.Phone,
            body.BudgetMinEur,
            body.BudgetMaxEur,
            body.PreferredLocations);

        var result = await mediator.Send(command, ct);
        return result.Match(
            dto => CreatedAtAction(nameof(GetById), new { id = dto.Id }, ApiResponse<BuyerDto>.Ok(dto)),
            errors => MapErrors(errors));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetBuyersQuery(pageNumber, pageSize, search), ct);
        return result.Match(
            paged => Ok(ApiResponse<PagedResult<BuyerListItemDto>>.Ok(paged)),
            errors => MapErrors(errors));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBuyerQuery(id), ct);
        return result.Match(
            dto => Ok(ApiResponse<BuyerDto>.Ok(dto)),
            errors => MapErrors(errors));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuyerRequest body, CancellationToken ct)
    {
        var command = new UpdateBuyerCommand(
            id,
            body.FullName,
            body.LifestyleDescription,
            body.Email,
            body.Phone,
            body.BudgetMinEur,
            body.BudgetMaxEur,
            body.PreferredLocations);

        var result = await mediator.Send(command, ct);
        return result.Match(
            dto => Ok(ApiResponse<BuyerDto>.Ok(dto)),
            errors => MapErrors(errors));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteBuyerCommand(id), ct);
        return result.Match(
            _ => NoContent(),
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
