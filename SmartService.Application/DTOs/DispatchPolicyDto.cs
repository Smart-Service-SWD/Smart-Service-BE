using System.Text.Json.Serialization;

namespace SmartService.Application.DTOs;

public class DispatchPolicyDto
{
    [JsonPropertyName("requiredSkillLevel")]
    public int RequiredSkillLevel { get; set; } = 1;

    [JsonPropertyName("minExperienceYears")]
    public int MinExperienceYears { get; set; } = 0;

    [JsonPropertyName("requiresCertification")]
    public bool RequiresCertification { get; set; } = false;

    [JsonPropertyName("requiresSeniorTechnician")]
    public bool RequiresSeniorTechnician { get; set; } = false;

    [JsonPropertyName("riskWeight")]
    public double RiskWeight { get; set; } = 0.0;
}
