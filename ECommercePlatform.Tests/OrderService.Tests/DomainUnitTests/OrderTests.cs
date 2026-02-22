using FluentAssertions;

using OrderService.Domain.Aggregates;
using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Tests.DomainUnitTests
{
    public class OrderTests
    {
        [Fact]
        public void Ctor_SetsDefaults()
        {
            var customerId = Guid.NewGuid();

            var order = new Order(customerId);

            order.CustomerId.Should().Be(customerId);
            order.Status.Should().Be(OrderStatus.Draft);
            order.Items.Should().BeEmpty();
            order.TotalPrice.Should().Be(0);
            order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void AddItem_CreatesNewItem_AndAddsDomainEvent()
        {
            var order = new Order(Guid.NewGuid());

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var (item, created) = order.AddItem(productId, variantId, "Product A", new Money(10m, "USD"), 2);

            created.Should().BeTrue();
            order.Items.Should().ContainSingle().Which.Should().Be(item);
            order.TotalPrice.Should().Be(20m);

            order.DomainEvents.Should().ContainSingle(ev => ev.GetType() == typeof(OrderCreatedDomainEvent) && ((OrderCreatedDomainEvent)ev).ProductId == productId && ((OrderCreatedDomainEvent)ev).ProductVariantId == variantId && ((OrderCreatedDomainEvent)ev).Quantity == 2);
        }

        [Fact]
        public void AddItem_IncreasesExistingItemQuantity()
        {
            var order = new Order(Guid.NewGuid());

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            order.AddItem(productId, variantId, "Product A", new Money(5m, "USD"), 1);
            var (item, created) = order.AddItem(productId, variantId, "Product A", new Money(5m, "USD"), 3);

            created.Should().BeFalse();
            order.Items.Should().HaveCount(1);
            order.Items.First().Quantity.Should().Be(4);
            order.TotalPrice.Should().Be(20m);
        }

        [Fact]
        public void AddItem_Throws_WhenNotDraft()
        {
            var order = new Order(Guid.NewGuid());
            order.SetShippingAddress(new Address("Street", "City", "Zip", "Country"));
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "p", new Money(1m, "USD"), 1);
            order.FinalizeOrder();

            Action act = () => order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "p2", new Money(1m, "USD"), 1);

            act.Should().Throw<OrderDomainException>().WithMessage("Cannot modify a finalized order.");
        }

        [Fact]
        public void FinalizeOrder_ValidatesAndAddsEvent()
        {
            var order = new Order(Guid.NewGuid());
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            order.AddItem(productId, variantId, "P", new Money(12.5m, "EUR"), 2);
            order.SetShippingAddress(new Address("St", "C", "Z", "Country"));

            order.FinalizeOrder();

            order.Status.Should().Be(OrderStatus.Finalized);
            order.DomainEvents.Should().ContainSingle(ev => ev.GetType() == typeof(OrderFinalizedDomainEvent) && ((OrderFinalizedDomainEvent)ev).TotalPrice == order.TotalPrice && ((OrderFinalizedDomainEvent)ev).Currency == "EUR");
        }

        [Fact]
        public void FinalizeOrder_Throws_WhenNoItemsOrNoAddress()
        {
            var order = new Order(Guid.NewGuid());

            Action actEmpty = () => order.FinalizeOrder();
            actEmpty.Should().Throw<OrderDomainException>().WithMessage("Cannot finalize an empty order.");

            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);
            Action actNoAddress = () => order.FinalizeOrder();
            actNoAddress.Should().Throw<OrderDomainException>().WithMessage("Cannot finalize an order without adress.");
        }

        [Fact]
        public void SetShippingAddress_Throws_WhenNotDraft()
        {
            var order = new Order(Guid.NewGuid());
            order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);
            order.FinalizeOrder();

            Action act = () => order.SetShippingAddress(new Address("St2", "C2", "Z2", "Country2"));
            act.Should().Throw<OrderDomainException>().WithMessage("Cannot change address after payment.");
        }

        [Fact]
        public void Cancel_WorksAndAddsEvent()
        {
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);

            order.Cancel("Customer request");

            order.Status.Should().Be(OrderStatus.Cancelled);
            order.CancellationReason.Should().Be("Customer request");
            order.DomainEvents.Should().ContainSingle(ev => ev.GetType() == typeof(OrderCancelledDomainEvent) && ((OrderCancelledDomainEvent)ev).Reason == "Customer request");
        }

        [Fact]
        public void Cancel_Throws_WhenShipped_And_IsIdempotent()
        {
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);
            order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
            order.FinalizeOrder();
            order.MarkAsPaid();
            order.MarkAsShipped("TRACK123");

            Action act = () => order.Cancel("Too late");
            act.Should().Throw<OrderDomainException>().WithMessage("Shipped orders cannot be cancelled.");

            // idempotent cancel when already cancelled
            var order2 = new Order(Guid.NewGuid());
            order2.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);
            order2.Cancel("Reason");
            // second cancel should do nothing (no exception)
            order2.Invoking(o => o.Cancel("Another")).Should().NotThrow();
        }

        [Fact]
        public void MarkAsPaid_And_MarkAsShipped_Behavior()
        {
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(2m, "USD"), 1);
            order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
            order.FinalizeOrder();

            order.MarkAsPaid();
            order.Status.Should().Be(OrderStatus.Paid);

            Action actShipInvalid = () => order.MarkAsShipped("");
            actShipInvalid.Should().Throw<OrderDomainException>().WithMessage("Tracking number is required.");

            order.MarkAsShipped("TN123");
            order.Status.Should().Be(OrderStatus.Shipped);
            order.TrackingNumber.Should().Be("TN123");
            order.ShippedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            order.DomainEvents.Should().ContainSingle(ev => ev.GetType() == typeof(OrderShippedDomainEvent) && ((OrderShippedDomainEvent)ev).TrackingNumber == "TN123");
        }

        [Fact]
        public void RemoveItem_RemovesAndAddsEvent_And_ThrowsWhenNotFoundOrNotDraft()
        {
            var order = new Order(Guid.NewGuid());
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var (item, created) = order.AddItem(productId, variantId, "P", new Money(3m, "USD"), 2);

            order.Items.Should().Contain(item);

            order.RemoveItem(item.Id);

            order.Items.Should().NotContain(item);
            order.DomainEvents.Should().ContainSingle(ev => ev.GetType() == typeof(OrderItemRemovedDomainEvent) && ((OrderItemRemovedDomainEvent)ev).ProductId == productId && ((OrderItemRemovedDomainEvent)ev).ProductVariantId == variantId);

            // removing non-existing
            Action act = () => order.RemoveItem(Guid.NewGuid());
            act.Should().Throw<OrderDomainException>().WithMessage("Item not found in the order.");

            // cannot remove when not draft
            order.AddItem(productId, variantId, "P", new Money(3m, "USD"), 1);
            order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
            order.FinalizeOrder();

            Action act2 = () => order.RemoveItem(order.Items.First().Id);
            act2.Should().Throw<OrderDomainException>().WithMessage("Cannot modify a finalized order.");
        }
    }
}
