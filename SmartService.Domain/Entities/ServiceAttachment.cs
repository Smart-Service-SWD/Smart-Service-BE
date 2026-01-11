namespace SmartService.Domain.Entities;

/// <summary>
/// Represents an attachment associated with a service request.
/// 
/// Attachments provide additional context such as images,
/// documents, or videos to support evaluation, execution,
/// and dispute resolution.
/// 
/// Only metadata is stored in the domain.
/// </summary>
public class ServiceAttachment
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    public string FileName { get; private set; }
    public string FileUrl { get; private set; }
    public AttachmentType Type { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private ServiceAttachment() { }

    private ServiceAttachment(Guid serviceRequestId, string fileName, string fileUrl, AttachmentType type)
    {
        Id = Guid.NewGuid();
        ServiceRequestId = serviceRequestId;
        FileName = fileName;
        FileUrl = fileUrl;
        Type = type;
        UploadedAt = DateTime.UtcNow;
    }

    public static ServiceAttachment Create(
        Guid serviceRequestId,
        string fileName,
        string fileUrl,
        AttachmentType type)
        => new(serviceRequestId, fileName, fileUrl, type);
}

public enum AttachmentType
{
    Image,
    Video,
    Document,
    Other
}
