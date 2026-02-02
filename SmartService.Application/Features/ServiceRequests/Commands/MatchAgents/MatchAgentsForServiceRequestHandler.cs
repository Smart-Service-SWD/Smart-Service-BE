using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.DTOs;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.MatchAgents;

/// <summary>
/// Handler for MatchAgentsForServiceRequestCommand.
/// Implements a matching algorithm to find suitable agents based on:
/// - Service category
/// - Service complexity
/// - Agent availability (IsActive)
/// - Agent capabilities (max complexity level)
/// </summary>
public class MatchAgentsForServiceRequestHandler : IRequestHandler<MatchAgentsForServiceRequestCommand, List<AgentMatchDto>>
{
    private readonly IAppDbContext _context;

    public MatchAgentsForServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<List<AgentMatchDto>> Handle(MatchAgentsForServiceRequestCommand request, CancellationToken cancellationToken)
    {
        // Get the service request with its complexity and category
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new DomainException("Service request not found.");

        if (serviceRequest.Complexity == null)
            throw new DomainException("Service request must have complexity evaluated before matching agents.");

        // Get all active agents with their capabilities
        var agents = await _context.ServiceAgents
            .Include(a => a.Capabilities)
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        var matches = new List<AgentMatchDto>();

        foreach (var agent in agents)
        {
            // Check if agent has capability for this category
            var capability = agent.Capabilities
                .FirstOrDefault(c => c.CategoryId == serviceRequest.CategoryId);

            if (capability == null)
                continue; // Agent not qualified for this category

            // Check if agent can handle this complexity level
            if (capability.MaxComplexity.Level < serviceRequest.Complexity.Level)
                continue; // Agent cannot handle this complexity

            // Calculate match score
            // Score is based on how close the agent's max complexity is to the required complexity
            // Agents with exact match get higher scores
            var complexityDiff = capability.MaxComplexity.Level - serviceRequest.Complexity.Level;
            var score = 100 - (complexityDiff * 10); // Perfect match = 100, each level over reduces by 10

            // Determine if this is a recommended match (score >= 80)
            var isRecommended = score >= 80;

            var reason = complexityDiff switch
            {
                0 => "Perfect complexity match",
                1 => "Slight overqualified",
                _ => "Significantly overqualified"
            };

            matches.Add(new AgentMatchDto
            {
                AgentId = agent.Id,
                AgentName = agent.FullName,
                Score = score,
                IsRecommended = isRecommended,
                Reason = reason
            });
        }

        // Sort by score descending (best matches first)
        return matches.OrderByDescending(m => m.Score).ToList();
    }
}
