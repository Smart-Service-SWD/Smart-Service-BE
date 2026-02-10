namespace SmartService.Domain.Exceptions
{
    /// <summary>
    /// Represents a base exception for domain-related issues.
    /// Ensures a consistent structure for exceptions related to domain-level invariants.
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }
}