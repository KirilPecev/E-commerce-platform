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

        [Fact]
        public async Task ChangeRoles_ShouldReturnUnauthorized_WhenNoToken()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            // Act
            var changeRoleResponse = await client.PutAsJsonAsync($"/api/identity/{Guid.NewGuid()}/roles", new
            {
                Roles = new[] { "Admin" }
            },
            TestContext.Current.CancellationToken);

            // Assert
            changeRoleResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ChangeRoles_ShouldAllowAdminAccess_AfterPromotingToAdmin()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            using (var scope = factory.Services.CreateScope())
            {
                await IdentityTestSeeder.SeedAdminAsync(scope.ServiceProvider);
            }

            string email = "promoted@test.com";
            string password = "StrongPass123!";

            var request = new RegisterRequest()
            {
                Email = email,
                Password = password
            };

            await client.PostAsJsonAsync(
                "/api/identity/register", request, TestContext.Current.CancellationToken);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new { Email = email, Password = password },
                TestContext.Current.CancellationToken);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);

            // Promote user to Admin using seeded admin account
            var adminLoginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new { Email = IdentityTestSeeder.Email, Password = IdentityTestSeeder.Password },
                TestContext.Current.CancellationToken);

            var adminAuth = await adminLoginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth!.Token);

            var promoteResponse = await client.PutAsJsonAsync($"/api/identity/{auth!.UserId}/roles", new
            {
                Roles = new[] { "Admin", "Customer" }
            },
            TestContext.Current.CancellationToken);

            promoteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Re-login as promoted user to get a token with the new roles
            var newLoginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new { Email = email, Password = password },
                TestContext.Current.CancellationToken);

            var newAuth = await newLoginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newAuth!.Token);

            // Act - promoted user should now be able to call admin-only endpoint
            var changeRoleResponse = await client.PutAsJsonAsync($"/api/identity/{auth.UserId}/roles", new
            {
                Roles = new[] { "Customer" }
            },
            TestContext.Current.CancellationToken);

            // Assert
            changeRoleResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ChangeRoles_ShouldThrow_WhenEmptyRoles()
        {
            // Arrange
            var factory = new IdentityWebApplicationFactory()
                .WithWebHostBuilder(b =>
                {
                    b.UseEnvironment("Testing");
                });

            var client = factory.CreateClient();

            using (var scope = factory.Services.CreateScope())
            {
                await IdentityTestSeeder.SeedAdminAsync(scope.ServiceProvider);
            }

            string email = "emptyroles@test.com";
            string password = "StrongPass123!";

            var request = new RegisterRequest()
            {
                Email = email,
                Password = password
            };

            await client.PostAsJsonAsync(
                "/api/identity/register", request, TestContext.Current.CancellationToken);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new { Email = email, Password = password },
                TestContext.Current.CancellationToken);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);

            var adminLoginResponse = await client.PostAsJsonAsync(
                "/api/identity/login",
                new { Email = IdentityTestSeeder.Email, Password = IdentityTestSeeder.Password },
                TestContext.Current.CancellationToken);

            var adminAuth = await adminLoginResponse.Content.ReadFromJsonAsync<AuthResult>(TestContext.Current.CancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth!.Token);

            // Act
            Func<Task> act = async () => await client.PutAsJsonAsync($"/api/identity/{auth!.UserId}/roles", new
            {
                Roles = Array.Empty<string>()
            },
            TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
