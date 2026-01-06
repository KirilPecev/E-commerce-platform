
using ECommercePlatform.Events.OrderIntegrationEvents;

using InventoryService.Application.Inventory.Commands;

using MassTransit;

using MediatR;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
    public class OrderPaidIntegrationEventConsumer
        (IMediator mediator) : IConsumer<OrderPaidIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<OrderPaidIntegrationEvent> context)
        {
            OrderPaidIntegrationEvent message = context.Message;

            await mediator.Send(new ConfirmStockCommand(message.OrderId));
        }
    }
}
