using MassTransit;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests
{
    public class OrderWebApplicationFactory
      : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public OrderWebApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with SQLite in-memory
                services.RemoveAll<DbContextOptions<OrdersDbContext>>();

                services.AddDbContext<OrdersDbContext>(options =>
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

                services.AddMassTransitTestHarness();

                // Build provider to create schema
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

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
