using IdentityService.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityService.Tests
{
    public class IdentityWebApplicationFactory
      : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public IdentityWebApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open(); // 🔥 MUST stay open
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<IdentityDbContext>>();

                services.AddDbContext<IdentityDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // Build provider to create schema ON THE SAME CONNECTION
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

                db.Database.EnsureCreated(); // ✅ Creates AspNetRoles
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Dispose();
        }
    }
}
