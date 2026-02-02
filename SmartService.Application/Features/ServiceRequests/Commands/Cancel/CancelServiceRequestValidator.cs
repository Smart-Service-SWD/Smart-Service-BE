using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.Cancel;

/// <summary>
/// Validator for CancelServiceRequestCommand.
/// Ensures all required fields are valid before handler execution.
/// </summary>
public class CancelServiceRequestValidator : AbstractValidator<CancelServiceRequestCommand>
{
    public CancelServiceRequestValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.CancellationReason)
            .NotEmpty()
            .WithMessage("Cancellation reason is required.")
            .MaximumLength(500)
            .WithMessage("Cancellation reason cannot exceed 500 characters.");
    }
}
