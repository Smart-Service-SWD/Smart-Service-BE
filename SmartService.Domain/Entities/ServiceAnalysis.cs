namespace SmartService.Domain.Entities;

/// <summary>
/// Represents AI analysis results for a service request.
/// Separated from ServiceRequest to keep domain clean and maintainable.
/// </summary>
public class ServiceAnalysis
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    
    public int ComplexityLevel { get; private set; }
    public int UrgencyLevel { get; private set; }
    public string? SafetyAdvice { get; private set; }
    public string? Summary { get; private set; }
    
    public bool IsCritical => UrgencyLevel >= 4;
    
    public DateTime AnalyzedAt { get; private set; }

    private ServiceAnalysis() { }

    private ServiceAnalysis(
        Guid id,
        Guid serviceRequestId,
        int complexityLevel,
        int urgencyLevel,
        string? safetyAdvice,
        string? summary)
    {
        Id = id;
        ServiceRequestId = serviceRequestId;
        ComplexityLevel = complexityLevel;
        UrgencyLevel = urgencyLevel;
        SafetyAdvice = safetyAdvice;
        Summary = summary;
        AnalyzedAt = DateTime.UtcNow;
    }

    public static ServiceAnalysis Create(
        Guid serviceRequestId,
        int complexityLevel,
        int urgencyLevel,
        string? safetyAdvice = null,
        string? summary = null)
    {
        return new ServiceAnalysis(
            Guid.NewGuid(),
            serviceRequestId,
            complexityLevel,
            urgencyLevel,
            safetyAdvice,
            summary);
    }
}
