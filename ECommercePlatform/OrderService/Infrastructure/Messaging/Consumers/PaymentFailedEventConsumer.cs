using ECommercePlatform.Events.PaymentIntegrationEvents;

using MassTransit;

using MediatR;

using OrderService.Application.Orders.Commands;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class PaymentFailedEventConsumer
        (IMediator mediator) : IConsumer<PaymentFailedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<PaymentFailedIntegrationEvent> context)
        {
            PaymentFailedIntegrationEvent message = context.Message;

            await mediator.Send(new CancelOrderCommand(message.OrderId, message.FailureReason ?? "Failed to process payment"));
        }
    }
}
