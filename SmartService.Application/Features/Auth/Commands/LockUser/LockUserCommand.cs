using MediatR;

namespace SmartService.Application.Features.Auth.Commands.LockUser;

/// <summary>
/// Command to lock or unlock a user account. Admin only.
/// </summary>
/// <param name="UserId">ID of the target user (Staff or Agent).</param>
/// <param name="IsLocked">True to lock, false to unlock.</param>
public sealed record LockUserCommand(Guid UserId, bool IsLocked) : IRequest<bool>;
