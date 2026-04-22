using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartEstate.Application.Common.Models;
using SmartEstate.Infrastructure.Identity;
using System.Security.Claims;

namespace SmartEstate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("User identity not found."));

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound(ApiResponse.Fail("User not found."));

        var profile = new
        {
            user.FirstName,
            user.LastName,
            user.Email
        };

        return Ok(ApiResponse<object>.Ok(profile));
    }
}
