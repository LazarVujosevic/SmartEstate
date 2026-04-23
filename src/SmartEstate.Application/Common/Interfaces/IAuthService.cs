using ErrorOr;
using SmartEstate.Application.Features.Auth.DTOs;

namespace SmartEstate.Application.Common.Interfaces;

public interface IAuthService
{
    Task<ErrorOr<LoginResponseDto>> LoginAsync(string email, string password, CancellationToken ct);
}
