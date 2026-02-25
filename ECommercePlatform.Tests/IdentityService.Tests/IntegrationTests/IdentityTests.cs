using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using IdentityService.Application.Identity.Commands;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;

namespace IdentityService.Tests.IntegrationTests
{
    public class IdentityTests : IClassFixture<IdentityWebApplicationFactory>
    {
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

        [Fact]
        public async Task Register_Then_Login_ShouldReturnToken()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            var email = "flow@test.com";
            var password = "Password123!";

            var registerResponse = await client.PostAsJsonAsync(
                "/api/identity/register",
                new
                {
                    Email = email,
                    Password = password
                },
                TestContext.Current.CancellationToken);

            registerResponse.EnsureSuccessStatusCode();
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new
                {
                    Email = email,
                    Password = password
                },
                TestContext.Current.CancellationToken);

            loginResponse.EnsureSuccessStatusCode();
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await loginResponse.Content
                .ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);

            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldThrow_WhenCredentialsInvalid()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            // Act
            Func<Task> act = async () => await client.PostAsJsonAsync("/api/identity/login", new
            {
                Email = "test@test.com",
                Password = "Password123!"
            },
            TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}