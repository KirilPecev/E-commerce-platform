using Microsoft.EntityFrameworkCore;

using PaymentService.Domain.Aggregates;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentDbContext
    {
        DbSet<Payment> Payments { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
