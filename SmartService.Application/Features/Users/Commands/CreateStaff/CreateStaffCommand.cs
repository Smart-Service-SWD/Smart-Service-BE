using MediatR;

namespace SmartService.Application.Features.Users.Commands.CreateStaff;

/// <summary>
/// Command to create a new staff user.
/// </summary>
public record CreateStaffCommand(
    string FullName,
    string Email,
    string PhoneNumber) : IRequest<Guid>;

