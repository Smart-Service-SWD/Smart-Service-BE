namespace SmartService.Domain.Exceptions
{
    /// <summary>
    /// Represents a domain-level business rule violation.
    /// 
    /// DomainException is thrown when an operation
    /// violates business invariants or domain constraints.
    /// 
    /// This exception should NOT be used for:
    /// - Infrastructure errors
    /// - Validation errors from UI
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}