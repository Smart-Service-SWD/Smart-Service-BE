using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.Create;

/// <summary>
/// Validator for CreateServiceRequestCommand.
/// Ensures all required fields are valid before handler execution.
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

        RuleFor(v => v.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(v => v.ComplexityLevel)
            .InclusiveBetween(1, 5)
            .When(v => v.ComplexityLevel.HasValue)
            .WithMessage("Complexity level must be between 1 and 5 if provided.");
    }
}

