using MediatR;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.AI;
using SmartService.Application.Abstractions.Notifications;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceRequests.Commands.Create;

/// <summary>
/// Handles CreateServiceRequestCommand.
///
/// Unified single-transaction flow:
/// 1. OCR – extract text from image (if provided)
/// 2. AI  – analyze combined text using ServiceDefinitions from DB as context
///          → returns: complexity, urgency, price estimate, duration, danger flag
/// 3. DB  – write ServiceRequest  → table: service_requests
///          write ServiceAnalysis → table: service_analyses
///          write ServiceAttachment (if image) → table: service_attachments
///          all committed in ONE SaveChanges call
/// 4. Alert – if isDangerFlagged, notify supervisor via IServiceRequestNotificationService
/// </summary>
public class CreateServiceRequestHandler : IRequestHandler<CreateServiceRequestCommand, CreateServiceRequestResult>
{
    private readonly IAppDbContext _context;
    private readonly IOcrService _ocr;
    private readonly IAiAnalyzer _ai;
    private readonly IServiceRequestNotificationService _notifications;
    private readonly ILogger<CreateServiceRequestHandler> _logger;

    public CreateServiceRequestHandler(
        IAppDbContext context,
        IOcrService ocr,
        IAiAnalyzer ai,
        IServiceRequestNotificationService notifications,
        ILogger<CreateServiceRequestHandler> logger)
    {
        _context = context;
        _ocr = ocr;
        _ai = ai;
        _notifications = notifications;
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

                // Normalize empty / whitespace-only OCR output to null
                if (string.IsNullOrWhiteSpace(ocrText))
                    ocrText = null;

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
        // STEP 2 – AI: analyze using ServiceDefinitions from DB (new flow)
        // ══════════════════════════════════════════════════════════════
        var aiInput = BuildAiInput(request.Description, ocrText);

        var aiResult = await _ai.AnalyzeServiceRequestAsync(
            aiInput,
            request.CategoryId,
            _context,
            cancellationToken);

        var complexityLevel = Math.Clamp(aiResult.ComplexityLevel, 1, 5);
        var urgencyLevel    = Math.Clamp(aiResult.UrgencyLevel, 1, 5);
        var complexity      = ServiceComplexity.From(complexityLevel);

        _logger.LogInformation(
            "[AI] complexity={C} urgency={U} danger={D} price={P} duration={Dur}",
            complexityLevel, urgencyLevel, aiResult.IsDangerFlagged,
            aiResult.EstimatedPrice, aiResult.EstimatedDuration);

        // ══════════════════════════════════════════════════════════════
        // STEP 3 – CREATE ServiceRequest (with AI estimates)
        // ══════════════════════════════════════════════════════════════
        var serviceRequest = ServiceRequest.Create(
            request.CustomerId,
            request.CategoryId,
            request.Description,
            request.AddressText,
            complexity);

        serviceRequest.MarkAsAnalyzed(urgencyLevel);

        // Store AI estimates on the request entity
        serviceRequest.SetAiEstimates(
            aiResult.EstimatedPrice,
            aiResult.EstimatedDuration,
            ocrText);

        _context.ServiceRequests.Add(serviceRequest);

        // ══════════════════════════════════════════════════════════════
        // STEP 4 – CREATE ServiceAnalysis (AI result → DB)
        // ══════════════════════════════════════════════════════════════
        var analysis = ServiceAnalysis.Create(
            serviceRequestId: serviceRequest.Id,
            complexityLevel:  complexityLevel,
            urgencyLevel:     urgencyLevel,
            safetyAdvice:     aiResult.SafetyAdvice,
            summary:          aiResult.Summary,
            riskExplanation:  aiResult.RiskExplanation,
            problemDiagnosis: aiResult.ProblemDiagnosis);

        _context.ServiceAnalyses.Add(analysis);

        // ══════════════════════════════════════════════════════════════
        // STEP 5 – CREATE ServiceAttachment (if image was uploaded)
        // ══════════════════════════════════════════════════════════════
        if (request.ImageStream is not null && !string.IsNullOrWhiteSpace(request.ImageFileName))
        {
            var attachment = ServiceAttachment.Create(
                serviceRequestId: serviceRequest.Id,
                fileName:         request.ImageFileName,
                fileUrl:          $"local://{request.ImageFileName}",
                type:             AttachmentType.Image);

            _context.ServiceAttachments.Add(attachment);
        }

        // ══════════════════════════════════════════════════════════════
        // STEP 6 – COMMIT: single round-trip writes all tables
        // ══════════════════════════════════════════════════════════════
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[CREATE] ServiceRequest {Id} saved — Analysis {AId} | isDangerFlagged={D}",
            serviceRequest.Id, analysis.Id, aiResult.IsDangerFlagged);

        // ══════════════════════════════════════════════════════════════
        // STEP 7 – ALERT: if danger flagged, notify supervisor
        // ══════════════════════════════════════════════════════════════
        if (aiResult.IsDangerFlagged)
        {
            try
            {
                await _notifications.NotifyDangerFlaggedAsync(
                    serviceRequest.Id,
                    aiResult.RiskExplanation,
                    cancellationToken);

                _logger.LogWarning(
                    "[DANGER] ServiceRequest {Id} flagged as dangerous. Alert sent.",
                    serviceRequest.Id);
            }
            catch (Exception ex)
            {
                // Alert failure is non-fatal – request is already saved
                _logger.LogError(ex, "[DANGER] Failed to send danger alert for ServiceRequest {Id}", serviceRequest.Id);
            }
        }

        return new CreateServiceRequestResult(
            ServiceRequestId:    serviceRequest.Id,
            AiComplexityLevel:   complexityLevel,
            AiUrgencyLevel:      urgencyLevel,
            AiSummary:           aiResult.Summary,
            AiProblemDiagnosis:  aiResult.ProblemDiagnosis,
            AiRiskExplanation:   aiResult.RiskExplanation,
            AiSafetyAdvice:      aiResult.SafetyAdvice,
            EstimatedPrice:      aiResult.EstimatedPrice,
            EstimatedDuration:   aiResult.EstimatedDuration,
            OcrExtractedText:    ocrText,
            WasAnalyzedByAI:     true,
            IsDangerFlagged:     aiResult.IsDangerFlagged);
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
