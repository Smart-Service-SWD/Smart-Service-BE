using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceAgents.Commands.UpdateCapabilities;

public class UpdateServiceAgentCapabilitiesHandler
    : IRequestHandler<UpdateServiceAgentCapabilitiesCommand, UpdateServiceAgentCapabilitiesResult>
{
    private readonly IAppDbContext _context;

    public UpdateServiceAgentCapabilitiesHandler(IAppDbContext context)
        => _context = context;

    public async Task<UpdateServiceAgentCapabilitiesResult> Handle(
        UpdateServiceAgentCapabilitiesCommand request,
        CancellationToken cancellationToken)
    {
        var agent = await _context.ServiceAgents
            .FirstOrDefaultAsync(x => x.Id == request.AgentId, cancellationToken);

        if (agent == null)
            throw new KeyNotFoundException($"ServiceAgent with ID '{request.AgentId}' not found.");

        var categoryIds = request.Capabilities.Select(c => c.CategoryId).Distinct().ToList();
        var existingCategoryIds = await _context.ServiceCategories
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missingCategories = categoryIds.Except(existingCategoryIds).ToList();
        if (missingCategories.Count > 0)
            throw new KeyNotFoundException($"Service categories not found: {string.Join(", ", missingCategories)}");

        var allServiceDefinitionIds = request.Capabilities
            .SelectMany(c => c.ServiceIds)
            .Distinct()
            .ToList();

        var existingDefinitions = await _context.ServiceDefinitions
            .Where(d => allServiceDefinitionIds.Contains(d.Id))
            .Select(d => new { d.Id, d.CategoryId })
            .ToListAsync(cancellationToken);

        var missingDefinitions = allServiceDefinitionIds.Except(existingDefinitions.Select(d => d.Id)).ToList();
        if (missingDefinitions.Count > 0)
            throw new KeyNotFoundException($"Service definitions not found: {string.Join(", ", missingDefinitions)}");

        foreach (var capability in request.Capabilities)
        {
            foreach (var serviceId in capability.ServiceIds.Distinct())
            {
                var definition = existingDefinitions.First(d => d.Id == serviceId);
                if (definition.CategoryId != capability.CategoryId)
                    throw new ArgumentException(
                        $"Service definition '{serviceId}' does not belong to category '{capability.CategoryId}'.");
            }
        }

        var newCapabilities = request.Capabilities
            .Select(capability => AgentCapability.Create(
                capability.CategoryId,
                ServiceComplexity.From(capability.MaxComplexityLevel),
                capability.ServiceIds.Distinct()))
            .ToList();

        var existingCapabilities = await _context.AgentCapabilities
            .Where(capability => capability.ServiceAgentId == agent.Id)
            .ToListAsync(cancellationToken);

        if (existingCapabilities.Count > 0)
        {
            _context.AgentCapabilities.RemoveRange(existingCapabilities);
            await _context.SaveChangesAsync(cancellationToken);
        }

        foreach (var capability in newCapabilities)
        {
            capability.AssignToAgent(agent.Id);
            _context.AgentCapabilities.Add(capability);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateServiceAgentCapabilitiesResult(agent.Id, newCapabilities.Count);
    }
}
