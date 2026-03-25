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
    public long? PayOSOrderCode { get; private set; }

        /// <summary>
        /// Optional reference to the primary ServiceDefinition this request is associated with.
        /// Helps analytics & reporting without needing to re-run AI matching.
        /// </summary>
        public Guid? ServiceDefinitionId { get; private set; }

    public string? Description { get; private set; }
    public ServiceComplexity Complexity { get; private set; } = null;
    public ServiceStatus Status { get; private set; }
    public Guid? AssignedProviderId { get; private set; }
    public Money? EstimatedCost { get; private set; }
    public Money? FinalPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public string? AddressText { get; private set; }
    
    public Money? DepositAmount { get; private set; }
    public bool IsDepositPaid { get; private set; } = false;
    public Money? FinalAmountPaid { get; private set; }
    public decimal CommissionRate { get; private set; } // e.g. 0.2
    public Money? CommissionAmount { get; private set; }
    public Money? WorkerAmount { get; private set; }

    // ── AI Analysis results ──────────────────────────────────────────
    /// <summary>AI-estimated price range. Example: "2.000.000 – 5.000.000 VNĐ"</summary>
    public string? EstimatedPrice { get; private set; }

    /// <summary>AI-estimated duration range. Example: "4 – 8 giờ"</summary>
    public string? EstimatedDuration { get; private set; }

    /// <summary>Text extracted from the uploaded image via OCR.</summary>
    public string? OcrExtractedText { get; private set; }

    /// <summary>True when this request was processed by the AI analysis pipeline.</summary>
    public bool WasAnalyzedByAI { get; private set; }

    private readonly List<ServiceAttachment> _attachments = new();
    public IReadOnlyCollection<ServiceAttachment> Attachments => _attachments.AsReadOnly();

    private readonly List<CompletionEvidence> _completionEvidences = new();
    public IReadOnlyCollection<CompletionEvidence> CompletionEvidences => _completionEvidences.AsReadOnly();

    private readonly List<MatchingResult> _matchingResults = new();
    public IReadOnlyCollection<MatchingResult> MatchingResults => _matchingResults.AsReadOnly();

    // EF Core
    private ServiceRequest() { }

    private ServiceRequest(
        Guid id,
        Guid customerId,
        Guid categoryId,
        string description,
        string? addressText = null,
        ServiceComplexity? complexity = null,
        Guid? serviceDefinitionId = null)
    {
        Id = id;
        CustomerId = customerId;
        CategoryId = categoryId;
        Description = description;
        AddressText = addressText;
        Complexity = complexity;
        ServiceDefinitionId = serviceDefinitionId;
        Status = ServiceStatus.AwaitingAnalysis;
        CreatedAt = DateTime.UtcNow;
    }

    // Factory Method – CHỈ DÙNG KHI CREATE
    public static ServiceRequest Create(
        Guid customerId,
        Guid categoryId,
        string description,
        string? addressText = null,
        ServiceComplexity? complexity = null,
        Guid? serviceDefinitionId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ServiceRequestException.InvalidDescriptionException();

        return new ServiceRequest(
            Guid.NewGuid(),
            customerId,
            categoryId,
            description,
            addressText,
            complexity,
            serviceDefinitionId);
    }
    
    public void MarkAsAnalyzed(int urgencyLevel)
    {
        if (Status != ServiceStatus.AwaitingAnalysis)
            throw new ServiceRequestException.InvalidStatusForOperationException("MarkAsAnalyzed", "AwaitingAnalysis");
        
        Status = ServiceStatus.Created;
    }

    /// <summary>
    /// Stores AI analysis estimates on the request.
    /// Called during the create flow after AI responds.
    /// </summary>
    public void SetAiEstimates(string? estimatedPrice, string? estimatedDuration, string? ocrExtractedText)
    {
        EstimatedPrice = estimatedPrice;
        EstimatedDuration = estimatedDuration;
        OcrExtractedText = ocrExtractedText;
        WasAnalyzedByAI = true;
    }

    // Domain Behaviors

    public void Evaluate(ServiceComplexity complexity, Guid? serviceDefinitionId = null, Money? estimatedCost = null)
    {
        // Cho phép staff đánh giá lần đầu (từ Created) hoặc đánh giá lại (khi đã ở PendingReview)
        if (Status != ServiceStatus.Created && Status != ServiceStatus.PendingReview && Status != ServiceStatus.UrgentDispatch)
            throw new ServiceRequestException.InvalidStatusForOperationException("Evaluate", "Created, UrgentDispatch or PendingReview");

        Complexity = complexity;
        if (serviceDefinitionId.HasValue) ServiceDefinitionId = serviceDefinitionId.Value;
        if (estimatedCost != null) EstimatedCost = estimatedCost;

        // Nếu đang ở trạng thái Created thì chuyển sang PendingReview.
        // Nếu đã PendingReview rồi thì giữ nguyên trạng thái.
        if (Status == ServiceStatus.Created || Status == ServiceStatus.UrgentDispatch)
        {
            Status = ServiceStatus.PendingReview;
        }
    }

    public void AssignProvider(Guid providerId, Money estimatedCost)
    {
        if (Status != ServiceStatus.PendingReview && Status != ServiceStatus.UrgentDispatch)
            throw new ServiceRequestException.InvalidStateTransitionException(Status.ToString(), "PendingReview");

        AssignedProviderId = providerId;
        EstimatedCost = estimatedCost;
        
        // Luồng mới: Gán thợ xong vẫn ở PendingReview để Staff bấm "Yêu cầu đặt cọc"
        // Status = ServiceStatus.PendingReview; (giữ nguyên)
    }

    public void Start()
    {
        if (Status != ServiceStatus.Assigned)
            throw new ServiceRequestException.InvalidStateTransitionException(Status.ToString(), "Assigned");

        Status = ServiceStatus.InProgress;
    }

    public void RequestDeposit(Money depositAmount, decimal commissionRate)
    {
        if (Status != ServiceStatus.PendingReview)
            throw new ServiceRequestException.InvalidStatusForOperationException("RequestDeposit", "PendingReview");

        if (AssignedProviderId == null || EstimatedCost == null)
            throw new ServiceRequestException.MissingAssignedProviderForDepositException();

        DepositAmount = depositAmount;
        CommissionRate = commissionRate;
        Status = ServiceStatus.AwaitingDeposit;
    }

    public void ConfirmDeposit()
    {
        if (Status != ServiceStatus.AwaitingDeposit)
            throw new ServiceRequestException.InvalidStatusForOperationException("ConfirmDeposit", "AwaitingDeposit");

        IsDepositPaid = true;
        // Sau khi đặt cọc, nếu đã có thợ (như luồng hiện tại) thì chuyển sang Assigned
        Status = ServiceStatus.Assigned;
    }


    public void RequestCompletion()
    {
        if (Status != ServiceStatus.InProgress)
            throw new ServiceRequestException.InvalidStateTransitionException(Status.ToString(), "InProgress");

        Status = ServiceStatus.AwaitingCompletionReview;
    }

    public void ApproveCompletion()
    {
        if (Status != ServiceStatus.AwaitingCompletionReview)
            throw new ServiceRequestException.InvalidStatusForOperationException("ApproveCompletion", "AwaitingCompletionReview");

        Status = ServiceStatus.CompletionApproved;
    }

    public void RejectCompletion()
    {
        if (Status != ServiceStatus.AwaitingCompletionReview)
            throw new ServiceRequestException.InvalidStatusForOperationException("RejectCompletion", "AwaitingCompletionReview");

        Status = ServiceStatus.InProgress;
    }

    public void MarkAsAwaitingFinalPayment()
    {
        if (Status != ServiceStatus.CompletionApproved)
            throw new ServiceRequestException.InvalidStatusForOperationException("MarkAsAwaitingFinalPayment", "CompletionApproved");

        Status = ServiceStatus.AwaitingFinalPayment;
        if (FinalPrice == null) FinalPrice = EstimatedCost;
    }

    public void MarkAsFinalPaymentPaid(Money amountPaid)
    {
        if (Status != ServiceStatus.AwaitingFinalPayment)
            throw new ServiceRequestException.InvalidStatusForOperationException("MarkAsFinalPaymentPaid", "AwaitingFinalPayment");

        FinalAmountPaid = amountPaid;
        Status = ServiceStatus.FinalPaymentPaid;
        
        // Calculate Payout details
        var total = FinalPrice?.Amount ?? 0;
        var comm = total * CommissionRate;
        var worker = total - comm;
        
        CommissionAmount = Money.Create(comm, FinalPrice?.Currency ?? "VND");
        WorkerAmount = Money.Create(worker, FinalPrice?.Currency ?? "VND");
    }

    public void MarkAsPayoutCompleted()
    {
        if (Status != ServiceStatus.FinalPaymentPaid)
            throw new ServiceRequestException.InvalidStatusForOperationException("MarkAsPayoutCompleted", "FinalPaymentPaid");
            
        Status = ServiceStatus.PayoutCompleted;
    }

    public void ApplyPriceAdjustment(Money newPrice)
    {
        // Price can be adjusted during work
        if (Status != ServiceStatus.Assigned && Status != ServiceStatus.InProgress)
             throw new ServiceRequestException.InvalidStatusForOperationException("ApplyPriceAdjustment", "Assigned or InProgress");

        FinalPrice = newPrice;
    }

    public bool CanCustomerCancelBeforeStaffConfirmation()
        => Status == ServiceStatus.AwaitingAnalysis
            || Status == ServiceStatus.Created
            || Status == ServiceStatus.UrgentDispatch;

    public void CancelByCustomer()
    {
        if (!CanCustomerCancelBeforeStaffConfirmation())
        {
            throw new ServiceRequestException.CustomerCancellationNotAllowedException();
        }

        Status = ServiceStatus.Cancelled;
    }

    public void SetPayOSOrderCode(long orderCode)
    {
        PayOSOrderCode = orderCode;
    }
}

