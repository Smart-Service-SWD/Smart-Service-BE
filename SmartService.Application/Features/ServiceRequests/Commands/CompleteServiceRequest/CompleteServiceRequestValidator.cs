using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.CompleteServiceRequest;

public class CompleteServiceRequestValidator : AbstractValidator<CompleteServiceRequestCommand>
{
    public CompleteServiceRequestValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");
    }
}
