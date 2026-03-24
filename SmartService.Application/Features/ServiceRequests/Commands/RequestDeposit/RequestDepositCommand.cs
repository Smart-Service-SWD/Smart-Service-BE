using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceRequests.Commands.RequestDeposit;

public record RequestDepositCommand(
    Guid ServiceRequestId,
    Money DepositAmount,
    decimal CommissionRate) : IRequest<Unit>;
