using SmartService.Application.Abstractions.AI;
using SmartService.Application.DTOs;
using SmartService.Infrastructure.KnowledgeBase.Complexity;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartService.Infrastructure.AI.Ollama;

public sealed class OllamaAiAnalyzer : IAiAnalyzer
{
    private readonly OllamaClient _client;
    private readonly ComplexityRuleProvider _rules;

    public OllamaAiAnalyzer(
        OllamaClient client,
        ComplexityRuleProvider rules)
    {
        _client = client;
        _rules = rules;
    }

    public async Task<AiAnalysisResultDto> AnalyzeAsync(string description, CancellationToken ct = default)
    {
        var rule = _rules.GetRule("TECH_ELECTRIC");
        var ruleJson = JsonSerializer.Serialize(rule);
        var prompt = OllamaPromptBuilder.BuildContextAndPolicyPrompt(description, ruleJson);

        var rawResponse = await _client.GenerateAsync("yasserrmd/Qwen2.5-7B-Instruct-1M", prompt);

        return ParseAiResponse(rawResponse);
    }

    private AiAnalysisResultDto ParseAiResponse(string rawResponse)
    {
        try
        {
            // 1. Làm sạch chuỗi (Bỏ markdown ```json ... ``` nếu có)
            var cleanJson = rawResponse.Trim();
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = System.Text.RegularExpressions.Regex.Replace(cleanJson, @"^```(?:json)?|```$", "").Trim();
            }

            // 2. Deserialize theo cấu trúc Prompt đã định nghĩa
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
            // Fallback nếu AI trả về rác
            return new AiAnalysisResultDto
            {
                Context = new(),
                Policy = new(),
                ComplexityLevel = 1,
                UrgencyLevel = 1
            };
        }
    }
}