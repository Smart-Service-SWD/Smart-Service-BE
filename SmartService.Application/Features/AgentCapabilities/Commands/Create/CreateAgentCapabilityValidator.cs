using FluentValidation;

namespace SmartService.Application.Features.AgentCapabilities.Commands.Create;

/// <summary>
/// Validator for CreateAgentCapabilityCommand.
/// </summary>
public class CreateAgentCapabilityValidator : AbstractValidator<CreateAgentCapabilityCommand>
{
    public CreateAgentCapabilityValidator()
    {
        RuleFor(v => v.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID cannot be empty.");

        RuleFor(v => v.MaxComplexity)
            .NotNull()
            .WithMessage("Maximum complexity is required.");
    }
}

