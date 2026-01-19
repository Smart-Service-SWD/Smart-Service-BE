using FluentValidation;

namespace SmartService.Application.Features.Assignments.Commands.Create;

/// <summary>
/// Validator for CreateAssignmentCommand.
/// </summary>
public class CreateAssignmentValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.AgentId)
            .NotEmpty()
            .WithMessage("Agent ID cannot be empty.");

        RuleFor(v => v.EstimatedCost)
            .NotNull()
            .WithMessage("Estimated cost is required.");
    }
}

