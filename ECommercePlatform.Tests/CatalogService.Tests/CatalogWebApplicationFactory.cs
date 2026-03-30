using CatalogService.Application.Interfaces;
using CatalogService.Infrastructure.Persistence;

using ECommercePlatform.Data;

using MassTransit;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CatalogService.Tests
{
    public class CatalogWebApplicationFactory
      : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public CatalogWebApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with SQLite in-memory
                services.RemoveAll<DbContextOptions<CatalogDbContext>>();

                services.AddDbContext<CatalogDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // Replace Redis with in-memory distributed cache
                services.RemoveAll<IDistributedCache>();
                services.AddDistributedMemoryCache();

                // Remove all existing MassTransit registrations before adding test harness
                var massTransitDescriptors = services
                    .Where(d => d.ServiceType.FullName?.StartsWith("MassTransit") == true
                             || d.ServiceType.FullName?.StartsWith("Microsoft.Extensions.Hosting") == true
                                && d.ImplementationType?.FullName?.StartsWith("MassTransit") == true)
                    .ToList();

                foreach (var descriptor in massTransitDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Remove outbox background processor to avoid concurrent SQLite access in tests
                var outboxDescriptor = services.FirstOrDefault(d =>
                    d.ImplementationType == typeof(OutboxMessageProcessor));
                if (outboxDescriptor != null)
                    services.Remove(outboxDescriptor);

                services.Configure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                });

                services.AddMassTransitTestHarness();

                // Build provider to create schema
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

                db.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Dispose();
        }
    }
}
