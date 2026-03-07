namespace SmartService.Application.DTOs;

/// <summary>
/// DTO returned by IAiAnalyzer.AnalyzeServiceRequestAsync.
/// Contains all fields the AI infers from the service request + definitions context.
/// </summary>
public class ServiceRequestAnalysisResultDto
{
    /// <summary>AI-assessed complexity level (1–5 scale).</summary>
    public int ComplexityLevel { get; set; } = 1;

    /// <summary>AI-assessed urgency level (1–5 scale, 4+ = critical).</summary>
    public int UrgencyLevel { get; set; } = 1;

    /// <summary>Short summary of the problem identified by AI.</summary>
    public string? Summary { get; set; }

    /// <summary>
    /// AI technical diagnosis of the specific problem situation.
    /// Always populated — describes what the customer is likely experiencing in technical terms.
    /// Example: "BSOD do driver lỗi hoặc RAM hỏng, mã lỗi 0x0E."
    /// </summary>
    public string? ProblemDiagnosis { get; set; }

    /// <summary>AI explanation of why the request is risky or dangerous. Null when not dangerous.</summary>
    public string? RiskExplanation { get; set; }

    /// <summary>Safety advice to communicate to the customer.</summary>
    public string? SafetyAdvice { get; set; }

    /// <summary>Human-readable price estimate. Example: "2.000.000 – 5.000.000 VNĐ"</summary>
    public string? EstimatedPrice { get; set; }

    /// <summary>Human-readable duration estimate. Example: "4 – 8 giờ"</summary>
    public string? EstimatedDuration { get; set; }

    /// <summary>
    /// True when the request should be flagged as dangerous.
    /// ALWAYS determined by deterministic post-processing in OllamaAiAnalyzer,
    /// NOT by the raw AI output value.
    /// </summary>
    public bool IsDangerFlagged { get; set; }
}

