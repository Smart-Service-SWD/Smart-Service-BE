using MediatR;

namespace SmartService.Application.Features.ServiceFeedbacks.Commands.Create;

/// <summary>
/// Command to create new service feedback.
/// </summary>
public record CreateServiceFeedbackCommand(
    Guid ServiceRequestId,
    Guid CreatedByUserId,
    int Rating,
    string? Comment) : IRequest<Guid>;

