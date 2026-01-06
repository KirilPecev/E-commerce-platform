
using ECommercePlatform.Events.OrderIntegrationEvents;

using InventoryService.Application.Inventory.Commands;

using MassTransit;

using MediatR;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
    public class OrderCancelledIntegrationEventConsumer
        (IMediator mediator) : IConsumer<OrderCancelledIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
        {
            OrderCancelledIntegrationEvent message = context.Message;

            await mediator.Send(new ReleaseStocksForOrderCommand(message.OrderId, message.Reason));
        }
    }
}
