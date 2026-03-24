using MediatR;
using SmartService.Application.Features.PriceAdjustments.Queries.GetPendingPriceAdjustments;

namespace SmartService.Application.Features.PriceAdjustments.Queries.GetPriceAdjustmentByServiceRequest;

public record GetPriceAdjustmentByServiceRequestQuery(Guid ServiceRequestId) : IRequest<PriceAdjustmentDto?>;
