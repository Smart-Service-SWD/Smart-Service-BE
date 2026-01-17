using System.Text.Json.Serialization;

namespace SmartService.Application.DTOs;

public class ContextDescriptionDto
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("riskExplanation")]
    public string RiskExplanation { get; set; } = string.Empty;

    [JsonPropertyName("safetyAdvice")]
    public string SafetyAdvice { get; set; } = string.Empty;
}
