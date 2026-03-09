namespace SmartService.API.Contracts;

/// <summary>
/// Simple JSON input model for money values.
/// The API layer maps this DTO into the domain Money value object.
/// </summary>
public record MoneyInput(decimal Amount, string Currency);
