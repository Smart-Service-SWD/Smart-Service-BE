using FluentValidation;

namespace SmartService.Application.Features.ServiceAgents.Commands.Deactivate;

/// <summary>
/// Validator for DeactivateServiceAgentCommand.
/// </summary>
public class DeactivateServiceAgentValidator : AbstractValidator<DeactivateServiceAgentCommand>
{
    public DeactivateServiceAgentValidator()
    {
        RuleFor(v => v.AgentId)
            .NotEmpty()
            .WithMessage("Agent ID cannot be empty.");
    }
}

