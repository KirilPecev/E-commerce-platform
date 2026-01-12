using ECommercePlatform.Events.PaymentIntegrationEvents;

using MassTransit;

using MediatR;

using OrderService.Application.Orders.Commands;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class PaymentCompletedEventConsumer
        (IMediator mediator) : IConsumer<PaymentCompletedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<PaymentCompletedIntegrationEvent> context)
        {
            PaymentCompletedIntegrationEvent message = context.Message;

            await mediator.Send(new PayOrderCommand(message.OrderId));
        }
    }
}
