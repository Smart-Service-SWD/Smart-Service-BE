using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.Payments;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.Common.Models.Payments;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Payments.Commands.CreatePayOSPaymentLink;

public class CreatePayOSPaymentLinkHandler : IRequestHandler<CreatePayOSPaymentLinkCommand, PaymentLinkResult>
{
    private readonly IAppDbContext _context;
    private readonly IPayOSService _payOSService;
    private readonly ILogger<CreatePayOSPaymentLinkHandler> _logger;

    public CreatePayOSPaymentLinkHandler(
        IAppDbContext context,
        IPayOSService payOSService,
        ILogger<CreatePayOSPaymentLinkHandler> logger)
    {
        _context = context;
        _payOSService = payOSService;
        _logger = logger;
    }

    public async Task<PaymentLinkResult> Handle(CreatePayOSPaymentLinkCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
        {
            _logger.LogWarning(
                "[PayOS] Cannot create payment link because ServiceRequest {ServiceRequestId} was not found. IsDeposit={IsDeposit}",
                request.ServiceRequestId,
                request.IsDeposit);
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");
        }

        _logger.LogInformation(
            "[PayOS] Creating payment link for ServiceRequest {ServiceRequestId}. IsDeposit={IsDeposit}, CurrentStatus={CurrentStatus}, ExistingOrderCode={ExistingOrderCode}",
            serviceRequest.Id,
            request.IsDeposit,
            serviceRequest.Status,
            serviceRequest.PayOSOrderCode);

        // Create the link
        var result = await _payOSService.CreatePaymentLink(serviceRequest, request.IsDeposit, request.ReturnUrl, request.CancelUrl);

        // Store the orderCode on the request for comparison in Webhook
        serviceRequest.SetPayOSOrderCode(result.OrderCode);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[PayOS] Created payment link for ServiceRequest {ServiceRequestId}. IsDeposit={IsDeposit}, NewOrderCode={OrderCode}, LinkStatus={LinkStatus}",
            serviceRequest.Id,
            request.IsDeposit,
            result.OrderCode,
            result.Status);

        return result;
    }
}
