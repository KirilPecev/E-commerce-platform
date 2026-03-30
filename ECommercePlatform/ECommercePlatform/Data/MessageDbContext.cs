using System.Reflection;

using ECommercePlatform.Data.Configuration;

using ECommercePlatform.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Data
{
    public abstract class MessageDbContext : DbContext
    {
        protected MessageDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected abstract Assembly ConfigurationsAssembly { get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new OutboxMessageConfiguration());

            builder.ApplyConfigurationsFromAssembly(this.ConfigurationsAssembly);

            base.OnModelCreating(builder);
        }
    }
}
