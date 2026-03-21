using MediatR;

using Microsoft.EntityFrameworkCore;

using OrderService.Application.Interfaces;
using OrderService.Domain.Aggregates;

namespace OrderService.Application.Orders.Commands
{
    public class CancelOrderCommandHandler
        (IOrdersDbContext ordersDbContext) : IRequestHandler<CancelOrderCommand>
    {
        public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            Order? order = await ordersDbContext
                .Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");

            order.Cancel(request.Reason);

            await ordersDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
