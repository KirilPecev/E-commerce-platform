using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Identity;

using InventoryService.Infrastructure.Messaging;
using InventoryService.Infrastructure.Messaging.Consumers;
using InventoryService.Infrastructure.Persistence;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services
                .AddDbContext<InventoryDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("InventoryDb"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

            // Domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Integration event publisher
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

            // MassTransit + RabbitMQ
            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(ProductCreatedIntegrationEventConsumer).Assembly);

                x.SetEndpointNameFormatter(
                    new KebabCaseEndpointNameFormatter("inventory", false));

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
    }
}
