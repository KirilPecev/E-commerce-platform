using CatalogService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class ProductVariantsConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Sku)
                   .IsRequired();

            builder.OwnsOne(p => p.Price, p =>
            {
                p.Property(x => x.Amount)
                 .HasColumnName("Amount");

                p.Property(x => x.Currency)
                 .HasColumnName("Currency");
            });

            builder.Property(p => p.Size);

            builder.Property(p => p.Color);

            builder.Property(p => p.StockQuantity)
                .IsRequired();

            builder.HasOne<Product>()
                   .WithMany(p => p.Variants)
                   .HasForeignKey(p => p.ProductId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
