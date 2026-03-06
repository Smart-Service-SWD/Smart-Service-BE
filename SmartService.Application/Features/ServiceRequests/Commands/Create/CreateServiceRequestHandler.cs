using MediatR;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.AI;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceRequests.Commands.Create;

/// <summary>
/// Handles CreateServiceRequestCommand.
///
/// Unified single-transaction flow:
/// 1. OCR – extract text from image (if provided)
/// 2. AI  – analyze combined text (OCR + description)
/// 3. DB  – write ServiceRequest  → table: service_requests
///          write ServiceAnalysis → table: service_analyses
///          write ServiceAttachment (if image) → table: service_attachments
///          all committed in ONE SaveChanges call
/// </summary>
public class CreateServiceRequestHandler : IRequestHandler<CreateServiceRequestCommand, CreateServiceRequestResult>
{
    private readonly IAppDbContext _context;
    private readonly IOcrService _ocr;
    private readonly IAiAnalyzer _ai;
    private readonly ILogger<CreateServiceRequestHandler> _logger;

    public CreateServiceRequestHandler(
        IAppDbContext context,
        IOcrService ocr,
        IAiAnalyzer ai,
        ILogger<CreateServiceRequestHandler> logger)
    {
        _context = context;
        _ocr = ocr;
        _ai = ai;
        _logger = logger;
    }

    public async Task<CreateServiceRequestResult> Handle(
        CreateServiceRequestCommand request,
        CancellationToken cancellationToken)
    {
        // ══════════════════════════════════════════════════════════════
        // STEP 1 – OCR: extract text from image (optional)
        // ══════════════════════════════════════════════════════════════
        string? ocrText = null;

        if (request.ImageStream is not null && !string.IsNullOrWhiteSpace(request.ImageFileName))
        {
            try
            {
                ocrText = await _ocr.ExtractTextAsync(
                    request.ImageStream,
                    request.ImageFileName,
                    cancellationToken);

                _logger.LogInformation(
                    "[OCR] Extracted {Words} words from '{File}'",
                    ocrText?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0,
                    request.ImageFileName);
            }
            catch (Exception ex)
            {
                // OCR failure is non-fatal – continue with text-only analysis
                _logger.LogWarning(ex, "[OCR] Failed for '{File}', proceeding without OCR text", request.ImageFileName);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // STEP 2 – AI: analyze description + OCR text
        // ══════════════════════════════════════════════════════════════
        var aiInput = BuildAiInput(request.Description, ocrText);
        var aiResult = await _ai.AnalyzeAsync(aiInput, cancellationToken);

        var complexityLevel = Math.Clamp(aiResult.ComplexityLevel, 1, 5);
        var urgencyLevel    = Math.Clamp(aiResult.UrgencyLevel, 1, 5);
        var complexity      = ServiceComplexity.From(complexityLevel);

        _logger.LogInformation(
            "[AI] complexity={C} urgency={U}",
            complexityLevel, urgencyLevel);

        // ══════════════════════════════════════════════════════════════
        // STEP 3 – CREATE ServiceRequest
        // ══════════════════════════════════════════════════════════════
        var serviceRequest = ServiceRequest.Create(
            request.CustomerId,
            request.CategoryId,
            request.Description,   // store original description only
            request.AddressText,
            complexity);

        serviceRequest.MarkAsAnalyzed(urgencyLevel);
        _context.ServiceRequests.Add(serviceRequest);

        // ══════════════════════════════════════════════════════════════
        // STEP 4 – CREATE ServiceAnalysis (AI result → DB)
        // ══════════════════════════════════════════════════════════════
        var analysis = ServiceAnalysis.Create(
            serviceRequestId: serviceRequest.Id,
            complexityLevel:  complexityLevel,
            urgencyLevel:     urgencyLevel,
            safetyAdvice:     aiResult.Context?.SafetyAdvice,
            summary:          aiResult.Context?.Summary);

        _context.ServiceAnalyses.Add(analysis);

        // ══════════════════════════════════════════════════════════════
        // STEP 5 – CREATE ServiceAttachment (if image was uploaded)
        // ══════════════════════════════════════════════════════════════
        if (request.ImageStream is not null && !string.IsNullOrWhiteSpace(request.ImageFileName))
        {
            // FileUrl: in production replace with a real cloud storage URL.
            // For now we store the filename so the record exists in DB.
            var attachment = ServiceAttachment.Create(
                serviceRequestId: serviceRequest.Id,
                fileName:         request.ImageFileName,
                fileUrl:          $"local://{request.ImageFileName}",
                type:             AttachmentType.Image);

            _context.ServiceAttachments.Add(attachment);
        }

        // ══════════════════════════════════════════════════════════════
        // STEP 6 – COMMIT: single round-trip writes all 3 tables
        // ══════════════════════════════════════════════════════════════
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[CREATE] ServiceRequest {Id} saved with Analysis {AId}",
            serviceRequest.Id, analysis.Id);

        return new CreateServiceRequestResult(
            ServiceRequestId:    serviceRequest.Id,
            AiComplexityLevel:   complexityLevel,
            AiSummary:           aiResult.Context?.Summary,
            AiRiskExplanation:   aiResult.Context?.RiskExplanation,
            OcrExtractedText:    ocrText,
            WasAnalyzedByAI:     true);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the combined text sent to AI.
    /// OCR content is prepended so the AI sees the full picture.
    /// </summary>
    private static string BuildAiInput(string description, string? ocrText)
    {
        if (string.IsNullOrWhiteSpace(ocrText))
            return description;

        return $"[Nội dung trích xuất từ ảnh / tài liệu (OCR)]:\n{ocrText}\n\n" +
               $"[Mô tả vấn đề của khách hàng]:\n{description}";
    }
}
