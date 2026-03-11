using FluentValidation;

namespace SmartService.Application.Features.ServiceAgents.Commands.SetActiveStatus;

public class SetServiceAgentActiveStatusValidator : AbstractValidator<SetServiceAgentActiveStatusCommand>
{
    public SetServiceAgentActiveStatusValidator()
    {
        RuleFor(v => v.AgentId)
            .NotEmpty()
            .WithMessage("Agent ID cannot be empty.");

        RuleFor(v => v.ActorUserId)
            .NotEmpty()
            .WithMessage("Actor user ID cannot be empty.");
    }
}
