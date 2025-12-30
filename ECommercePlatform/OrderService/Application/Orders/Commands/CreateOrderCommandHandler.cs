
using MediatR;

using OrderService.Domain.Aggregates;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Orders.Commands
{
    public class CreateOrderCommandHandler
        (OrdersDbContext ordersDbContext) : IRequestHandler<CreateOrderCommand, Guid>
    {
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            Order order = new Order(request.CustomerId);

            await ordersDbContext.Orders.AddAsync(order, cancellationToken);

            await ordersDbContext.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
