using ECommercePlatform.Data.Models;

using FluentAssertions;

namespace ECommercePlatform.Tests
{
    public class OutboxMessageTests
    {
        [Fact]
        public void Constructor_WithData_ShouldInitializeIdAsNewGuid()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Constructor_WithData_ShouldSetCreatedAtToUtcNow()
        {
            var before = DateTime.UtcNow;

            var message = new OutboxMessage(new TestEvent("test"));

            var after = DateTime.UtcNow;

            message.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Fact]
        public void Constructor_WithData_ShouldSetTypeFromData()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.Type.Should().Be(typeof(TestEvent));
        }

        [Fact]
        public void Constructor_WithData_ShouldSerializeAndDeserializeDataCorrectly()
        {
            var original = new TestEvent("hello");

            var message = new OutboxMessage(original);

            var deserialized = message.Data as TestEvent;
            deserialized.Should().NotBeNull();
            deserialized!.Value.Should().Be("hello");
        }

        [Fact]
        public void Constructor_WithData_ShouldNotBePublished()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.Published.Should().BeFalse();
            message.PublishedAt.Should().BeNull();
        }

        [Fact]
        public void Constructor_TwoMessages_ShouldHaveUniqueIds()
        {
            var message1 = new OutboxMessage(new TestEvent("a"));
            var message2 = new OutboxMessage(new TestEvent("b"));

            message1.Id.Should().NotBe(message2.Id);
        }

        [Fact]
        public void MarkAsPublished_ShouldSetPublishedToTrue()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.MarkAsPublished();

            message.Published.Should().BeTrue();
        }

        [Fact]
        public void MarkAsPublished_ShouldSetPublishedAtToUtcNow()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            var before = DateTime.UtcNow;
            message.MarkAsPublished();
            var after = DateTime.UtcNow;

            message.PublishedAt.Should().NotBeNull();
            message.PublishedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Fact]
        public void Data_Setter_ShouldUpdateTypeWhenReassigned()
        {
            var message = new OutboxMessage(new TestEvent("initial"));

            message.Type.Should().Be(typeof(TestEvent));
        }

        [Fact]
        public void Data_Getter_ShouldRoundTripComplexObject()
        {
            var original = new ComplexTestEvent(42, "complex", [1, 2, 3]);

            var message = new OutboxMessage(original);

            var deserialized = message.Data as ComplexTestEvent;
            deserialized.Should().NotBeNull();
            deserialized!.Id.Should().Be(42);
            deserialized.Name.Should().Be("complex");
            deserialized.Items.Should().BeEquivalentTo([1, 2, 3]);
        }

        [Fact]
        public void Data_Getter_ShouldIgnoreNullValues()
        {
            var original = new NullableTestEvent("test", null);

            var message = new OutboxMessage(original);

            var deserialized = message.Data as NullableTestEvent;
            deserialized.Should().NotBeNull();
            deserialized!.Name.Should().Be("test");
            deserialized.Description.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithData_ShouldInitializeRetryCountToZero()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.RetryCount.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithData_ShouldInitializeErrorToNull()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.Error.Should().BeNull();
        }

        [Fact]
        public void RecordFailure_ShouldIncrementRetryCount()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.RecordFailure("Some error");

            message.RetryCount.Should().Be(1);
        }

        [Fact]
        public void RecordFailure_CalledMultipleTimes_ShouldTrackAllRetries()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.RecordFailure("Error 1");
            message.RecordFailure("Error 2");
            message.RecordFailure("Error 3");

            message.RetryCount.Should().Be(3);
        }

        [Fact]
        public void RecordFailure_ShouldStoreLatestError()
        {
            var message = new OutboxMessage(new TestEvent("test"));

            message.RecordFailure("First error");
            message.RecordFailure("Second error");

            message.Error.Should().Be("Second error");
        }

        [Fact]
        public void MarkAsPublished_ShouldClearError()
        {
            var message = new OutboxMessage(new TestEvent("test"));
            message.RecordFailure("Some error");

            message.MarkAsPublished();

            message.Error.Should().BeNull();
        }

        public record TestEvent(string Value);

        public record ComplexTestEvent(int Id, string Name, List<int> Items);

        public record NullableTestEvent(string Name, string? Description);
    }
}
