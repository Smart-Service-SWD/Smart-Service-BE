using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.MatchingResults.Commands.Create;

/// <summary>
/// Command to create a new matching result.
/// </summary>
public record CreateMatchingResultCommand(
    Guid ServiceRequestId,
    Guid ServiceAgentId,
    ServiceComplexity SupportedComplexity,
    decimal MatchingScore,
    bool IsRecommended) : IRequest<Guid>;

