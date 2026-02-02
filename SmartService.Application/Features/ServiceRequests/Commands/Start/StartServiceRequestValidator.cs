using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.Start;

/// <summary>
/// Validator for StartServiceRequestCommand.
/// Ensures all required fields are valid before handler execution.
/// </summary>
public class StartServiceRequestValidator : AbstractValidator<StartServiceRequestCommand>
{
    public StartServiceRequestValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");
    }
}
