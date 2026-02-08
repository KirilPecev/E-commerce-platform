
using ECommercePlatform.Events.OrderIntegrationEvents;
using ECommercePlatform.Events.PaymentIntegrationEvents;

using MassTransit;

using MediatR;

using PaymentService.Application.Payments.Command;

namespace PaymentService.Infrastructure.Messaging.Consumers
{
    public class OrderFinalizedIntegrationEventConsumer
         (IMediator mediator) : IConsumer<OrderFinalizedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<OrderFinalizedIntegrationEvent> context)
        {
            OrderFinalizedIntegrationEvent message = context.Message;

            await mediator.Send(new CreatePaymentCommand(message.OrderId, message.Amount, message.Currecy));
        }
    }
}
