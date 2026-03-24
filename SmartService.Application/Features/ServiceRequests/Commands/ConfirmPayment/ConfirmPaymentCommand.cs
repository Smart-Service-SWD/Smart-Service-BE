using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.ConfirmPayment;

public record ConfirmPaymentCommand(Guid ServiceRequestId) : IRequest;
