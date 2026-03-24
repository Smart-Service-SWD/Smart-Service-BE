using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.PayoutServiceRequest;

public record PayoutServiceRequestCommand(Guid ServiceRequestId) : IRequest;
