using FluentValidation;

namespace SmartService.Application.Features.ServiceAttachments.Commands.Create;

/// <summary>
/// Validator for CreateServiceAttachmentCommand.
/// </summary>
public class CreateServiceAttachmentValidator : AbstractValidator<CreateServiceAttachmentCommand>
{
    public CreateServiceAttachmentValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");

        RuleFor(v => v.FileName)
            .NotEmpty()
            .WithMessage("File name is required.")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters.");

        RuleFor(v => v.FileUrl)
            .NotEmpty()
            .WithMessage("File URL is required.")
            .MaximumLength(2000)
            .WithMessage("File URL cannot exceed 2000 characters.");

        RuleFor(v => v.Type)
            .IsInEnum()
            .WithMessage("Invalid attachment type.");
    }
}

