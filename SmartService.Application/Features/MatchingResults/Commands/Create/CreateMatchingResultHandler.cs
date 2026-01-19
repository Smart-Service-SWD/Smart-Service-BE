using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.MatchingResults.Commands.Create;

/// <summary>
/// Handler for CreateMatchingResultCommand.
/// Creates a matching evaluation between request and agent.
/// </summary>
public class CreateMatchingResultHandler : IRequestHandler<CreateMatchingResultCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateMatchingResultHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateMatchingResultCommand request, CancellationToken cancellationToken)
    {
        var matchingResult = MatchingResult.Create(
            request.ServiceRequestId,
            request.ServiceAgentId,
            request.SupportedComplexity,
            request.MatchingScore,
            request.IsRecommended);

        _context.MatchingResults.Add(matchingResult);
        await _context.SaveChangesAsync(cancellationToken);

        return matchingResult.Id;
    }
}

