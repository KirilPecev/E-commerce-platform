using ECommercePlatform.Domain;

using PaymentService.Domain.Exceptions;

namespace PaymentService.Domain.ValueObjects
{
    public class Money : ValueObject
    {
        public decimal Amount { get; }

        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new PaymentDomainException("Amount cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new PaymentDomainException("Currency must be a valid 3-letter ISO code.");

            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
