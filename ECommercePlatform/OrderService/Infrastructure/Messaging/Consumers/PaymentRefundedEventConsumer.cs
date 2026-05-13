using ECommercePlatform.Events.PaymentIntegrationEvents;

using MassTransit;

using MediatR;

using OrderService.Application.Orders.Commands;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class PaymentRefundedEventConsumer
        (IMediator mediator) : IConsumer<PaymentRefundedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<PaymentRefundedIntegrationEvent> context)
        {
            PaymentRefundedIntegrationEvent message = context.Message;

            await mediator.Send(new CancelOrderCommand(message.OrderId, "Order refunded"));
        }
    }
}
