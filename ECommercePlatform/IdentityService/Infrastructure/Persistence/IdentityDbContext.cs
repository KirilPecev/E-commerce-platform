using IdentityService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(IdentityDbContext).Assembly);
        }
    }
}
