using MediatR;

using Microsoft.EntityFrameworkCore;

using OrderService.Domain.Aggregates;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Orders.Commands
{
    public class PayOrderCommandHandler
        (OrdersDbContext ordersDbContext) : IRequestHandler<PayOrderCommand>
    {
        public async Task Handle(PayOrderCommand request, CancellationToken cancellationToken)
        {
            Order? order = await ordersDbContext
                 .Orders
                 .Include(o => o.Items)
                 .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
            }

            order.MarkAsPaid();

            await ordersDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
