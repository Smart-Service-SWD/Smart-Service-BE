using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceAgents.Queries.SearchServiceAgents;

public record SearchServiceAgentsQuery(
    Guid? CategoryId,
    Guid? ServiceDefinitionId,
    int? MinComplexityLevel,
    int PageNumber = 1,
    int PageSize = 5) : IRequest<PagedServiceAgentsResult>;

public record PagedServiceAgentsResult(
    IReadOnlyList<ServiceAgentSearchItem> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record ServiceAgentSearchItem(
    Guid Id,
    string FullName,
    bool IsActive,
    bool IsBusy,
    int MatchingScore,
    string? BusyReason);
