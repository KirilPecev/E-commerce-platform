using InventoryService.Infrastructure.Messaging.Consumers;
using InventoryService.Infrastructure.Persistence;

using MassTransit;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InventoryService.Tests
{
    public class InventoryWebApplicationFactory
      : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public InventoryWebApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with SQLite in-memory
                services.RemoveAll<DbContextOptions<InventoryDbContext>>();

                services.AddDbContext<InventoryDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

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

                services.Configure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                });

                services.AddMassTransitTestHarness(x =>
                {
                    x.AddConsumers(typeof(ProductCreatedIntegrationEventConsumer).Assembly);
                    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("inventory", false));
                });

                // Build provider to create schema
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

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
