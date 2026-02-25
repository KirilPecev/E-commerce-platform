using ECommercePlatform.Identity;

using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Seeding;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (!environment.IsEnvironment("Testing"))
            {
                // DbContext
                services
                    .AddDbContext<IdentityDbContext>(options =>
                        options.UseSqlServer(
                            configuration.GetConnectionString("IdentityDb"),
                            sqlOptions => sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName)));
            }

            services
                .AddIdentity<User, Role>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddTokenAuthentication(configuration);

            services.AddTransient<IJwtTokenGenerator, JwtTokenGeneratorService>();

            return services;
        }

        public static async Task<IApplicationBuilder> Initialize(this IApplicationBuilder app)
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            RoleManager<Role> roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            await RoleSeeder.SeedRolesAsync(roleManager);

            return app;
        }
    }
}
