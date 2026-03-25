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

        // Replace previous completion evidence set so staff sees the latest submission only.
        var existingEvidences = await _context.CompletionEvidences
            .Where(x => x.ServiceRequestId == request.ServiceRequestId)
            .ToListAsync(cancellationToken);
        if (existingEvidences.Count > 0)
        {
            _context.CompletionEvidences.RemoveRange(existingEvidences);
        }

        // Handle uploaded evidence image
        if (!string.IsNullOrWhiteSpace(request.UploadedImageUrl))
        {
            var uploadedEvidence = new CompletionEvidence(
                serviceRequest.Id,
                serviceRequest.AssignedProviderId ?? Guid.Empty,
                request.UploadedImageUrl.Trim(),
                EvidenceType.After,
                string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim());

            _context.CompletionEvidences.Add(uploadedEvidence);
        }

        // Add URL-based evidences (legacy or additional)
        foreach (var ev in request.Evidences)
        {
            if (string.IsNullOrWhiteSpace(ev.ImageUrl))
            {
                continue;
            }

            var evidence = new CompletionEvidence(
                serviceRequest.Id,
                serviceRequest.AssignedProviderId ?? Guid.Empty,
                ev.ImageUrl.Trim(),
                ev.Type,
                ev.Notes);
            
            _context.CompletionEvidences.Add(evidence);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
