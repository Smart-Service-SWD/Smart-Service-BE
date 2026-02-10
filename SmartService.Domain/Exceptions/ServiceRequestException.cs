namespace SmartService.Domain.Exceptions
{
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

        public class InvalidStatusForOperationException : ServiceRequestException
        {
            public InvalidStatusForOperationException(string operation, string requiredStatus)
                : base($"Operation '{operation}' requires status to be '{requiredStatus}'.") { }
        }
    }
}
