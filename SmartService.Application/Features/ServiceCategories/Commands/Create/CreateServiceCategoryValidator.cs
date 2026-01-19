using FluentValidation;

namespace SmartService.Application.Features.ServiceCategories.Commands.Create;

/// <summary>
/// Validator for CreateServiceCategoryCommand.
/// </summary>
public class CreateServiceCategoryValidator : AbstractValidator<CreateServiceCategoryCommand>
{
    public CreateServiceCategoryValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");
    }
}

