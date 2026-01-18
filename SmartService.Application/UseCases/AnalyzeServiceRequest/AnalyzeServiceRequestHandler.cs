using SmartService.Application.Abstractions.AI;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.UseCases.AnalyzeServiceRequest;

public sealed class AnalyzeServiceRequestHandler
{
    private readonly IAiAnalyzer _ai;

    public AnalyzeServiceRequestHandler(IAiAnalyzer ai)
    {
        _ai = ai;
    }

public async Task<object> HandleAsync(string description)
{
    var aiResult = await _ai.AnalyzeAsync(description);

    // Bạn có thể vẫn dùng ServiceComplexity cho logic nghiệp vụ
    var normalizedLevel = Math.Clamp(aiResult.Policy.RequiredSkillLevel, 1, 5);
    var domainComplexity = ServiceComplexity.From(normalizedLevel);

    // Nhưng trả về toàn bộ thông tin để UI hiển thị Part A
    return new 
    {
        Complexity = domainComplexity.Level,
        UserMessage = aiResult.Context, // Gửi Summary, Risk, Safety cho User
        DispatchRules = aiResult.Policy // Gửi yêu cầu kỹ thuật cho System
    };
}

}
