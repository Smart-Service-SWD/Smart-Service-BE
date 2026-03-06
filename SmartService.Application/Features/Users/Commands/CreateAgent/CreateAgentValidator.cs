using FluentValidation;

namespace SmartService.Application.Features.Users.Commands.CreateAgent;

/// <summary>
/// Validator for CreateAgentCommand.
/// </summary>
public class CreateAgentValidator : AbstractValidator<CreateAgentCommand>
{
    public CreateAgentValidator()
    {
        RuleFor(v => v.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .MaximumLength(200)
            .WithMessage("Full name cannot exceed 200 characters.");

        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters.");

        RuleFor(v => v.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?[0-9\s\-\(\)]{10,}$")
            .WithMessage("Phone number must be valid (at least 10 digits).");

        RuleFor(v => v.Capabilities)
            .NotEmpty()
            .WithMessage("At least one agent capability is required.");

        RuleForEach(v => v.Capabilities).ChildRules(cap =>
        {
            cap.RuleFor(c => c.CategoryId)
                .NotEmpty()
                .WithMessage("Category ID is required for each capability.");

            cap.RuleFor(c => c.MaxComplexityLevel)
                .InclusiveBetween(1, 5)
                .WithMessage("Max complexity level must be between 1 and 5.");

            cap.RuleFor(c => c.ServiceIds)
                .NotEmpty()
                .WithMessage("At least one service definition must be assigned per capability.");
        });
    }
}

