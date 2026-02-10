using MediatR;
using SmartService.Application.Abstractions.Auth;

namespace SmartService.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user.
/// </summary>
public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string PhoneNumber,
    Domain.Entities.UserRole Role) : IRequest<AuthResult>;
