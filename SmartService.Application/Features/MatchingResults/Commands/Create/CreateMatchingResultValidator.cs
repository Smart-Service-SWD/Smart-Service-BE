using FluentValidation;

namespace SmartService.Application.Features.MatchingResults.Commands.Create;

/// <summary>
/// Validator for CreateMatchingResultCommand.
/// </summary>
public class CreateMatchingResultValidator : AbstractValidator<CreateMatchingResultCommand>
{
    public CreateMatchingResultValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.ServiceAgentId)
            .NotEmpty()
            .WithMessage("Service Agent ID cannot be empty.");

        RuleFor(v => v.SupportedComplexity)
            .NotNull()
            .WithMessage("Supported complexity is required.");

        RuleFor(v => v.MatchingScore)
            .InclusiveBetween(0, 100)
            .WithMessage("Matching score must be between 0 and 100.");
    }
}

