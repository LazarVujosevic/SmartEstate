using ErrorOr;
using MediatR;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Auth.DTOs;

namespace SmartEstate.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(IAuthService authService)
    : IRequestHandler<LoginCommand, ErrorOr<LoginResponseDto>>
{
    public Task<ErrorOr<LoginResponseDto>> Handle(LoginCommand request, CancellationToken ct)
        => authService.LoginAsync(request.Email, request.Password, ct);
}
