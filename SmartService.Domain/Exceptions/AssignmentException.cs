namespace SmartService.Domain.Exceptions
{
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
}
