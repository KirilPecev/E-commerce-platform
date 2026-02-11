
using MediatR;

using Microsoft.EntityFrameworkCore;

using OrderService.Domain.Aggregates;
using OrderService.Domain.ValueObjects;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Orders.Commands
{
    public class AddItemCommandHandler
        (OrdersDbContext ordersDbContext) : IRequestHandler<AddItemCommand>
    {
        public async Task Handle(AddItemCommand request, CancellationToken cancellationToken)
        {
            Order? order = await ordersDbContext
                .Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");

            (OrderItem item, bool isCreated) = order.AddItem(
                request.ProductId,
                request.ProductVariantId,
                request.ProductName,
                new Money(request.Price, request.Currency),
                request.Quantity);

            if (isCreated)
            {
                ordersDbContext.Items.Add(item);
            }

            await ordersDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
