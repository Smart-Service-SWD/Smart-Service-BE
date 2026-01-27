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

    // ==================== ACTIVITYLOG-RELATED DOMAIN EXCEPTIONS ====================

    /// <summary>
    /// Exception for ActivityLog-related domain issues.
    /// </summary>
    public abstract class ActivityLogException : DomainException
    {
        protected ActivityLogException(string message) : base(message) { }

        public class MissingServiceRequestIdException : ActivityLogException
        {
            public MissingServiceRequestIdException()
                : base("The ServiceRequestId field cannot be null or empty in an ActivityLog.") { }
        }

        public class InvalidActionException : ActivityLogException
        {
            public InvalidActionException()
                : base("The Action field in an ActivityLog cannot be null, empty, or exceed 250 characters.") { }
        }

        public class InvalidTimestampException : ActivityLogException
        {
            public InvalidTimestampException(DateTime timestamp)
                : base($"The timestamp '{timestamp}' for the ActivityLog entry cannot be in the future or invalid.") { }
        }

        public class DuplicateLogEntryException : ActivityLogException
        {
            public DuplicateLogEntryException(Guid serviceRequestId, string action, DateTime timestamp)
                : base($"An ActivityLog entry for ServiceRequestId '{serviceRequestId}' with action '{action}' already exists at '{timestamp}'.") { }
        }
    }

    // ==================== ASSIGNMENT-RELATED DOMAIN EXCEPTIONS =====================

    /// <summary>
    /// Exception for Assignment-related domain issues.
    /// </summary>
    public abstract class AssignmentException : DomainException
    {
        protected AssignmentException(string message) : base(message) { }

        public class MissingServiceRequestIdException : AssignmentException
        {
            public MissingServiceRequestIdException()
                : base("The ServiceRequestId must be specified in the Assignment entity.") { }
        }

        public class MissingAgentIdException : AssignmentException
        {
            public MissingAgentIdException()
                : base("The AgentId must be specified in the Assignment entity.") { }
        }

        public class NegativeEstimatedCostException : AssignmentException
        {
            public NegativeEstimatedCostException(decimal estimatedCost)
                : base($"The EstimatedCost '{estimatedCost}' cannot be negative.") { }
        }

        public class InvalidAssignmentDateException : AssignmentException
        {
            public InvalidAssignmentDateException(DateTime assignmentDate)
                : base($"Assignment date '{assignmentDate}' cannot be in the future or before the ServiceRequest creation date.") { }
        }

        public class AssignmentAlreadyExistsException : AssignmentException
        {
            public AssignmentAlreadyExistsException(Guid serviceRequestId, Guid agentId)
                : base($"An Assignment for ServiceRequestId '{serviceRequestId}' and AgentId '{agentId}' already exists.") { }
        }
    }

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

    // ==================== SERVICEAGENT-RELATED DOMAIN EXCEPTIONS ====================

    /// <summary>
    /// Exception for ServiceAgent-related domain issues.
    /// </summary>
    public abstract class ServiceAgentException : DomainException
    {
        protected ServiceAgentException(string message) : base(message) { }

        public class AgentNameRequiredException : ServiceAgentException
        {
            public AgentNameRequiredException()
                : base("The ServiceAgent name cannot be null, empty, or contain only whitespace.") { }
        }

        public class AddingInvalidCapabilityException : ServiceAgentException
        {
            public AddingInvalidCapabilityException(string capability)
                : base($"The capability '{capability}' added to the ServiceAgent is invalid.") { }
        }

        public class AgentAlreadyDeactivatedException : ServiceAgentException
        {
            public AgentAlreadyDeactivatedException()
                : base("Cannot perform this operation because the ServiceAgent is already deactivated.") { }
        }

        public class CapabilityAlreadyExistsException : ServiceAgentException
        {
            public CapabilityAlreadyExistsException(string capability)
                : base($"The capability '{capability}' already exists for the ServiceAgent.") { }
        }
    }

    // =================== SERVICECATEGORY-RELATED DOMAIN EXCEPTIONS ==================

    /// <summary>
    /// Exception for ServiceCategory-related domain issues.
    /// </summary>
    public abstract class ServiceCategoryException : DomainException
    {
        protected ServiceCategoryException(string message) : base(message) { }

        public class NameRequiredException : ServiceCategoryException
        {
            public NameRequiredException()
                : base("The Name property in ServiceCategory cannot be null or empty.") { }
        }

        public class InvalidCategoryDescriptionException : ServiceCategoryException
        {
            public InvalidCategoryDescriptionException(string description)
                : base($"The Description '{description}' in ServiceCategory is invalid. Please ensure it meets length requirements.") { }
        }

        public class DuplicateCategoryNameException : ServiceCategoryException
        {
            public DuplicateCategoryNameException(string categoryName)
                : base($"A category with the name '{categoryName}' already exists.") { }
        }
    }

    // ================== SERVICEREQUEST-RELATED DOMAIN EXCEPTIONS ====================

    /// <summary>
    /// Exception for ServiceRequest-related domain issues.
    /// </summary>
    public abstract class ServiceRequestException : DomainException
    {
        protected ServiceRequestException(string message) : base(message) { }

        public class InvalidDescriptionException : ServiceRequestException
        {
            public InvalidDescriptionException()
                : base("The ServiceRequest description cannot be null, empty, or exceed 1000 characters.") { }
        }

        public class InvalidStateTransitionException : ServiceRequestException
        {
            public InvalidStateTransitionException(string currentState, string attemptedState)
                : base($"The transition from '{currentState}' to '{attemptedState}' is not allowed for this ServiceRequest.") { }
        }

        public class MissingAssignedProviderException : ServiceRequestException
        {
            public MissingAssignedProviderException()
                : base("An AssignedProvider must be specified before marking the ServiceRequest as In Progress.") { }
        }
    }

    // ====================== VALUE OBJECT MONEY DOMAIN EXCEPTIONS ====================

    /// <summary>
    /// Exception for value object Money-related domain issues.
    /// </summary>
    public abstract class MoneyException : DomainException
    {
        protected MoneyException(string message) : base(message) { }

        public class NegativeAmountException : MoneyException
        {
            public NegativeAmountException(decimal amount)
                : base($"The monetary value '{amount}' cannot be negative.") { }
        }

        public class UnsupportedCurrencyException : MoneyException
        {
            public UnsupportedCurrencyException(string currency)
                : base($"The currency '{currency}' is not supported. Supported currencies include: USD, EUR.") { }
        }

        public class InvalidAmountPrecisionException : MoneyException
        {
            public InvalidAmountPrecisionException(decimal amount)
                : base($"The monetary value '{amount}' exceeds the allowed precision for this operation.") { }
        }
    }

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