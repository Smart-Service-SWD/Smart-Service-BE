namespace SmartService.Domain.Exceptions
{
    // ====================== VALUE OBJECT MONEY DOMAIN EXCEPTIONS ====================

    /// <summary>
    /// Exception for value object Money-related domain issues.
    /// </summary>
    public abstract class MoneyException : DomainException
    {
        protected MoneyException(string message) : base(message) { }

        public class NegativeAmountException : MoneyException
        {
            public NegativeAmountException(decimal amount)
                : base($"The monetary value '{amount}' cannot be negative.") { }
        }

        public class UnsupportedCurrencyException : MoneyException
        {
            public UnsupportedCurrencyException(string currency)
                : base($"The currency '{currency}' is not supported. Supported currencies include: USD, EUR.") { }
        }

        public class InvalidAmountPrecisionException : MoneyException
        {
            public InvalidAmountPrecisionException(decimal amount)
                : base($"The monetary value '{amount}' exceeds the allowed precision for this operation.") { }
        }
    }
}
