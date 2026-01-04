using InventoryService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Infrastructure.Configurations
{
    public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
    {
        public void Configure(EntityTypeBuilder<StockReservation> builder)
        {
            builder.HasKey(sr => sr.Id);

            builder.Property(sr => sr.OrderId)
                .IsRequired();

            builder.Property(sr => sr.Quantity)
                .IsRequired();

            builder.Property(sr => sr.ReservedAt)
                .IsRequired();

            builder.Property(sr => sr.Status)
                .HasConversion<string>()
                .IsRequired();
        }
    }
}
