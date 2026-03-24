using MediatR;
using SmartService.Domain.ValueObjects;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.PriceAdjustments.Queries.GetPendingPriceAdjustments;

public record GetPendingPriceAdjustmentsQuery : IRequest<List<PriceAdjustmentDto>>;

public class PriceAdjustmentDto
{
    public Guid Id { get; set; }
    public Guid ServiceRequestId { get; set; }
    public Money OldPrice { get; set; } = null!;
    public Money NewPrice { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public PriceAdjustmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}
