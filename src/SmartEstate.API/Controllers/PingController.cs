using Microsoft.AspNetCore.Mvc;
using SmartEstate.Application.Common.Models;

namespace SmartEstate.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiResponse<string>.Ok("pong"));
    }
}
