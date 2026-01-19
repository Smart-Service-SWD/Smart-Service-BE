using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Users.Commands.CreateCustomer;

/// <summary>
/// Handler for CreateCustomerCommand.
/// Creates a new customer user in the system.
/// </summary>
public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateCustomerHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = User.CreateCustomer(request.FullName, request.Email, request.PhoneNumber);

        _context.Users.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}

