using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using IdentityService.Application.Exceptions;
using IdentityService.Application.Identity.Commands;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;

namespace IdentityService.Tests.IntegrationTests
{
    public class ChangePasswordTests : IClassFixture<IdentityWebApplicationFactory>
    {
        [Fact]
        public async Task ChangePassword_Should_ChangePasswordWithValidData()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            string email = "testuser@test.com";
            string password = "StrongPass123!";
            string newPassword = "NewStrongPass123!";

            var request = new RegisterRequest()
            {
                Email = email,
                Password = password
            };

            var response = await client.PostAsJsonAsync(
                "/api/identity/register", request, TestContext.Current.CancellationToken);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new
                {
                    Email = email,
                    Password = password
                },
                TestContext.Current.CancellationToken);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

            // Act
            var changePasswordResponse = await client.PutAsJsonAsync("/api/identity/me/password", new
            {
                CurrentPassword = password,
                NewPassword = newPassword
            },
            TestContext.Current.CancellationToken);

            // Assert
            changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var loginResponseAfterChange = await client.PostAsJsonAsync(
                "/api/identity/login",
                new
                {
                    Email = email,
                    Password = newPassword
                },
                TestContext.Current.CancellationToken);

            loginResponseAfterChange.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ChangePassword_ShouldThrow_Unauthorized()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            string email = "testuser@test.com";
            string password = "StrongPass123!";
            string newPassword = "NewStrongPass123!";

            var request = new RegisterRequest()
            {
                Email = email,
                Password = password
            };

            var response = await client.PostAsJsonAsync(
                "/api/identity/register", request, TestContext.Current.CancellationToken);

            // Act
            var changePasswordResponse = await client.PutAsJsonAsync("/api/identity/me/password", new
            {
                CurrentPassword = password,
                NewPassword = newPassword
            },
            TestContext.Current.CancellationToken);

            // Assert
            changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ChangePassword_ShouldThrow_NewInvalidPassword()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            string email = "testuser@test.com";
            string password = "StrongPass123!";
            string newPassword = "newstron";

            var request = new RegisterRequest()
            {
                Email = email,
                Password = password
            };

            var response = await client.PostAsJsonAsync(
                "/api/identity/register", request, TestContext.Current.CancellationToken);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new
                {
                    Email = email,
                    Password = password
                },
                TestContext.Current.CancellationToken);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

            // Act
            Func<Task> act = async () => await client.PutAsJsonAsync("/api/identity/me/password", new
            {
                CurrentPassword = password,
                NewPassword = newPassword
            },
            TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<IdentityException>();
        }
    }
}
