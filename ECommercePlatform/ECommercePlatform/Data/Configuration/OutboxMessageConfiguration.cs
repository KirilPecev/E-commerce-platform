using ECommercePlatform.Data.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Data.Configuration
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder
                .HasKey(m => m.Id);

            builder
                .Property<string>("serializedData")
                .IsRequired()
                .HasField("serializedData")
                .HasColumnName("SerializedData");

            builder
                .Property(m => m.Type)
                .IsRequired()
                .HasConversion(
                    t => t.AssemblyQualifiedName,
                    t => Type.GetType(t));

            builder
                .Property(m => m.CreatedAt)
                .IsRequired();

            builder
                .Property(m => m.PublishedAt)
                .IsRequired(false);

            builder
                .HasIndex(m => new { m.Published, m.CreatedAt })
                .HasDatabaseName("IX_OutboxMessages_Published_CreatedAt");
        }
    }
}
