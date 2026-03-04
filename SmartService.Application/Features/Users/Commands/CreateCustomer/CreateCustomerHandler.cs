using MediatR;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Notifications;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Users.Commands.CreateCustomer;

/// <summary>
/// Handler for CreateCustomerCommand.
/// Creates a new customer user, AuthData entry, and sends welcome email.
/// </summary>
public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public CreateCustomerHandler(
        IAppDbContext context,
        IAuthService authService,
        IEmailService emailService)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = User.CreateCustomer(request.FullName, request.Email, request.PhoneNumber);

        _context.Users.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate temporary password and create AuthData
        var tempPassword = GenerateTemporaryPassword();
        await _authService.CreateAuthDataAsync(customer.Id, customer.Email, tempPassword, cancellationToken);

        // Send welcome email with credentials (fire-and-forget on failure to not block)
        try
        {
            await _emailService.SendWelcomeEmailAsync(customer.Email, customer.FullName, tempPassword, cancellationToken);
        }
        catch
        {
            // Email failure should not prevent user creation
        }

        return customer.Id;
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#!";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

