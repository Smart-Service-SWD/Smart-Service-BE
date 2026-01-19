using FluentValidation;

namespace SmartService.Application.Features.ServiceAgents.Commands.Create;

/// <summary>
/// Validator for CreateServiceAgentCommand.
/// </summary>
public class CreateServiceAgentValidator : AbstractValidator<CreateServiceAgentCommand>
{
    public CreateServiceAgentValidator()
    {
        RuleFor(v => v.FullName)
            .NotEmpty()
            .WithMessage("Agent name is required.")
            .MaximumLength(200)
            .WithMessage("Agent name cannot exceed 200 characters.");
    }
}

