using MediatR;
using SmartService.Domain.ValueObjects;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.PriceAdjustments.Queries.GetPendingPriceAdjustments;

public record GetPendingPriceAdjustmentsQuery : IRequest<List<PriceAdjustmentDto>>;

public class PriceAdjustmentDto
{
    public Guid Id { get; set; }
    public Guid ServiceRequestId { get; set; }
    public decimal OldPriceAmount { get; set; }
    public string OldPriceCurrency { get; set; } = "VND";
    public decimal NewPriceAmount { get; set; }
    public string NewPriceCurrency { get; set; } = "VND";
    public string Reason { get; set; } = null!;
    public string EvidenceImageUrl { get; set; } = null!;
    public PriceAdjustmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}
