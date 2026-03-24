using MediatR;

namespace SmartService.Application.Features.PriceAdjustments.Commands.RejectPriceAdjustment;

public record RejectPriceAdjustmentCommand(Guid AdjustmentRequestId, Guid ProcessedBy) : IRequest;
