using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;
using ECommercePlatform.Data.Models;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace ECommercePlatform.Tests
{
    public class OutboxMessageProcessorTests : IDisposable
    {
        private readonly TestDbContext dbContext;
        private readonly Mock<IOutboxMessageSender> senderMock;
        private readonly OutboxMessageProcessor processor;

        public OutboxMessageProcessorTests()
        {
            this.dbContext = TestDbContext.Create();
            this.senderMock = new Mock<IOutboxMessageSender>();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(MessageDbContext)))
                .Returns(this.dbContext);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOutboxMessageSender)))
                .Returns(this.senderMock.Object);

            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

            ILogger<OutboxMessageProcessor> logger = NullLogger<OutboxMessageProcessor>.Instance;

            this.processor = new OutboxMessageProcessor(scopeFactory.Object, logger);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldStopWhenCancelled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> act = () => this.processor.StartAsync(cts.Token);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ProcessOutboxMessages_ShouldSendUnpublishedMessages()
        {
            var testEvent = new TestEvent("test-value");
            var message = new OutboxMessage(testEvent);
            this.dbContext.OutboxMessages.Add(message);
            await this.dbContext.SaveChangesAsync();

            using var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                cts.Cancel();
            });

            try { await this.processor.StartAsync(cts.Token); } catch (OperationCanceledException) { }
            await Task.Delay(200);

            this.senderMock.Verify(s => s.SendAsync(
                It.IsAny<object>(),
                typeof(TestEvent),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessOutboxMessages_ShouldMarkMessageAsPublishedAfterSend()
        {
            var testEvent = new TestEvent("test-value");
            var message = new OutboxMessage(testEvent);
            this.dbContext.OutboxMessages.Add(message);
            await this.dbContext.SaveChangesAsync();

            using var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                cts.Cancel();
            });

            try { await this.processor.StartAsync(cts.Token); } catch (OperationCanceledException) { }
            await Task.Delay(200);

            var updatedMessage = this.dbContext.OutboxMessages.First();
            updatedMessage.Published.Should().BeTrue();
            updatedMessage.PublishedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ProcessOutboxMessages_ShouldNotSendAlreadyPublishedMessages()
        {
            var testEvent = new TestEvent("test-value");
            var message = new OutboxMessage(testEvent);
            message.MarkAsPublished();
            this.dbContext.OutboxMessages.Add(message);
            await this.dbContext.SaveChangesAsync();

            using var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                cts.Cancel();
            });

            try { await this.processor.StartAsync(cts.Token); } catch (OperationCanceledException) { }
            await Task.Delay(200);

            this.senderMock.Verify(s => s.SendAsync(
                It.IsAny<object>(),
                It.IsAny<Type>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessOutboxMessages_WhenSenderFails_ShouldNotMarkMessageAsPublished()
        {
            this.senderMock
                .Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Send failed"));

            var testEvent = new TestEvent("test-value");
            var message = new OutboxMessage(testEvent);
            this.dbContext.OutboxMessages.Add(message);
            await this.dbContext.SaveChangesAsync();

            using var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                cts.Cancel();
            });

            try { await this.processor.StartAsync(cts.Token); } catch (OperationCanceledException) { }
            await Task.Delay(200);

            var updatedMessage = this.dbContext.OutboxMessages.First();
            updatedMessage.Published.Should().BeFalse();
            updatedMessage.PublishedAt.Should().BeNull();
        }

        [Fact]
        public async Task ProcessOutboxMessages_ShouldProcessMultipleMessages()
        {
            for (int i = 0; i < 5; i++)
            {
                this.dbContext.OutboxMessages.Add(new OutboxMessage(new TestEvent($"value-{i}")));
            }
            await this.dbContext.SaveChangesAsync();

            using var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                cts.Cancel();
            });

            try { await this.processor.StartAsync(cts.Token); } catch (OperationCanceledException) { }
            await Task.Delay(200);

            this.senderMock.Verify(s => s.SendAsync(
                It.IsAny<object>(),
                typeof(TestEvent),
                It.IsAny<CancellationToken>()), Times.Exactly(5));
        }

        [Fact]
        public async Task ProcessOutboxMessages_WhenOneFails_ShouldContinueProcessingOthers()
        {
            int callCount = 0;
            this.senderMock
                .Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<CancellationToken>()))
                .Returns<object, Type, CancellationToken>((_, _, _) =>
                {
                    callCount++;
                    if (callCount == 2)
                        throw new InvalidOperationException("Send failed");
                    return Task.CompletedTask;
                });

            for (int i = 0; i < 3; i++)
            {
                this.dbContext.OutboxMessages.Add(new OutboxMessage(new TestEvent($"value-{i}")));
            }
            await this.dbContext.SaveChangesAsync();

            using var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                cts.Cancel();
            });

            try { await this.processor.StartAsync(cts.Token); } catch (OperationCanceledException) { }
            await Task.Delay(200);

            this.senderMock.Verify(s => s.SendAsync(
                It.IsAny<object>(),
                It.IsAny<Type>(),
                It.IsAny<CancellationToken>()), Times.Exactly(3));

            this.dbContext.OutboxMessages.Count(m => m.Published).Should().Be(2);
        }

        [Fact]
        public async Task ProcessOutboxMessages_WithNoMessages_ShouldNotCallSender()
        {
            using var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                cts.Cancel();
            });

            try { await this.processor.StartAsync(cts.Token); } catch (OperationCanceledException) { }
            await Task.Delay(200);

            this.senderMock.Verify(s => s.SendAsync(
                It.IsAny<object>(),
                It.IsAny<Type>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        public void Dispose()
        {
            this.dbContext.Dispose();
        }

        public record TestEvent(string Value);
    }
}
