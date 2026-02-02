using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.Complete;

/// <summary>
/// Validator for CompleteServiceRequestCommand.
/// Ensures all required fields are valid before handler execution.
/// </summary>
public class CompleteServiceRequestValidator : AbstractValidator<CompleteServiceRequestCommand>
{
    public CompleteServiceRequestValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");
    }
}
