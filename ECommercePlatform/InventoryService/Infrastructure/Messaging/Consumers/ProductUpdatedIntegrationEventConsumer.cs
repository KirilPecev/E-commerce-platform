
using ECommercePlatform.Events.ProductIntegrationEvents;

using InventoryService.Application.Inventory.Commands;

using MassTransit;

using MediatR;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
    public class ProductUpdatedIntegrationEventConsumer
        (IMediator mediator) : IConsumer<ProductUpdatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductUpdatedIntegrationEvent> context)
        {
            ProductUpdatedIntegrationEvent message = context.Message;

            await mediator.Send(new UpdateProductStockCommand(message.ProductId, message.ProductVariantId, message.Quantity));
        }
    }
}
