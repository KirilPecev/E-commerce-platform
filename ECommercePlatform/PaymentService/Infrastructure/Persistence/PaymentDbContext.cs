using System.Reflection;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;

using Microsoft.EntityFrameworkCore;

using PaymentService.Application.Interfaces;
using PaymentService.Domain.Aggregates;

namespace PaymentService.Infrastructure.Persistence
{
    public class PaymentDbContext : MessageDbContext, IPaymentDbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options, IDomainEventDispatcher dispatcher)
            : base(options, dispatcher)
        {
        }

        public DbSet<Payment> Payments { get; set; } = default!;

        protected override Assembly ConfigurationsAssembly => Assembly.GetExecutingAssembly();
    }
}
