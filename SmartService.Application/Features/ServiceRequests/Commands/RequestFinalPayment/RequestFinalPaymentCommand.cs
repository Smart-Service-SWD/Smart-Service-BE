using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.RequestFinalPayment;

public record RequestFinalPaymentCommand(Guid ServiceRequestId) : IRequest;
