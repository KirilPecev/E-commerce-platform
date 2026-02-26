using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using IdentityService.Application.Identity.Commands;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Tests.IntegrationTests
{
    public class ChangeRolesTests : IClassFixture<IdentityWebApplicationFactory>
    {
        [Fact]
        public async Task ChangeRoles_ShouldReturnNoContent_ChangeRolesWithAdminRole()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            // Seed admin
            using (var scope = factory.Services.CreateScope())
            {
                await IdentityTestSeeder.SeedAdminAsync(scope.ServiceProvider);
            }

            string email = "testuser@test.com";
            string password = "StrongPass123!";

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

            var adminLoginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new
                {
                    Email = IdentityTestSeeder.Email,
                    Password = IdentityTestSeeder.Password,
                },
                TestContext.Current.CancellationToken);

            var adminAuth = await adminLoginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth!.Token);

            // Act
            var changeRoleResponse = await client.PutAsJsonAsync($"/api/identity/{auth.UserId}/roles", new
            {
                Roles = new[] { "Admin", "Customer" }
            },
            TestContext.Current.CancellationToken);

            changeRoleResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ChangeRoles_ShouldReturnForbidden_ChangeRolesWithNoAdminRole()
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
            var changeRoleResponse = await client.PutAsJsonAsync($"/api/identity/{auth.UserId}/roles", new
            {
                Roles = new[] { "Admin", "Customer" }
            },
            TestContext.Current.CancellationToken);

            changeRoleResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ChangeRoles_ShouldThrow_WhenInvalidUser()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            // Seed admin
            using (var scope = factory.Services.CreateScope())
            {
                await IdentityTestSeeder.SeedAdminAsync(scope.ServiceProvider);
            }

            string email = "testuser@test.com";
            string password = "StrongPass123!";

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

            var adminLoginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new
                {
                    Email = IdentityTestSeeder.Email,
                    Password = IdentityTestSeeder.Password,
                },
                TestContext.Current.CancellationToken);

            var adminAuth = await adminLoginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth!.Token);

            // Act
            Func<Task> act = async () => await client.PutAsJsonAsync($"/api/identity/d5194374-6037-4fd8-5b24-08de5b8ac4ed/roles", new
            {
                Roles = new[] { "Admin", "Customer" }
            },
            TestContext.Current.CancellationToken);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ChangeRoles_ShouldThrow_WhenInvalidRole()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            // Seed admin
            using (var scope = factory.Services.CreateScope())
            {
                await IdentityTestSeeder.SeedAdminAsync(scope.ServiceProvider);
            }

            string email = "testuser@test.com";
            string password = "StrongPass123!";

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

            var adminLoginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new
                {
                    Email = IdentityTestSeeder.Email,
                    Password = IdentityTestSeeder.Password,
                },
                TestContext.Current.CancellationToken);

            var adminAuth = await adminLoginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth!.Token);

            // Act
            Func<Task> act = async () => await client.PutAsJsonAsync($"/api/identity/{auth.UserId}/roles", new
            {
                Roles = new[] { "TestRole" }
            },
            TestContext.Current.CancellationToken);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
