using ECommercePlatform.Data;
using ECommercePlatform.Data.Models;

using FluentAssertions;

namespace ECommercePlatform.Tests
{
    public class OutboxEventPublisherTests
    {
        [Fact]
        public async Task PublishAsync_ShouldAddOutboxMessageToDbContext()
        {
            using var dbContext = TestDbContext.Create();
            var publisher = new OutboxEventPublisher(dbContext);
            var testEvent = new TestIntegrationEvent(Guid.NewGuid(), "TestProduct");

            await publisher.PublishAsync(testEvent);

            dbContext.OutboxMessages.Local.Should().ContainSingle();
        }

        [Fact]
        public async Task PublishAsync_ShouldSetCorrectTypeOnOutboxMessage()
        {
            using var dbContext = TestDbContext.Create();
            var publisher = new OutboxEventPublisher(dbContext);
            var testEvent = new TestIntegrationEvent(Guid.NewGuid(), "TestProduct");

            await publisher.PublishAsync(testEvent);

            OutboxMessage message = dbContext.OutboxMessages.Local.Single();
            message.Type.Should().Be(typeof(TestIntegrationEvent));
        }

        [Fact]
        public async Task PublishAsync_ShouldSerializeEventData()
        {
            using var dbContext = TestDbContext.Create();
            var publisher = new OutboxEventPublisher(dbContext);
            var productId = Guid.NewGuid();
            var testEvent = new TestIntegrationEvent(productId, "TestProduct");

            await publisher.PublishAsync(testEvent);

            OutboxMessage message = dbContext.OutboxMessages.Local.Single();
            var deserialized = message.Data as TestIntegrationEvent;
            deserialized.Should().NotBeNull();
            deserialized!.ProductId.Should().Be(productId);
            deserialized.Name.Should().Be("TestProduct");
        }

        [Fact]
        public async Task PublishAsync_ShouldNotMarkMessageAsPublished()
        {
            using var dbContext = TestDbContext.Create();
            var publisher = new OutboxEventPublisher(dbContext);
            var testEvent = new TestIntegrationEvent(Guid.NewGuid(), "TestProduct");

            await publisher.PublishAsync(testEvent);

            OutboxMessage message = dbContext.OutboxMessages.Local.Single();
            message.Published.Should().BeFalse();
        }

        [Fact]
        public async Task PublishAsync_MultipleCalls_ShouldAddMultipleMessages()
        {
            using var dbContext = TestDbContext.Create();
            var publisher = new OutboxEventPublisher(dbContext);

            await publisher.PublishAsync(new TestIntegrationEvent(Guid.NewGuid(), "Product1"));
            await publisher.PublishAsync(new TestIntegrationEvent(Guid.NewGuid(), "Product2"));
            await publisher.PublishAsync(new TestIntegrationEvent(Guid.NewGuid(), "Product3"));

            dbContext.OutboxMessages.Local.Should().HaveCount(3);
        }

        [Fact]
        public async Task PublishAsync_ShouldNotSaveChanges()
        {
            using var dbContext = TestDbContext.Create();
            var publisher = new OutboxEventPublisher(dbContext);
            var testEvent = new TestIntegrationEvent(Guid.NewGuid(), "TestProduct");

            await publisher.PublishAsync(testEvent);

            // The message should only be in the change tracker, not saved to the database
            dbContext.OutboxMessages.Local.Should().ContainSingle();
            dbContext.OutboxMessages.Count().Should().Be(0);
        }

        public record TestIntegrationEvent(Guid ProductId, string Name);
    }
}
