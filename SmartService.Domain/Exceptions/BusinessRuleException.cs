namespace SmartService.Domain.Exceptions
{
    /// <summary>
    /// Represents validation or business rule violations.
    /// This abstract base class defines common structures, while specialized exceptions refine specific use cases.
    /// </summary>
    public abstract class BusinessRuleException : Exception
    {
        protected BusinessRuleException(string message) : base(message) { }
        protected BusinessRuleException(string message, Exception innerException) : base(message, innerException) { }

        // ============================== COMMON VALIDATION ERRORS ==============================

        /// <summary>
        /// Thrown when a required field is missing or empty.
        /// </summary>
        public class RequiredFieldException : BusinessRuleException
        {
            public RequiredFieldException(string fieldName)
                : base($"The field '{fieldName}' is required and cannot be empty.") { }
        }

        /// <summary>
        /// Thrown when a field value exceeds the maximum allowed length.
        /// </summary>
        public class FieldLengthExceededException : BusinessRuleException
        {
            public FieldLengthExceededException(string fieldName, int maxLength)
                : base($"The field '{fieldName}' exceeds the maximum allowed length of {maxLength} characters.") { }
        }

        /// <summary>
        /// Thrown when a numeric value falls outside the acceptable range.
        /// </summary>
        public class NumericValueOutOfRangeException : BusinessRuleException
        {
            public NumericValueOutOfRangeException(string fieldName, decimal minValue, decimal maxValue)
                : base($"The value for '{fieldName}' must be between {minValue} and {maxValue}.") { }
        }

        /// <summary>
        /// Thrown when a field does not match a required format or pattern.
        /// </summary>
        public class InvalidFormatException : BusinessRuleException
        {
            public InvalidFormatException(string fieldName, string expectedFormat)
                : base($"The field '{fieldName}' does not match the expected format: {expectedFormat}.") { }
        }

        /// <summary>
        /// Thrown when a rule or constraint specific to the business logic is violated.
        /// </summary>
        public class BusinessConstraintViolationException : BusinessRuleException
        {
            public BusinessConstraintViolationException(string fieldName, string message)
                : base($"Constraint violation in '{fieldName}': {message}") { }
        }
    }
}