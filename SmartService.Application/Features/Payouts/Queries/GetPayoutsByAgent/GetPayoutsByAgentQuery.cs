using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.Payouts.Queries.GetPayoutsByAgent;

public record PayoutDto(
    Guid Id,
    Guid ServiceRequestId,
    Guid AgentId,
    Money Amount,
    decimal CommissionRate,
    Money NetAmount,
    DateTime PayoutDate);

public record GetPayoutsByAgentQuery(Guid AgentId) : IRequest<List<PayoutDto>>;
