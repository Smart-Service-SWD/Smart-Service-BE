using MediatR;
using SmartService.Application.DTOs;

namespace SmartService.Application.Features.ServiceRequests.Commands.MatchAgents;

/// <summary>
/// Command to find and match suitable agents for a service request.
/// Returns a list of matching agents with scores.
/// </summary>
public record MatchAgentsForServiceRequestCommand(Guid ServiceRequestId) : IRequest<List<AgentMatchDto>>;

/// <summary>
/// DTO representing a matched agent with score.
/// </summary>
public class AgentMatchDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public double Score { get; set; }
    public bool IsRecommended { get; set; }
    public string Reason { get; set; } = string.Empty;
}
