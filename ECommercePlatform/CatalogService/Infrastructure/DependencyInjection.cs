using CatalogService.Infrastructure.Messaging;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence.Seeding;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Identity;

using MassTransit;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services
                .AddDbContext<CatalogDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("CatalogDb"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName)));

            // Domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Integration event publisher
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

            // MassTransit + RabbitMQ
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration["RabbitMQ:Host"] ?? throw new InvalidOperationException("RabbitMQ:Host configuration is missing.");
                    var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? throw new InvalidOperationException("RabbitMQ:Username configuration is missing.");
                    var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("RabbitMQ:Password configuration is missing.");

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddTokenAuthentication(configuration);

            return services;
        }

        public static async Task<IApplicationBuilder> Initialize(this IApplicationBuilder app)
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            CatalogDbContext dbContext = serviceProvider.GetRequiredService<CatalogDbContext>();

            await CategoriesSeeder.SeedCategoriesAsync(dbContext);

            return app;
        }
    }
}
