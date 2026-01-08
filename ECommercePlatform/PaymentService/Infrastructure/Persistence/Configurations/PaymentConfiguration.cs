using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PaymentService.Domain.Aggregates;

namespace PaymentService.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.OrderId)
                .IsRequired();

            builder.OwnsOne(p => p.Amount, up =>
            {
                up.Property(u => u.Amount)
                    .HasColumnName("Amount")
                    .IsRequired();

                up.Property(u => u.Currency)
                    .HasColumnName("Currency")
                    .IsRequired()
                    .HasMaxLength(3);
            });

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.PaymentMethod)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.ProcessedAt)
                .IsRequired();
        }
    }
}
