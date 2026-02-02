using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.Approve;

/// <summary>
/// Validator for ApproveServiceRequestCommand.
/// Ensures all required fields are valid before handler execution.
/// </summary>
public class ApproveServiceRequestValidator : AbstractValidator<ApproveServiceRequestCommand>
{
    public ApproveServiceRequestValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");
    }
}
