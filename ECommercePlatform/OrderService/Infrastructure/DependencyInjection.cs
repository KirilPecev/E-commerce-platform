using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Identity;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Messaging.Consumers;
using OrderService.Infrastructure.Persistence;

using static MassTransit.Monitoring.Performance.BuiltInCounters;

namespace OrderService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services
                .AddDbContext<OrdersDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("OrdersDb"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(OrdersDbContext).Assembly.FullName)));

            // Domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Integration event publisher
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

            // MassTransit + RabbitMQ
            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(PaymentCompletedEventConsumer).Assembly);

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
    }
}
