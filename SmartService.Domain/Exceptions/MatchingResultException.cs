namespace SmartService.Domain.Exceptions
{
    // =================== MATCHINGRESULT-RELATED DOMAIN EXCEPTIONS ===================

    /// <summary>
    /// Exception for MatchingResult-related domain issues.
    /// </summary>
    public abstract class MatchingResultException : DomainException
    {
        protected MatchingResultException(string message) : base(message) { }

        public class InvalidMatchingScoreException : MatchingResultException
        {
            public InvalidMatchingScoreException(decimal matchingScore)
                : base($"The MatchingScore '{matchingScore}' must be a value between 0 and 1.") { }
        }

        public class MissingServiceRequestIdException : MatchingResultException
        {
            public MissingServiceRequestIdException()
                : base("The ServiceRequestId cannot be null or empty for a MatchingResult.") { }
        }

        public class MissingSupportedComplexityException : MatchingResultException
        {
            public MissingSupportedComplexityException()
                : base("The MatchingResult must have a SupportedComplexity value defined.") { }
        }
    }
}
