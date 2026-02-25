using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using IdentityService.Application.Exceptions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;

namespace IdentityService.Tests.IntegrationTests
{
    public class RegisterTests : IClassFixture<IdentityWebApplicationFactory>
    {

        [Fact]
        public async Task Register_ShouldThrow_WhenInvalidEmail()
        {
            var factory = new IdentityWebApplicationFactory()
                 .WithWebHostBuilder(b =>
                 {
                     b.UseEnvironment("Testing");
                 });

            var client = factory.CreateClient();

            var request = new RegisterRequest()
            {
                Email = "testusertest",
                Password = "StrongPass123!"
            };

            Func<Task> act = async () => await client.PostAsJsonAsync(
                "/api/identity/register", request, TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<IdentityException>();
        }

        [Fact]
        public async Task Register_ShouldThrow_WhenInvalidPassword()
        {
            var factory = new IdentityWebApplicationFactory()
                 .WithWebHostBuilder(b =>
                 {
                     b.UseEnvironment("Testing");
                 });

            var client = factory.CreateClient();

            var request = new RegisterRequest()
            {
                Email = "testusertest",
                Password = "strong"
            };

            Func<Task> act = async () => await client.PostAsJsonAsync(
                "/api/identity/register", request, TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<IdentityException>();
        }


        [Fact]
        public async Task Register_Should_Create_User()
        {
            var factory = new IdentityWebApplicationFactory()
                 .WithWebHostBuilder(b =>
                 {
                     b.UseEnvironment("Testing");
                 });

            var client = factory.CreateClient();

            var request = new RegisterRequest()
            {
                Email = "testuser@test.com",
                Password = "StrongPass123!"
            };

            var response = await client.PostAsJsonAsync(
                "/api/identity/register", request, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
