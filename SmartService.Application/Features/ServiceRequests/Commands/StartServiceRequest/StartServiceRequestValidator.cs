using FluentValidation;

namespace SmartService.Application.Features.ServiceRequests.Commands.StartServiceRequest;

public class StartServiceRequestValidator : AbstractValidator<StartServiceRequestCommand>
{
    public StartServiceRequestValidator()
    {
        RuleFor(v => v.ServiceRequestId)
            .NotEmpty()
            .WithMessage("Service Request ID cannot be empty.");
    }
}
