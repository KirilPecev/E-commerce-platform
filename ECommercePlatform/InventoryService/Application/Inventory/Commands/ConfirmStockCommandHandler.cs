
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Inventory.Commands
{
    public class ConfirmStockCommandHandler
        (InventoryDbContext inventoryDbContext) : IRequestHandler<ConfirmStockCommand>
    {
        public async Task Handle(ConfirmStockCommand request, CancellationToken cancellationToken)
        {
            List<StockReservation> stockReservations = await inventoryDbContext
                 .StockReservations
                 .Where(sr => sr.OrderId == request.OrderId && sr.Status == ReservationStatus.Pending)
                 .ToListAsync(cancellationToken);

            foreach (StockReservation stockReservation in stockReservations)
            {
                stockReservation.Confirm();
            }

            await inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
