namespace SmartService.Domain.Exceptions
{
    /// <summary>
    /// Exception for ServiceComplexity-related domain issues.
    /// </summary>
    public abstract class ServiceComplexityException : DomainException
    {
        protected ServiceComplexityException(string message) : base(message) { }

        /// <summary>
        /// Exception for invalid complexity levels.
        /// </summary>
        public class InvalidComplexityLevelException : ServiceComplexityException
        {
            public InvalidComplexityLevelException(int level)
                : base($"Invalid complexity level '{level}'. Complexity must be between 1 and 5.") { }
        }
    }
}
