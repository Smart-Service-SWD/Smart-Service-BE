using MediatR;
using SmartService.Application.Abstractions.Auth;

namespace SmartService.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to login a user.
/// </summary>
public record LoginCommand(
    string Email,
    string Password) : IRequest<AuthResult>;
