using MediatR;

namespace SmartService.Application.Features.PriceAdjustments.Commands.CreatePriceAdjustmentRequest;

public class CreatePriceAdjustmentRequestCommand : IRequest<Guid>
{
    public Guid ServiceRequestId { get; set; }
    public decimal NewPriceAmount { get; set; }
    public string NewPriceCurrency { get; set; } = "VND";
    public string Reason { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string EvidenceImageUrl { get; set; } = string.Empty;
}
