using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.Data;

namespace IdentityService.Tests.ApplicationTests
{
    public class RegisterTests : IClassFixture<IdentityWebApplicationFactory>
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
    }
}
