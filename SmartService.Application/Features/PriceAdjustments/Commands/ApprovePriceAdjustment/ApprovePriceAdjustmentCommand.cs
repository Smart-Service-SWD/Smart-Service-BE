using MediatR;

namespace SmartService.Application.Features.PriceAdjustments.Commands.ApprovePriceAdjustment;

public record ApprovePriceAdjustmentCommand(
    Guid AdjustmentRequestId,
    Guid ProcessedBy) : IRequest<Unit>;
