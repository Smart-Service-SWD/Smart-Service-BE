using MediatR;

namespace SmartService.Application.Features.Payouts.Commands.ProcessPayout;

public record ProcessPayoutCommand(
    Guid ServiceRequestId,
    decimal CommissionPercent) : IRequest<Guid>;
