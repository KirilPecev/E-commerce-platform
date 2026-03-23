using System.Reflection;

using FluentAssertions;

using MassTransit;

using NetArchTest.Rules;

namespace Architecture.Tests
{
    public class IntegrationEventTests
    {
        [Fact]
        public void IntegrationEvents_ShouldResideIn_SharedKernelEventsNamespace()
        {
            var result = Types.InAssembly(ServiceAssemblies.SharedKernel)
                .That()
                .HaveNameEndingWith("IntegrationEvent")
                .Should()
                .ResideInNamespaceStartingWith("ECommercePlatform.Events")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, "All integration events should reside in ECommercePlatform.Events namespace"));
        }

        [Fact]
        public void IntegrationEvents_ShouldEndWith_IntegrationEvent()
        {
            var assembly = ServiceAssemblies.SharedKernel;

            var eventTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith("ECommercePlatform.Events")
                    && t.IsClass && !t.IsAbstract);

            eventTypes.Should().NotBeEmpty(
                "Shared kernel should contain integration event types");

            eventTypes.Should().AllSatisfy(t =>
                t.Name.Should().EndWith("IntegrationEvent",
                    $"Type '{t.FullName}' in ECommercePlatform.Events should end with 'IntegrationEvent'"));
        }

        [Fact]
        public void IntegrationEvents_ShouldNotDependOn_AnyServiceDomain()
        {
            var result = Types.InAssembly(ServiceAssemblies.SharedKernel)
                .That()
                .ResideInNamespaceStartingWith("ECommercePlatform.Events")
                .ShouldNot()
                .HaveDependencyOnAny(
                    "CatalogService.Domain",
                    "OrderService.Domain",
                    "PaymentService.Domain",
                    "InventoryService.Domain")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, "Integration events should not depend on any service's domain layer"));
        }

        [Theory]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Consumers_ShouldImplement_IConsumer(string service)
        {
            var assembly = GetAssembly(service);

            var consumerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains("Messaging.Consumers")
                    && t.IsClass && !t.IsAbstract && !t.IsNested);

            consumerTypes.Should().NotBeEmpty(
                $"{service} should have consumer types");

            consumerTypes.Should().AllSatisfy(t =>
                t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>))
                .Should().BeTrue(
                    $"Consumer '{t.FullName}' should implement IConsumer<T>"));
        }

        [Theory]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Consumers_ShouldResideIn_InfrastructureMessagingConsumers(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IConsumer<>))
                .Should()
                .ResideInNamespaceContaining("Infrastructure.Messaging.Consumers")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"All consumers in {service} should reside in Infrastructure.Messaging.Consumers"));
        }

        [Theory]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Consumers_ShouldConsumeOnly_IntegrationEvents(string service)
        {
            var assembly = GetAssembly(service);

            var consumerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains("Messaging.Consumers")
                    && t.IsClass && !t.IsAbstract);

            foreach (var consumer in consumerTypes)
            {
                var consumedTypes = consumer.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .ToList();

                consumedTypes.Should().AllSatisfy(t =>
                    t.Name.Should().EndWith("IntegrationEvent",
                        $"Consumer '{consumer.Name}' should only consume integration events, " +
                        $"but consumes '{t.FullName}'"));
            }
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DomainEventHandlers_ShouldPublish_IntegrationEvents(string service)
        {
            var assembly = GetAssembly(service);

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains("DomainEventHandlers")
                    && t.IsClass && !t.IsAbstract && !t.IsNested);

            handlerTypes.Should().NotBeEmpty(
                $"{service} should have domain event handlers");

            // Domain event handlers should depend on IEventPublisher (they translate domain → integration)
            handlerTypes.Should().AllSatisfy(t =>
            {
                var constructorParams = t.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .Select(p => p.ParameterType);

                constructorParams.Should().Contain(
                    p => p.Name == "IEventPublisher",
                    $"Domain event handler '{t.Name}' should depend on IEventPublisher to publish integration events");
            });
        }

        private static Assembly GetAssembly(string service) => service switch
        {
            "CatalogService" => ServiceAssemblies.Catalog,
            "OrderService" => ServiceAssemblies.Order,
            "PaymentService" => ServiceAssemblies.Payment,
            "InventoryService" => ServiceAssemblies.Inventory,
            _ => throw new ArgumentException($"Unknown service: {service}")
        };

        private static string FormatFailingTypes(TestResult result, string rule)
        {
            if (result.IsSuccessful || result.FailingTypes is null)
                return rule;

            var types = string.Join(", ", result.FailingTypes.Select(t => t.FullName));
            return $"{rule}. Offending types: [{types}]";
        }
    }
}
