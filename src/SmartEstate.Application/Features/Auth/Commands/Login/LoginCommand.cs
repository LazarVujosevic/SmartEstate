using ErrorOr;
using MediatR;
using SmartEstate.Application.Features.Auth.DTOs;

namespace SmartEstate.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<ErrorOr<LoginResponseDto>>;
