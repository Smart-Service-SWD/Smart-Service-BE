using FluentValidation;

namespace SmartService.Application.Features.ServiceFeedbacks.Commands.Create;

/// <summary>
/// Validator for CreateServiceFeedbackCommand.
/// </summary>
public class CreateServiceFeedbackValidator : AbstractValidator<CreateServiceFeedbackCommand>
{
    public CreateServiceFeedbackValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.CreatedByUserId)
            .NotEmpty()
            .WithMessage("User ID cannot be empty.");

        RuleFor(v => v.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(v => v.Comment)
            .MaximumLength(1000)
            .WithMessage("Comment cannot exceed 1000 characters.");
    }
}

