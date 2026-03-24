using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.PriceAdjustments.Commands.CreatePriceAdjustmentRequest;

public record CreatePriceAdjustmentRequestCommand(
    Guid ServiceRequestId,
    Money NewPrice,
    string Reason,
    Guid CreatedBy) : IRequest<Guid>;
