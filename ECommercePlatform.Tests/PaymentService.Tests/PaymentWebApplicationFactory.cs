using MassTransit;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using PaymentService.Infrastructure.Messaging.Consumers;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Tests
{
    public class PaymentWebApplicationFactory
      : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public PaymentWebApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with SQLite in-memory
                services.RemoveAll<DbContextOptions<PaymentDbContext>>();

                services.AddDbContext<PaymentDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // Replace payment gateway with a fake
                services.RemoveAll<IPaymentGateway>();
                services.AddTransient<IPaymentGateway, FakePaymentGateway>();

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
                    x.AddConsumers(typeof(OrderFinalizedIntegrationEventConsumer).Assembly);
                });

                // Build provider to create schema
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                db.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Dispose();
        }
    }

    internal class FakePaymentGateway : IPaymentGateway
    {
        public Task<PaymentResult> ProcessCardPaymentAsync(
            decimal amount, string currency, CardDetails card, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentResult(true));
        }
    }
}
