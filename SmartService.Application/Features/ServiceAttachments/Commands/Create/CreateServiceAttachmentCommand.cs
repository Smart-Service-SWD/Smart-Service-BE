using MediatR;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceAttachments.Commands.Create;

/// <summary>
/// Command to create a new service attachment.
/// </summary>
public record CreateServiceAttachmentCommand(
    Guid ServiceRequestId,
    string FileName,
    string FileUrl,
    AttachmentType Type) : IRequest<Guid>;

