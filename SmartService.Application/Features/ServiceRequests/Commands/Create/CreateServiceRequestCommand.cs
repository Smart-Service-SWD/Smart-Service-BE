using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.Create;

/// <summary>
/// Command to create a new service request with optional image for OCR + AI analysis.
/// 
/// Flow:
/// 1. If ImageStream provided → OCR extracts text
/// 2. Combined text (description + OCR) sent to AI for analysis
/// 3. AI result (complexity, urgency, context) written to DB in same transaction
/// 4. If image provided → ServiceAttachment created automatically
/// </summary>
public record CreateServiceRequestCommand(
    Guid CustomerId,
    Guid CategoryId,
    string Description,
    string? AddressText = null,
    // ComplexityLevel removed – AI derives it from description + OCR text automatically
    Stream? ImageStream = null,
    string? ImageFileName = null) : IRequest<CreateServiceRequestResult>;

/// <summary>
/// Result returned after creating a service request.
/// Includes AI analysis result if image/text was analyzed.
/// </summary>
public record CreateServiceRequestResult(
    Guid ServiceRequestId,
    int? AiComplexityLevel,
    string? AiSummary,
    string? AiRiskExplanation,
    string? OcrExtractedText,
    bool WasAnalyzedByAI);
