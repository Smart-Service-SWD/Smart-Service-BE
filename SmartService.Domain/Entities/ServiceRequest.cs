using SmartService.Domain.Interfaces;
using SmartService.Domain.ValueObjects;
using SmartService.Domain.Exceptions;

namespace SmartService.Domain.Entities;

/// <summary>
/// Represents a service request created by a customer.
/// 
/// This is the Aggregate Root of the Service Request domain.
/// It controls the entire lifecycle of a service request, including:
/// - Creation by customer
/// - Complexity evaluation by staff
/// - Assignment to service agent
/// - Execution and completion
///
/// All business rules related to service request state transitions
/// are enforced within this entity.
/// </summary>

public class ServiceRequest : IAggregateRoot
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid CategoryId { get; private set; }

    public string Description { get; private set; }
    public ServiceComplexity Complexity { get; private set; } = null;
    public ServiceStatus Status { get; private set; }
    public Guid? AssignedProviderId { get; private set; }
    public Money? EstimatedCost { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<ServiceAttachment> _attachments = new();
    public IReadOnlyCollection<ServiceAttachment> Attachments => _attachments.AsReadOnly();

    private readonly List<MatchingResult> _matchingResults = new();
    public IReadOnlyCollection<MatchingResult> MatchingResults => _matchingResults.AsReadOnly();

    // EF Core
    private ServiceRequest() { }

    private ServiceRequest(Guid id, Guid customerId, Guid categoryId, string description)
    {
        Id = id;
        CustomerId = customerId;
        CategoryId = categoryId;
        Description = description;
        Status = ServiceStatus.Created;
        CreatedAt = DateTime.UtcNow;
    }

    // Factory Method – CHỈ DÙNG KHI CREATE
    public static ServiceRequest Create(
        Guid customerId,
        Guid categoryId,
        string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        return new ServiceRequest(
            Guid.NewGuid(),
            customerId,
            categoryId,
            description);
    }

    // Domain Behaviors

    public void Evaluate(ServiceComplexity complexity)
    {
        if (Status != ServiceStatus.Created)
            throw new DomainException("Service request must be in Created state.");

        Complexity = complexity;
        Status = ServiceStatus.PendingReview;
    }

    public void AssignProvider(Guid providerId, Money estimatedCost)
    {
        if (Status != ServiceStatus.PendingReview)
            throw new DomainException("Service request must be evaluated first.");

        AssignedProviderId = providerId;
        EstimatedCost = estimatedCost;
        Status = ServiceStatus.Assigned;
    }

    public void Start()
    {
        if (Status != ServiceStatus.Assigned)
            throw new DomainException("Service request must be assigned.");

        Status = ServiceStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != ServiceStatus.InProgress)
            throw new DomainException("Service request must be in progress.");

        Status = ServiceStatus.Completed;
    }
}
