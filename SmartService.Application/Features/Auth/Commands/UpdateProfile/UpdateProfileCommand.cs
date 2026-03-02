using MediatR;

namespace SmartService.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string FullName,
    string PhoneNumber
) : IRequest<bool>;
