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
    }
}

