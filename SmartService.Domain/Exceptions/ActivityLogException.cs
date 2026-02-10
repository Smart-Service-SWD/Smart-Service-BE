namespace SmartService.Domain.Exceptions
{
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
}
