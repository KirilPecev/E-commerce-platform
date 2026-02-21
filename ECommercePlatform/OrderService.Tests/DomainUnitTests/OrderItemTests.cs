using FluentAssertions;

using OrderService.Domain.Aggregates;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Tests.DomainUnitTests
{
    public class OrderItemTests
    {
        [Fact]
        public void Ctor_SetsProperties()
        {
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var unitPrice = new Money(10m, "USD");

            var item = new OrderItem(productId, variantId, "Product A", unitPrice, 3);

            item.Id.Should().NotBe(Guid.Empty);
            item.ProductId.Should().Be(productId);
            item.ProductVariantId.Should().Be(variantId);
            item.ProductName.Should().Be("Product A");
            item.Quantity.Should().Be(3);
            item.UnitPrice.Amount.Should().Be(10m);
            item.UnitPrice.Currency.Should().Be("USD");
            item.TotalPrice.Should().Be(30m);
        }

        [Fact]
        public void Ctor_Throws_WhenQuantityNotPositive()
        {
            Action act = () => new OrderItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(5m, "USD"), 0);

            act.Should().Throw<OrderDomainException>().WithMessage("Quantity must be greater than zero");
        }

        [Fact]
        public void IncreaseQuantity_Increases_WhenValid()
        {
            var item = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(2m, "USD"), 1);

            item.IncreaseQuantity(4);

            item.Quantity.Should().Be(5);
            item.TotalPrice.Should().Be(10m);
        }

        [Fact]
        public void IncreaseQuantity_Throws_WhenNotPositive()
        {
            var item = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(2m, "USD"), 1);

            Action act = () => item.IncreaseQuantity(0);

            act.Should().Throw<OrderDomainException>().WithMessage("Quantity must be greater than zero");
        }
    }
}
