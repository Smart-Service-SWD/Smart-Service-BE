using FluentValidation;

namespace SmartService.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Validator for LogoutCommand.
/// </summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
