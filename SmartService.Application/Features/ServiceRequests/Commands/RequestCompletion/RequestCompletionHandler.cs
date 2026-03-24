using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SmartService.Application.Features.ServiceRequests.Commands.RequestCompletion;

public class RequestCompletionHandler : IRequestHandler<RequestCompletionCommand, Unit>
{
    private readonly IAppDbContext _context;

    public RequestCompletionHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(RequestCompletionCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        serviceRequest.RequestCompletion();

        // Handle File Uploaded Evidence (if any)
        if (request.ImageStream != null && !string.IsNullOrWhiteSpace(request.ImageFileName))
        {
            var uploadedEvidence = new CompletionEvidence(
                serviceRequest.Id,
                serviceRequest.AssignedProviderId ?? Guid.Empty,
                $"local://{request.ImageFileName}", // Mock URL
                EvidenceType.After,
                "Ảnh bằng chứng tải lên từ thợ.");

            _context.CompletionEvidences.Add(uploadedEvidence);
        }

        // Add URL-based evidences (legacy or additional)
        foreach (var ev in request.Evidences)
        {
            var evidence = new CompletionEvidence(
                serviceRequest.Id,
                serviceRequest.AssignedProviderId ?? Guid.Empty,
                ev.ImageUrl ?? "local://placeholder.png",
                ev.Type,
                ev.Notes);
            
            _context.CompletionEvidences.Add(evidence);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
