namespace SmartService.Application.DTOs;

public class AiAnalysisResultDto
{
    public ContextDescriptionDto Context { get; set; } = new();
    public DispatchPolicyDto Policy { get; set; } = new();

    // Backward compatibility
    public int ComplexityLevel { get; set; }
    
    // New fields for AI-driven matching
    public int UrgencyLevel { get; set; } = 1; // 1-5 scale
}
