using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Users.Commands.CreateStaff;

/// <summary>
/// Handler for CreateStaffCommand.
/// Creates a new staff user in the system.
/// </summary>
public class CreateStaffHandler : IRequestHandler<CreateStaffCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateStaffHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = User.CreateStaff(request.FullName, request.Email, request.PhoneNumber);

        _context.Users.Add(staff);
        await _context.SaveChangesAsync(cancellationToken);

        return staff.Id;
    }
}

