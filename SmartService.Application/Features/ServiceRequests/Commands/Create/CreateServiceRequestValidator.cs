using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.Create;

/// <summary>
/// Validator for CreateServiceRequestCommand.
/// ComplexityLevel is optional since AI will derive it automatically from description + OCR.
/// </summary>
public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequestCommand>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(v => v.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID cannot be empty.");

        RuleFor(v => v.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID cannot be empty.");

        RuleFor(v => v.ServiceDefinitionId)
            .NotEmpty()
            .WithMessage("Service Definition ID cannot be empty.");

        RuleFor(v => v.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");
        // ComplexityLevel removed – AI sets it from description + OCR analysis
    }
}
