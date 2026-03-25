using CarRentalSystem.Data.Configuration;

using ECommercePlatform.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Data
{
    public class MessageDbContext : DbContext
    {
        protected MessageDbContext(DbContextOptions<MessageDbContext> options)
            : base(options) { }

        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new OutboxMessageConfiguration());

            builder.ApplyConfigurationsFromAssembly(this.ConfigurationsAssembly);

            base.OnModelCreating(builder);
        }
    }
}
