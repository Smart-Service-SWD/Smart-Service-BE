using MediatR;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Auth.Commands.UpdateUserRole;

public sealed record UpdateUserRoleCommand(Guid UserId, UserRole Role) : IRequest<bool>;

