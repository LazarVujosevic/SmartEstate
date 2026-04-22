using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using SmartEstate.Application.Common.Models;

namespace SmartEstate.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var data = new
        {
            AppName = "SmartEstate API",
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
            Timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(data, "Healthy"));
    }
}
