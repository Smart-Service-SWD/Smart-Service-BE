using MediatR;

namespace SmartService.Application.Features.Users.Commands.CreateAgent;

/// <summary>
/// Command to create a new agent user.
/// </summary>
public record CreateAgentCommand(
    string FullName,
    string Email,
    string PhoneNumber) : IRequest<Guid>;

