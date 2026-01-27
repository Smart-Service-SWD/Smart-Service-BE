namespace SmartService.Infrastructure.AI.Ollama;

public static class OllamaPromptBuilder
{
    public static string BuildContextAndPolicyPrompt(string description, string ruleJson)
{
    return $$"""
    You are a strict Logic Mapper. Your only job is to classify a "Service Description" into one of the "Levels" defined in the "Rules (JSON)".

    ### RULES (JSON):
    {{ruleJson}}

    ### INPUT:
    Description: "{{description}}"

    ### INSTRUCTIONS:
    1. Scan the Description for technical keywords.
    2. Look for these keywords in the "criteria" field of the Rules (JSON).
    3. If "3 pha" or "cân pha" is mentioned, you MUST select Level 4.
    4. Use the EXACT values (minExperienceYears, requiresCertification, etc.) from the selected Level.
    5. The "contextDescription" must be in the SAME LANGUAGE as the Description.

    ### OUTPUT REQUIREMENT:
    Return ONLY a JSON object. No preamble, no explanation outside the JSON.

    {
      "contextDescription": {
        "summary": "...",
        "riskExplanation": "...",
        "safetyAdvice": "..."
      },
      "dispatchPolicy": {
        "selectedLevelReason": "Mention the specific criteria matched from Rules",
        "requiredSkillLevel": number,
        "minExperienceYears": number,
        "requiresCertification": true|false,
        "requiresSeniorTechnician": true|false,
        "riskWeight": float
      },
      "urgencyLevel": number (1-5, where 4-5 = critical/urgent)
    }
    """;
}
}
