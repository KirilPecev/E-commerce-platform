using MediatR;

using Microsoft.EntityFrameworkCore;

using OrderService.Application.Interfaces;
using OrderService.Domain.Aggregates;

namespace OrderService.Application.Orders.Commands
{
    public class FinalizeOrderCommandHandler
        (IOrdersDbContext ordersDbContext) : IRequestHandler<FinalizeOrderCommand>
    {
        public async Task Handle(FinalizeOrderCommand request, CancellationToken cancellationToken)
        {
            Order? order = await ordersDbContext
                 .Orders
                 .Include(o => o.Items)
                 .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
            }

            order.FinalizeOrder();

            await ordersDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
