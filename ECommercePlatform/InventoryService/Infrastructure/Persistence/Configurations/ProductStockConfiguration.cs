using InventoryService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Infrastructure.Persistence.Configurations
{
    public class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
    {
        public void Configure(EntityTypeBuilder<ProductStock> builder)
        {
            builder.HasKey(ps => ps.Id);

            builder.Property(ps => ps.ProductId)
                .IsRequired();

            builder.Property(ps => ps.ProductVariantId)
                .IsRequired();

            builder.Property(ps => ps.AvailableQuantity)
                .IsRequired();

            builder.HasMany(ps => ps.Reservations)
                .WithOne()
                .HasForeignKey("ProductStockId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
