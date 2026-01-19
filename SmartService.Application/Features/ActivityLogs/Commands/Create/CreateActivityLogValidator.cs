using FluentValidation;

namespace SmartService.Application.Features.ActivityLogs.Commands.Create;

/// <summary>
/// Validator for CreateActivityLogCommand.
/// </summary>
public class CreateActivityLogValidator : AbstractValidator<CreateActivityLogCommand>
{
    public CreateActivityLogValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.Action)
            .NotEmpty()
            .WithMessage("Action is required.")
            .MaximumLength(500)
            .WithMessage("Action cannot exceed 500 characters.");
    }
}

