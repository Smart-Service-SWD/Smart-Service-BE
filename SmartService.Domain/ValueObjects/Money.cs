using SmartService.Domain.Exceptions;

/// <summary>
/// Represents a monetary value in the domain.
/// 
/// Money is a Value Object and therefore immutable.
/// It enforces domain rules such as:
/// - No negative amounts
/// - Explicit currency handling
/// </summary>
namespace SmartService.Domain.ValueObjects
{
    public sealed class Money
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "USD";

        private Money() { } // EF Core

        private Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new DomainException("Amount cannot be negative.");

            Amount = amount;
            Currency = currency;
        }

        public static Money Create(decimal amount, string currency = "USD")
            => new(amount, currency);
    }
}