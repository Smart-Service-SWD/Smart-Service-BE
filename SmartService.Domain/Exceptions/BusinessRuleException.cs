namespace SmartService.Domain.Exceptions;

/// <summary>
/// Represents a validation or business rule violation.
/// 
/// BusinessRuleException is thrown when input data
/// fails validation rules or business constraints.
/// 
/// This is distinct from DomainException in that it
/// represents issues with INPUT VALIDATION rather than
/// business logic invariant violations during execution.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }

    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException) { }
}
