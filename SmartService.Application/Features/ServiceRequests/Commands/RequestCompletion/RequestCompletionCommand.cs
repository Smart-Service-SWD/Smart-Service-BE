using MediatR;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceRequests.Commands.RequestCompletion;

public record RequestCompletionCommand(
    Guid ServiceRequestId,
    List<CompletionEvidenceDto> Evidences,
    string? UploadedImageUrl = null,
    string? Notes = null) : IRequest<Unit>;

public record CompletionEvidenceDto(string? ImageUrl, EvidenceType Type, string? Notes);
