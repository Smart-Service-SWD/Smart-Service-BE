using MediatR;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceRequests.Commands.RequestCompletion;

public record RequestCompletionCommand(
    Guid ServiceRequestId,
    List<CompletionEvidenceDto> Evidences,
    System.IO.Stream? ImageStream = null,
    string? ImageFileName = null) : IRequest<Unit>;

public record CompletionEvidenceDto(string? ImageUrl, EvidenceType Type, string? Notes);
