using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceAttachments.Commands.Create;

/// <summary>
/// Handler for CreateServiceAttachmentCommand.
/// Creates and attaches a file to a service request.
/// </summary>
public class CreateServiceAttachmentHandler : IRequestHandler<CreateServiceAttachmentCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateServiceAttachmentHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateServiceAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = ServiceAttachment.Create(
            request.ServiceRequestId,
            request.FileName,
            request.FileUrl,
            request.Type);

        _context.ServiceAttachments.Add(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        return attachment.Id;
    }
}

