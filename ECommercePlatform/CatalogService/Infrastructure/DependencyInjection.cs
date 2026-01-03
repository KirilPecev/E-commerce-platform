using CatalogService.Infrastructure.Messaging;
using CatalogService.Infrastructure.Persistence;

using ECommercePlatform.Application.Interfaces;

using MassTransit;

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

            return services;
        }
    }
}
