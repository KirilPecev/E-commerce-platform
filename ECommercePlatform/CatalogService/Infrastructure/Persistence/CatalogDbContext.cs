using System.Reflection;

using CatalogService.Application.Interfaces;
using CatalogService.Domain.Aggregates;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Persistence
{
    public class CatalogDbContext : MessageDbContext, ICatalogDbContext
    {
        public CatalogDbContext(
            DbContextOptions<CatalogDbContext> options,
            IDomainEventDispatcher dispatcher) : base(options, dispatcher)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }

        protected override Assembly ConfigurationsAssembly => Assembly.GetExecutingAssembly();
    }
}