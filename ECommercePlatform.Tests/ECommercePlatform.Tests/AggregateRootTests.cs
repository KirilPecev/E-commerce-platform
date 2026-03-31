using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

namespace ECommercePlatform.Tests
{
    public class AggregateRootTests
    {
        [Fact]
        public void DomainEvents_Initially_ShouldBeEmpty()
        {
            var aggregate = new TestAggregate();

            aggregate.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void AddDomainEvent_ShouldAddEventToCollection()
        {
            var aggregate = new TestAggregate();

            aggregate.RaiseEvent();

            aggregate.DomainEvents.Should().ContainSingle();
        }

        [Fact]
        public void AddDomainEvent_MultipleTimes_ShouldAddAllEvents()
        {
            var aggregate = new TestAggregate();

            aggregate.RaiseEvent();
            aggregate.RaiseEvent();
            aggregate.RaiseEvent();

            aggregate.DomainEvents.Should().HaveCount(3);
        }

        [Fact]
        public void ClearDomainEvents_ShouldRemoveAllEvents()
        {
            var aggregate = new TestAggregate();
            aggregate.RaiseEvent();
            aggregate.RaiseEvent();

            aggregate.ClearDomainEvents();

            aggregate.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void ClearDomainEvents_WhenNoEvents_ShouldNotThrow()
        {
            var aggregate = new TestAggregate();

            Action act = () => aggregate.ClearDomainEvents();

            act.Should().NotThrow();
        }

        [Fact]
        public void DomainEvents_ShouldReturnReadOnlyCollection()
        {
            var aggregate = new TestAggregate();
            aggregate.RaiseEvent();

            aggregate.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
        }

        [Fact]
        public void AddDomainEvent_ShouldPreserveEventInstances()
        {
            var aggregate = new TestAggregate();

            aggregate.RaiseEvent();
            aggregate.RaiseEvent();

            aggregate.DomainEvents.Should().AllBeOfType<SampleDomainEvent>();
        }

        private class TestAggregate : AggregateRoot
        {
            public void RaiseEvent()
            {
                AddDomainEvent(new SampleDomainEvent());
            }
        }

        private class SampleDomainEvent : IDomainEvent
        {
            public DateTime OccurredOn => DateTime.UtcNow;
        }
    }
}
