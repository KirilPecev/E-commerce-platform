using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using PaymentService.Domain.Aggregates;
using PaymentService.Domain.ValueObjects;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Tests.IntegrationTests
{
    public class PaymentTests : IClassFixture<PaymentWebApplicationFactory>
    {
        [Fact]
        public async Task PayWithCard_ShouldReturnAccepted_WhenPaymentExists()
        {
            // Arrange
            var factory = new PaymentWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var paymentId = Guid.NewGuid();

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                var payment = new Payment(Guid.NewGuid(), new Money(100.00m, "USD"));

                // Override the auto-generated Id with our known Id
                var idProp = typeof(Payment).BaseType!.GetProperty("Id")!;
                idProp.SetValue(payment, paymentId);

                db.Payments.Add(payment);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", PaymentTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.PostAsJsonAsync("/api/payment/pay", new
            {
                PaymentId = paymentId,
                CardNumber = "4111111111111111",
                CardHolder = "Test User",
                Expiry = "12/30",
                Cvv = "123"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task PayWithCard_ShouldReturnAccepted_WhenAdmin()
        {
            // Arrange
            var factory = new PaymentWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var paymentId = Guid.NewGuid();

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                var payment = new Payment(Guid.NewGuid(), new Money(50.00m, "EUR"));

                var idProp = typeof(Payment).BaseType!.GetProperty("Id")!;
                idProp.SetValue(payment, paymentId);

                db.Payments.Add(payment);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", PaymentTestTokenGenerator.GenerateAdminToken());

            // Act
            var response = await client.PostAsJsonAsync("/api/payment/pay", new
            {
                PaymentId = paymentId,
                CardNumber = "4111111111111111",
                CardHolder = "Admin User",
                Expiry = "12/30",
                Cvv = "456"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task PayWithCard_ShouldReturnUnauthorized_WhenNoToken()
        {
            // Arrange
            var factory = new PaymentWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/payment/pay", new
            {
                PaymentId = Guid.NewGuid(),
                CardNumber = "4111111111111111",
                CardHolder = "Test User",
                Expiry = "12/30",
                Cvv = "123"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PayWithCard_ShouldReturnNotFound_WhenPaymentNotFound()
        {
            // Arrange
            var factory = new PaymentWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", PaymentTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.PostAsJsonAsync("/api/payment/pay", new
            {
                PaymentId = Guid.NewGuid(),
                CardNumber = "4111111111111111",
                CardHolder = "Test User",
                Expiry = "12/30",
                Cvv = "123"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task PayWithCard_ShouldReturnBadRequest_WhenPaymentAlreadyPaid()
        {
            // Arrange
            var factory = new PaymentWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var paymentId = Guid.NewGuid();

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                var payment = new Payment(Guid.NewGuid(), new Money(75.00m, "USD"));

                var idProp = typeof(Payment).BaseType!.GetProperty("Id")!;
                idProp.SetValue(payment, paymentId);

                payment.MarkAsPaid(PaymentMethod.Card);

                db.Payments.Add(payment);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", PaymentTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.PostAsJsonAsync("/api/payment/pay", new
            {
                PaymentId = paymentId,
                CardNumber = "4111111111111111",
                CardHolder = "Test User",
                Expiry = "12/30",
                Cvv = "123"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        }
    }
}
