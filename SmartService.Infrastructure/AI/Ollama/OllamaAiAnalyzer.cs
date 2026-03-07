using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.AI;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.DTOs;
using SmartService.Infrastructure.KnowledgeBase.Complexity;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SmartService.Infrastructure.AI.Ollama;

public sealed class OllamaAiAnalyzer : IAiAnalyzer
{
    private readonly OllamaClient _client;
    private readonly ComplexityRuleProvider _rules;

    // Keywords that trigger isDangerFlagged = true regardless of AI output.
    // IMPORTANT: these are matched against the raw customer description ONLY (not safetyAdvice).
    // Keep this list TIGHT – overly broad keywords cause false positives.
    private static readonly string[] DangerKeywords =
    [
        "cháy",         // fire
        "chập điện",    // electrical short
        "chập cháy",    // short-circuit fire
        "gas rò",       // gas leak
        "rò gas",
        "ngập điện",    // water + electricity flooding
        "điện giật",    // electric shock
        "bùng cháy",    // fire outbreak
        "nổ bình",      // tank/cylinder explosion
        "cháy nhà",     // house fire
    ];

    public OllamaAiAnalyzer(
        OllamaClient client,
        ComplexityRuleProvider rules)
    {
        _client = client;
        _rules = rules;
    }

    // ─── Legacy method (old flow) ───────────────────────────────────────────────

    public async Task<AiAnalysisResultDto> AnalyzeAsync(string description, CancellationToken ct = default)
    {
        var rule = _rules.GetRule("TECH_ELECTRIC");
        var ruleJson = JsonSerializer.Serialize(rule);
        var prompt = OllamaPromptBuilder.BuildContextAndPolicyPrompt(description, ruleJson);

        var rawResponse = await _client.GenerateAsync("yasserrmd/Qwen2.5-7B-Instruct-1M", prompt);

        return ParseLegacyAiResponse(rawResponse);
    }

    // ─── New method: loads context from DB ───────────────────────────────────────

    public async Task<ServiceRequestAnalysisResultDto> AnalyzeServiceRequestAsync(
        string description,
        Guid categoryId,
        IAppDbContext context,
        CancellationToken ct = default)
    {
        // STEP 1 – Load definitions from DB for this category
        var definitions = await context.ServiceDefinitions
            .Where(d => d.CategoryId == categoryId && d.IsActive)
            .ToListAsync(ct);

        // STEP 2 – Serialize definitions as AI context (only the fields AI needs)
        var definitionsContext = definitions.Select(d => new
        {
            name = d.Name,
            description = d.Description,
            basePriceVnd = d.BasePrice,
            estimatedDurationMinutes = d.EstimatedDuration,
            complexityRange = d.ComplexityRange,
            isDangerous = d.IsDangerous
        });

        var definitionsJson = JsonSerializer.Serialize(definitionsContext, new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        // STEP 3 – Build prompt and call Ollama
        var prompt = OllamaPromptBuilder.BuildServiceRequestPrompt(description, definitionsJson);
        var rawResponse = await _client.GenerateAsync("yasserrmd/Qwen2.5-7B-Instruct-1M", prompt);

        // STEP 4 – Parse AI response
        var result = ParseServiceRequestAiResponse(rawResponse);

        // STEP 5 – Post-process isDangerFlagged (OVERRIDE AI's value with deterministic logic).
        // We do NOT trust the AI's isDangerFlagged flag because LLMs hallucinate.
        // Our logic: dangerous if definition is flagged, complexity is high, OR specific danger keywords found.
        var anyDefinitionIsDangerous = definitions.Any(d => d.IsDangerous);
        var hasKeyword = ContainsDangerKeywords(description);

        // Full override — regardless of what AI returned
        result.IsDangerFlagged = anyDefinitionIsDangerous || result.ComplexityLevel >= 4 || hasKeyword;


        return result;
    }

    // ─── Parsers ────────────────────────────────────────────────────────────────

    private AiAnalysisResultDto ParseLegacyAiResponse(string rawResponse)
    {
        try
        {
            var cleanJson = CleanJson(rawResponse);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var aiOutput = JsonSerializer.Deserialize<AiRawOutput>(cleanJson, options);

            return new AiAnalysisResultDto
            {
                Context = aiOutput?.ContextDescription ?? new(),
                Policy = aiOutput?.DispatchPolicy ?? new(),
                ComplexityLevel = aiOutput?.DispatchPolicy?.RequiredSkillLevel ?? 1,
                UrgencyLevel = Math.Clamp(aiOutput?.UrgencyLevel ?? 1, 1, 5)
            };
        }
        catch
        {
            return new AiAnalysisResultDto
            {
                Context = new(),
                Policy = new(),
                ComplexityLevel = 1,
                UrgencyLevel = 1
            };
        }
    }

    private ServiceRequestAnalysisResultDto ParseServiceRequestAiResponse(string rawResponse)
    {
        try
        {
            var cleanJson = CleanJson(rawResponse);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var output = JsonSerializer.Deserialize<ServiceRequestRawOutput>(cleanJson, options);

            return new ServiceRequestAnalysisResultDto
            {
                ComplexityLevel   = Math.Clamp(output?.ComplexityLevel ?? 1, 1, 5),
                UrgencyLevel      = Math.Clamp(output?.UrgencyLevel ?? 1, 1, 5),
                Summary           = output?.Summary,
                ProblemDiagnosis  = output?.ProblemDiagnosis,
                RiskExplanation   = output?.RiskExplanation,
                SafetyAdvice      = output?.SafetyAdvice,
                EstimatedPrice    = output?.EstimatedPrice,
                EstimatedDuration = output?.EstimatedDuration,
                IsDangerFlagged   = output?.IsDangerFlagged ?? false  // will be overridden by post-process
            };
        }
        catch
        {
            // Fallback – safe defaults
            return new ServiceRequestAnalysisResultDto
            {
                ComplexityLevel = 1,
                UrgencyLevel    = 1
            };
        }
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static string CleanJson(string raw)
    {
        var clean = raw.Trim();
        if (clean.StartsWith("```"))
        {
            clean = Regex.Replace(clean, @"^```(?:json)?|```$", "", RegexOptions.Multiline).Trim();
        }
        return clean;
    }

    private static bool ContainsDangerKeywords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var lower = text.ToLowerInvariant();
        return DangerKeywords.Any(kw => lower.Contains(kw));
    }
}

// ─── Raw output shape for new prompt ────────────────────────────────────────

internal sealed class ServiceRequestRawOutput
{
    [JsonPropertyName("complexityLevel")]
    public int ComplexityLevel { get; set; } = 1;

    [JsonPropertyName("urgencyLevel")]
    public int UrgencyLevel { get; set; } = 1;

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("problemDiagnosis")]
    public string? ProblemDiagnosis { get; set; }

    [JsonPropertyName("riskExplanation")]
    public string? RiskExplanation { get; set; }

    [JsonPropertyName("safetyAdvice")]
    public string? SafetyAdvice { get; set; }

    [JsonPropertyName("estimatedPrice")]
    public string? EstimatedPrice { get; set; }

    [JsonPropertyName("estimatedDuration")]
    public string? EstimatedDuration { get; set; }

    [JsonPropertyName("isDangerFlagged")]
    public bool IsDangerFlagged { get; set; }
}