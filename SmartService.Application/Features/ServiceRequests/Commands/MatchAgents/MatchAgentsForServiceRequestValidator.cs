using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.MatchAgents;

/// <summary>
/// Validator for MatchAgentsForServiceRequestCommand.
/// Ensures all required fields are valid before handler execution.
/// </summary>
public class MatchAgentsForServiceRequestValidator : AbstractValidator<MatchAgentsForServiceRequestCommand>
{
    public MatchAgentsForServiceRequestValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");
    }
}
