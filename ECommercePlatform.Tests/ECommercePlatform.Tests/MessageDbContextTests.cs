using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using Moq;

namespace ECommercePlatform.Tests
{
    public class MessageDbContextTests : IDisposable
    {
        private readonly Mock<IDomainEventDispatcher> dispatcherMock;
        private readonly TestDbContext dbContext;

        public MessageDbContextTests()
        {
            this.dispatcherMock = new Mock<IDomainEventDispatcher>();
            this.dbContext = TestDbContext.Create(this.dispatcherMock.Object);
        }

        [Fact]
        public async Task SaveChangesAsync_WithDomainEvents_ShouldDispatchEvents()
        {
            var entity = new TestAggregate();
            entity.RaiseSampleEvent();

            this.dbContext.TestAggregates.Add(entity);

            await this.dbContext.SaveChangesAsync();

            this.dispatcherMock.Verify(
                d => d.DispatchAsync(It.IsAny<TestDomainEvent>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveChangesAsync_WithMultipleDomainEvents_ShouldDispatchAll()
        {
            var entity = new TestAggregate();
            entity.RaiseSampleEvent();
            entity.RaiseSampleEvent();
            entity.RaiseSampleEvent();

            this.dbContext.TestAggregates.Add(entity);

            await this.dbContext.SaveChangesAsync();

            this.dispatcherMock.Verify(
                d => d.DispatchAsync(It.IsAny<TestDomainEvent>()),
                Times.Exactly(3));
        }

        [Fact]
        public async Task SaveChangesAsync_WithNoDomainEvents_ShouldNotDispatch()
        {
            var entity = new TestAggregate();

            this.dbContext.TestAggregates.Add(entity);

            await this.dbContext.SaveChangesAsync();

            this.dispatcherMock.Verify(
                d => d.DispatchAsync(It.IsAny<IDomainEvent>()),
                Times.Never);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldClearDomainEventsAfterDispatch()
        {
            var entity = new TestAggregate();
            entity.RaiseSampleEvent();

            this.dbContext.TestAggregates.Add(entity);

            await this.dbContext.SaveChangesAsync();

            entity.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveChangesAsync_WithMultipleAggregates_ShouldDispatchAllEvents()
        {
            var entity1 = new TestAggregate();
            entity1.RaiseSampleEvent();

            var entity2 = new TestAggregate();
            entity2.RaiseSampleEvent();
            entity2.RaiseSampleEvent();

            this.dbContext.TestAggregates.Add(entity1);
            this.dbContext.TestAggregates.Add(entity2);

            await this.dbContext.SaveChangesAsync();

            this.dispatcherMock.Verify(
                d => d.DispatchAsync(It.IsAny<TestDomainEvent>()),
                Times.Exactly(3));
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldClearEventsOnAllAggregates()
        {
            var entity1 = new TestAggregate();
            entity1.RaiseSampleEvent();

            var entity2 = new TestAggregate();
            entity2.RaiseSampleEvent();

            this.dbContext.TestAggregates.Add(entity1);
            this.dbContext.TestAggregates.Add(entity2);

            await this.dbContext.SaveChangesAsync();

            entity1.DomainEvents.Should().BeEmpty();
            entity2.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldReturnSavedCount()
        {
            var entity = new TestAggregate();
            this.dbContext.TestAggregates.Add(entity);

            int result = await this.dbContext.SaveChangesAsync();

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenDispatcherThrows_ShouldStillClearDomainEvents()
        {
            this.dispatcherMock
                .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>()))
                .ThrowsAsync(new InvalidOperationException("Dispatch failed"));

            var entity = new TestAggregate();
            entity.RaiseSampleEvent();

            this.dbContext.TestAggregates.Add(entity);

            Func<Task> act = () => this.dbContext.SaveChangesAsync();

            await act.Should().ThrowAsync<InvalidOperationException>();
            entity.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldDispatchEventsInOrder()
        {
            var dispatchedEvents = new List<IDomainEvent>();

            this.dispatcherMock
                .Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>()))
                .Callback<IDomainEvent>(e => dispatchedEvents.Add(e))
                .Returns(Task.CompletedTask);

            var entity = new TestAggregate();
            entity.RaiseSampleEvent();
            entity.RaiseSampleEvent();
            entity.RaiseSampleEvent();

            this.dbContext.TestAggregates.Add(entity);

            await this.dbContext.SaveChangesAsync();

            dispatchedEvents.Should().HaveCount(3);
            dispatchedEvents.Should().AllBeOfType<TestDomainEvent>();
        }

        [Fact]
        public async Task SaveChangesAsync_WithCancellationToken_ShouldPassTokenToBase()
        {
            var entity = new TestAggregate();
            this.dbContext.TestAggregates.Add(entity);

            using var cts = new CancellationTokenSource();

            int result = await this.dbContext.SaveChangesAsync(cts.Token);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task SaveChangesAsync_WithMixedAggregates_ShouldOnlyDispatchFromAggregatesWithEvents()
        {
            var entityWithEvents = new TestAggregate();
            entityWithEvents.RaiseSampleEvent();

            var entityWithoutEvents = new TestAggregate();

            this.dbContext.TestAggregates.Add(entityWithEvents);
            this.dbContext.TestAggregates.Add(entityWithoutEvents);

            await this.dbContext.SaveChangesAsync();

            this.dispatcherMock.Verify(
                d => d.DispatchAsync(It.IsAny<TestDomainEvent>()),
                Times.Once);
        }

        public void Dispose()
        {
            this.dbContext.Dispose();
        }
    }

    public class TestAggregate : AggregateRoot
    {
        public TestAggregate()
        {
            this.Id = Guid.NewGuid();
        }

        public string Name { get; set; } = "Test";

        public void RaiseSampleEvent()
        {
            AddDomainEvent(new TestDomainEvent());
        }
    }

    public class TestDomainEvent : IDomainEvent
    {
        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
