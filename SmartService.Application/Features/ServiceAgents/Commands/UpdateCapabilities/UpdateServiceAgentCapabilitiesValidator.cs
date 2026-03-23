using FluentValidation;

namespace SmartService.Application.Features.ServiceAgents.Commands.UpdateCapabilities;

public class UpdateServiceAgentCapabilitiesValidator : AbstractValidator<UpdateServiceAgentCapabilitiesCommand>
{
    public UpdateServiceAgentCapabilitiesValidator()
    {
        RuleFor(v => v.AgentId)
            .NotEmpty()
            .WithMessage("Agent ID cannot be empty.");

        RuleFor(v => v.Capabilities)
            .NotEmpty()
            .WithMessage("At least one capability is required.");

        RuleFor(v => v.Capabilities)
            .Must(capabilities => capabilities.Select(c => c.CategoryId).Distinct().Count() == capabilities.Count)
            .WithMessage("Each capability must use a distinct category.");

        RuleForEach(v => v.Capabilities).ChildRules(cap =>
        {
            cap.RuleFor(c => c.CategoryId)
                .NotEmpty()
                .WithMessage("Category ID is required for each capability.");

            cap.RuleFor(c => c.MaxComplexityLevel)
                .InclusiveBetween(1, 5)
                .WithMessage("Max complexity level must be between 1 and 5.");

            cap.RuleFor(c => c.ServiceIds)
                .NotEmpty()
                .WithMessage("At least one service definition must be assigned per capability.");
        });
    }
}
