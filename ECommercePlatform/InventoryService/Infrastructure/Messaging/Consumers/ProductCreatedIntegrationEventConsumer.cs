
using ECommercePlatform.Events.ProductIntegrationEvents;

using InventoryService.Application.Inventory.Commands;

using MassTransit;

using MediatR;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
    public class ProductCreatedIntegrationEventConsumer
        (IMediator mediator) : IConsumer<ProductCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
        {
            var message = context.Message;

            await mediator.Send(new CreateProductStockCommand(message.ProductId, message.ProductVariantId, message.InitialQuantity));
        }
    }
}
