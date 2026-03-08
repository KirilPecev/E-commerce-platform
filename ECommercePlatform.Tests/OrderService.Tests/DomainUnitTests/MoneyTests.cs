using FluentAssertions;

using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Tests.DomainUnitTests
{
    public class MoneyTests
    {
        [Fact]
        public void Ctor_SetsProperties_WhenValid()
        {
            var money = new Money(100.50m, "USD");

            money.Amount.Should().Be(100.50m);
            money.Currency.Should().Be("USD");
        }

        [Fact]
        public void Ctor_NormalizesCurrency_ToUpperCase()
        {
            var money = new Money(10m, "eur");

            money.Currency.Should().Be("EUR");
        }

        [Fact]
        public void Ctor_AllowsZeroAmount()
        {
            var money = new Money(0m, "USD");

            money.Amount.Should().Be(0m);
        }

        [Fact]
        public void Ctor_Throws_WhenAmountIsNegative()
        {
            Action act = () => new Money(-1m, "USD");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Amount cannot be negative.");
        }

        [Fact]
        public void Ctor_Throws_WhenCurrencyIsEmpty()
        {
            Action act = () => new Money(10m, "");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Currency must be a valid 3-letter ISO code.");
        }

        [Fact]
        public void Ctor_Throws_WhenCurrencyIsNull()
        {
            Action act = () => new Money(10m, null!);

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Currency must be a valid 3-letter ISO code.");
        }

        [Fact]
        public void Ctor_Throws_WhenCurrencyIsTooShort()
        {
            Action act = () => new Money(10m, "US");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Currency must be a valid 3-letter ISO code.");
        }

        [Fact]
        public void Ctor_Throws_WhenCurrencyIsTooLong()
        {
            Action act = () => new Money(10m, "USDD");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Currency must be a valid 3-letter ISO code.");
        }

        [Fact]
        public void Equality_ShouldBeEqual_WhenSameValues()
        {
            var money1 = new Money(50m, "USD");
            var money2 = new Money(50m, "USD");

            money1.Should().Be(money2);
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenDifferentAmount()
        {
            var money1 = new Money(50m, "USD");
            var money2 = new Money(100m, "USD");

            money1.Should().NotBe(money2);
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenDifferentCurrency()
        {
            var money1 = new Money(50m, "USD");
            var money2 = new Money(50m, "EUR");

            money1.Should().NotBe(money2);
        }
    }
}
