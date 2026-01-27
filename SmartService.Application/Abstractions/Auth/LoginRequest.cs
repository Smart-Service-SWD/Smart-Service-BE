namespace SmartService.Application.Abstractions.Auth;

/// <summary>
/// Request model for user login.
/// </summary>
public record LoginRequest(
    string Email,
    string Password);
