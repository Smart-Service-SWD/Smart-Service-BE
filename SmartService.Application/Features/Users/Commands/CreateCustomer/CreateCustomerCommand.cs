using MediatR;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Users.Commands.CreateCustomer;

/// <summary>
/// Command to create a new customer user.
/// </summary>
public record CreateCustomerCommand(
    string FullName,
    string Email,
    string PhoneNumber) : IRequest<Guid>;

