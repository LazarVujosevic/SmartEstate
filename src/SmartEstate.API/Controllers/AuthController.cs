using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartEstate.Application.Common.Models;
using SmartEstate.Application.Features.Auth.Commands.Login;
using SmartEstate.Application.Features.Auth.DTOs;

namespace SmartEstate.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.Match(
            dto => Ok(ApiResponse<LoginResponseDto>.Ok(dto)),
            errors => errors.Any(e => e.Type == ErrorType.Unauthorized)
                ? Unauthorized(ApiResponse.Fail("Invalid credentials."))
                : Problem());
    }
}
