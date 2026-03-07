using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.Create;

/// <summary>
/// Command to create a new service request with optional image for OCR + AI analysis.
/// 
/// Flow:
/// 1. If ImageStream provided → OCR extracts text
/// 2. Combined text (description + OCR) sent to AI for analysis
///    AI also loads ServiceDefinitions from DB by CategoryId as context
/// 3. AI result (complexity, urgency, price, duration, danger) written to DB
/// 4. If image provided → ServiceAttachment created automatically
/// 5. If isDangerFlagged → supervisor alert triggered
/// </summary>
public record CreateServiceRequestCommand(
    Guid CustomerId,
    Guid CategoryId,
    string Description,
    string? AddressText = null,
    Stream? ImageStream = null,
    string? ImageFileName = null) : IRequest<CreateServiceRequestResult>;

/// <summary>
/// Result returned after creating a service request.
/// Includes full AI analysis result.
/// </summary>
public record CreateServiceRequestResult(
    Guid ServiceRequestId,
    int? AiComplexityLevel,
    int? AiUrgencyLevel,
    string? AiSummary,
    string? AiProblemDiagnosis,
    string? AiRiskExplanation,
    string? AiSafetyAdvice,
    string? EstimatedPrice,
    string? EstimatedDuration,
    string? OcrExtractedText,
    bool WasAnalyzedByAI,
    bool IsDangerFlagged);

