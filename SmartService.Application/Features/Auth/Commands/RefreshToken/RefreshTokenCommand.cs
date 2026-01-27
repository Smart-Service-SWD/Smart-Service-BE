using MediatR;
using SmartService.Application.Abstractions.Auth;

namespace SmartService.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh access token.
/// </summary>
public record RefreshTokenCommand(
    string RefreshToken) : IRequest<AuthResult>;
