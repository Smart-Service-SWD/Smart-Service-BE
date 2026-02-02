using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.Update;

/// <summary>
/// Validator for UpdateServiceRequestCommand.
/// Ensures all required fields are valid before handler execution.
/// </summary>
public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequestCommand>
{
    public UpdateServiceRequestValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");
    }
}
