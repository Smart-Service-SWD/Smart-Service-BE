namespace SmartService.Application.DTOs;

public class AiAnalysisResultDto
{
    public ContextDescriptionDto Context { get; set; } = new();
    public DispatchPolicyDto Policy { get; set; } = new();

    // Backward compatibility
    public int ComplexityLevel { get; set; }
}
