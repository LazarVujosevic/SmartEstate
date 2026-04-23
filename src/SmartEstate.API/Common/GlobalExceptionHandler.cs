using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using SmartEstate.Application.Common.Models;

namespace SmartEstate.API.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors.Select(e => e.ErrorMessage).ToList();
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse.Fail("Validation failed.", errors), ct);
            return true;
        }

        logger.LogError(exception, "Unhandled exception");
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.Fail("An unexpected error occurred."), ct);
        return true;
    }
}
