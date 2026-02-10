namespace SmartService.Domain.Exceptions
{
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
}
