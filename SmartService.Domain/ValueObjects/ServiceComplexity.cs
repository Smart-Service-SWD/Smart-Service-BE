using SmartService.Domain.Exceptions;

/// <summary>
/// Represents the complexity level of a service request.
/// 
/// ServiceComplexity is a Value Object and therefore immutable.
/// 
/// The complexity level is determined by the system or staff
/// based on business evaluation rules, NOT by service agents.
/// 
/// Valid levels range from:
/// 1 - Very simple
/// 5 - Highly complex
/// 
/// This value directly affects:
/// - Agent eligibility
/// - Pricing estimation
/// - Workflow routing
/// </summary>
namespace SmartService.Domain.ValueObjects
{
    public sealed class ServiceComplexity
    {
        public int Level { get; private set; }

        private ServiceComplexity(int level)
        {
            if (level < 1 || level > 5)
                throw new ServiceComplexityException.InvalidComplexityLevelException(level);

            Level = level;
        }

        private ServiceComplexity() { } // EF Core

        public static ServiceComplexity From(int level)
            => new(level);
    }
}