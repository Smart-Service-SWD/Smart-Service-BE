using SmartService.Application.DTOs;
using System.Text.Json.Serialization;

namespace SmartService.Infrastructure.AI.Ollama;

internal sealed class AiRawOutput
{
    [JsonPropertyName("contextDescription")]
    public ContextDescriptionDto ContextDescription { get; set; } = new();

    [JsonPropertyName("dispatchPolicy")]
    public DispatchPolicyDto DispatchPolicy { get; set; } = new();
    
    [JsonPropertyName("urgencyLevel")]
    public int UrgencyLevel { get; set; } = 1;
}
