using ECommercePlatform.Application.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Data
{
    public class OutboxMessageProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxMessageProcessor> logger) : BackgroundService
    {
        private const int MaxRetries = 5;
        private const int BatchSize = 20;

        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
        private static readonly TimeSpan PublishedMessageRetention = TimeSpan.FromDays(7);

        private DateTime lastCleanup = DateTime.MinValue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                    await CleanupPublishedMessagesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing outbox messages.");
                }

                await Task.Delay(PollingInterval, stoppingToken);
            }
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<MessageDbContext>();
            var sender = scope.ServiceProvider.GetRequiredService<IOutboxMessageSender>();

            var messages = await dbContext.OutboxMessages
                .Where(m => !m.Published && m.RetryCount < MaxRetries)
                .OrderBy(m => m.CreatedAt)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    await sender.SendAsync(message.Data, message.Type, cancellationToken);

                    message.MarkAsPublished();
                }
                catch (Exception ex)
                {
                    message.RecordFailure(ex.Message);

                    logger.LogError(ex, "Failed to publish outbox message {MessageId}. Retry {RetryCount}/{MaxRetries}.",
                        message.Id, message.RetryCount, MaxRetries);
                }
            }

            if (messages.Count > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task CleanupPublishedMessagesAsync(CancellationToken cancellationToken)
        {
            if (DateTime.UtcNow - lastCleanup < CleanupInterval)
            {
                return;
            }

            lastCleanup = DateTime.UtcNow;

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MessageDbContext>();

            var cutoff = DateTime.UtcNow - PublishedMessageRetention;

            var oldMessages = await dbContext.OutboxMessages
                .Where(m => m.Published && m.PublishedAt < cutoff)
                .ToListAsync(cancellationToken);

            if (oldMessages.Count > 0)
            {
                dbContext.OutboxMessages.RemoveRange(oldMessages);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Cleaned up {Count} published outbox messages.", oldMessages.Count);
            }
        }
    }
}
