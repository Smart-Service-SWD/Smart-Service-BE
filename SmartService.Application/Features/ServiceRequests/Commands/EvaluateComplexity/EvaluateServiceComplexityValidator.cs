using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.EvaluateComplexity;

/// <summary>
/// Validator for EvaluateServiceComplexityCommand.
/// Ensures service request ID is valid.
/// </summary>
public class EvaluateServiceComplexityValidator : AbstractValidator<EvaluateServiceComplexityCommand>
{
    public EvaluateServiceComplexityValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.Complexity)
            .NotNull()
            .WithMessage("Complexity must be evaluated.");
    }
}

