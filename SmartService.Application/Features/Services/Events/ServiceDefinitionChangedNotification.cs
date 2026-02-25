using MediatR;

namespace SmartService.Application.Features.Services.Events;

/// <summary>
/// Notification fired when a ServiceDefinition is created, updated, or deleted.
/// Triggers KnowledgeBase JSON sync to keep AI data up-to-date.
/// </summary>
public record ServiceDefinitionChangedNotification : INotification;
