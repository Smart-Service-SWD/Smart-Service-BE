using MediatR;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Notifications;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Users.Commands.CreateStaff;

/// <summary>
/// Handler for CreateStaffCommand.
/// Creates a new staff user, AuthData entry, and sends welcome email.
/// </summary>
public class CreateStaffHandler : IRequestHandler<CreateStaffCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public CreateStaffHandler(
        IAppDbContext context,
        IAuthService authService,
        IEmailService emailService)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
    }

    public async Task<Guid> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = User.CreateStaff(request.FullName, request.Email, request.PhoneNumber);

        _context.Users.Add(staff);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate temporary password and create AuthData
        var tempPassword = GenerateTemporaryPassword();
        await _authService.CreateAuthDataAsync(staff.Id, staff.Email, tempPassword, cancellationToken);

        // Send welcome email with credentials
        try
        {
            await _emailService.SendWelcomeEmailAsync(staff.Email, staff.FullName, tempPassword, cancellationToken);
        }
        catch
        {
            // Email failure should not prevent user creation
        }

        return staff.Id;
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#!";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

