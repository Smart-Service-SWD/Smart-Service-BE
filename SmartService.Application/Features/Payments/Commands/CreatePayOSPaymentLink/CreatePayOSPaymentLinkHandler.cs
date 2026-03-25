using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Payments;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.Common.Models.Payments;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Payments.Commands.CreatePayOSPaymentLink;

public class CreatePayOSPaymentLinkHandler : IRequestHandler<CreatePayOSPaymentLinkCommand, PaymentLinkResult>
{
    private readonly IAppDbContext _context;
    private readonly IPayOSService _payOSService;

    public CreatePayOSPaymentLinkHandler(IAppDbContext context, IPayOSService payOSService)
    {
        _context = context;
        _payOSService = payOSService;
    }

    public async Task<PaymentLinkResult> Handle(CreatePayOSPaymentLinkCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        // Create the link
        var result = await _payOSService.CreatePaymentLink(serviceRequest, request.IsDeposit, request.ReturnUrl, request.CancelUrl);

        // Store the orderCode on the request for comparison in Webhook
        serviceRequest.SetPayOSOrderCode(result.OrderCode);

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}
