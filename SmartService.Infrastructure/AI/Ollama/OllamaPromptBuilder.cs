namespace SmartService.Infrastructure.AI.Ollama;

public static class OllamaPromptBuilder
{
    /// <summary>
    /// Legacy prompt - used by AnalyzeAsync (old flow).
    /// </summary>
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

    /// <summary>
    /// New prompt for service request analysis.
    /// Uses ServiceDefinitions from DB as context so AI can estimate price/duration.
    /// </summary>
    /// <param name="description">Combined customer description + OCR text.</param>
    /// <param name="definitionsJson">JSON array of ServiceDefinitions for the request's category.</param>
    public static string BuildServiceRequestPrompt(string description, string definitionsJson)
    {
        return $$"""
    Bạn là AI phân tích yêu cầu dịch vụ kỹ thuật.

    ### DANH SÁCH DỊCH VỤ (context từ DB):
    {{definitionsJson}}

    ### YÊU CẦU KHÁCH HÀNG:
    {{description}}

    ### HƯỚNG DẪN:
    1. Dựa vào danh sách dịch vụ để ước tính giá và thời gian phù hợp.
    2. Đánh giá mức độ phức tạp (complexityLevel: 1–5).
    3. Đánh giá mức độ khẩn cấp (urgencyLevel: 1–5, 4+ = nguy cấp cần xử lý ngay).
    4. Nếu mô tả chứa từ khóa nguy hiểm (cháy, chập điện, gas rò, ngập điện, điện giật),
       thì riskExplanation phải được điền rõ ràng.
    5. estimatedPrice và estimatedDuration phải bằng tiếng Việt, dạng khoảng giá trị.
       Ví dụ: "2.000.000 – 5.000.000 VNĐ", "4 – 8 giờ".
    6. Nếu không đủ thông tin để ước tính, hãy ghi "Cần khảo sát thực tế".
    7. Ngôn ngữ của TẤT CẢ các trường phải cùng ngôn ngữ với mô tả khách hàng.
    8. problemDiagnosis: luôn điền, mô tả cụ thể vấn đề kỹ thuật đang gặp phải.
       Ví dụ: "Màn hình xanh chết chóc (BSOD) do driver lỗi hoặc RAM gặp sự cố."
              "Máy tính không vào được Windows do phân vùng khởi động bị hỏng."

    ### OUTPUT:
    Chỉ trả về JSON thuần, không markdown, không giải thích ngoài JSON.

    {
      "complexityLevel": number (1-5),
      "urgencyLevel": number (1-5),
      "summary": "Tóm tắt ngắn gọn vấn đề",
      "problemDiagnosis": "Chẩn đoán kỹ thuật cụ thể: vấn đề gì, có thể do nguyên nhân gì",
      "riskExplanation": "Giải thích lý do nguy hiểm nếu có (null nếu không nguy hiểm)",
      "safetyAdvice": "Lời khuyên an toàn cho khách hàng",
      "estimatedPrice": "X – Y VNĐ",
      "estimatedDuration": "X – Y giờ"
    }
    """;
    }
}

