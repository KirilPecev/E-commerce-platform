
using ECommercePlatform.Events.OrderIntegrationEvents;

using InventoryService.Application.Inventory.Commands;

using MassTransit;

using MediatR;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
    public class OrderItemRemovedIntegrationEventConsumer
        (IMediator mediator): IConsumer<OrderItemRemovedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<OrderItemRemovedIntegrationEvent> context)
        {
            OrderItemRemovedIntegrationEvent message = context.Message;

            await mediator.Send(new ReleaseStockCommand(message.ProductId, message.ProductVariantId, message.OrderId));
        }
    }
}
