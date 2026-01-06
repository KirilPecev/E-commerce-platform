
using ECommercePlatform.Events.OrderIntegrationEvents;

using InventoryService.Application.Inventory.Commands;

using MassTransit;

using MediatR;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
    public class OrderCreatedIntegrationEventConsumer
        (IMediator mediator) : IConsumer<OrderCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
        {
            OrderCreatedIntegrationEvent message = context.Message;

            await mediator.Send(new ReserveStockCommand(message.OrderId, message.ProductId, message.ProductVariantId, message.Quantity));
        }
    }
}
