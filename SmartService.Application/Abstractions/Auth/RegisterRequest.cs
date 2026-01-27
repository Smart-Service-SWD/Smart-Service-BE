namespace SmartService.Application.Abstractions.Auth;

/// <summary>
/// Request model for user registration.
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string PhoneNumber,
    Domain.Entities.UserRole Role);
