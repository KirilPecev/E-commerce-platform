
using ECommercePlatform.Events.OrderIntegrationEvents;

using InventoryService.Application.Inventory.Commands;

using MassTransit;

using MediatR;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
    public class OrderFinalizedIntegrationEventConsumer
        (IMediator mediator) : IConsumer<OrderFinalizedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<OrderFinalizedIntegrationEvent> context)
        {
            OrderFinalizedIntegrationEvent message = context.Message;

            await mediator.Send(new ConfirmStockCommand(message.OrderId));
        }
    }
}
