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
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
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
                .Where(m => !m.Published)
                .OrderBy(m => m.Id)
                .Take(20)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    await sender.SendAsync(message.Data, message.Type, cancellationToken);

                    message.MarkAsPublished();

                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to publish outbox message {MessageId}.", message.Id);
                }
            }
        }
    }
}
