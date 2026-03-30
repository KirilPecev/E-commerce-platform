using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data.Models;

namespace ECommercePlatform.Data
{
    public class OutboxEventPublisher(MessageDbContext dbContext) : IEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            var message = new OutboxMessage(@event);

            dbContext.OutboxMessages.Add(message);

            return Task.CompletedTask;
        }
    }
}
