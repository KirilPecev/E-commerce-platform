using CatalogService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.OwnsOne(p => p.Name, n =>
            {
                n.Property(p => p.Value)
                 .HasColumnName("Name")
                 .IsRequired();
            });

            builder.OwnsOne(p => p.Price, p =>
            {
                p.Property(x => x.Amount)
                 .HasColumnName("Amount");

                p.Property(x => x.Currency)
                 .HasColumnName("Currency");
            });

            builder.Property(p => p.Status)
                   .HasConversion<string>();

            builder.Property(p => p.Description);

            builder.Property(p => p.CreatedAt)
                   .IsRequired();

            builder.HasMany(p => p.Variants)
                   .WithOne(v => v.Product  )
                   .HasForeignKey("ProductId")
                   .IsRequired();
        }
    }
}
