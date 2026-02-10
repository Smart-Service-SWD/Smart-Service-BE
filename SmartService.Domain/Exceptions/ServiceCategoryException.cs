namespace SmartService.Domain.Exceptions
{
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
}
