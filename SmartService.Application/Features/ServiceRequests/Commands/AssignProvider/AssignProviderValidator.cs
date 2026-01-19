using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.AssignProvider;

/// <summary>
/// Validator for AssignProviderCommand.
/// Ensures all required fields are valid before handler execution.
/// </summary>
public class AssignProviderValidator : AbstractValidator<AssignProviderCommand>
{
    public AssignProviderValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.ProviderId)
            .NotEmpty()
            .WithMessage("Provider ID cannot be empty.");

        RuleFor(v => v.EstimatedCost)
            .NotNull()
            .WithMessage("Estimated cost is required.");
    }
}

