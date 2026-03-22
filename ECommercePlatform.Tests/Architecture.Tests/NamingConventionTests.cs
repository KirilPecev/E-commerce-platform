using System.Reflection;

using FluentAssertions;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using NetArchTest.Rules;

namespace Architecture.Tests
{
    public class NamingConventionTests
    {
        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        [InlineData("IdentityService")]
        public void Controllers_ShouldEndWith_Controller(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Controllers")
                .And()
                .AreClasses()
                .And()
                .Inherit(typeof(ControllerBase))
                .Should()
                .HaveNameEndingWith("Controller")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, "All controllers should end with 'Controller'"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DomainEvents_ShouldEndWith_DomainEvent(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Domain.Events")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("DomainEvent")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, "All domain event classes should end with 'DomainEvent'"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DomainEventHandlers_ShouldEndWith_DomainEventHandler(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Application.DomainEventHandlers")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("DomainEventHandler")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, "All domain event handler classes should end with 'DomainEventHandler'"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void CommandHandlers_ShouldEndWith_CommandHandler(string service)
        {
            var assembly = GetAssembly(service);

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith($"{service}.Application")
                    && t.IsClass && !t.IsAbstract
                    && t.Name.Contains("Command")
                    && t.GetInterfaces().Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

            handlerTypes.Should().AllSatisfy(t =>
                t.Name.Should().EndWith("CommandHandler",
                    $"Type '{t.FullName}' implements IRequestHandler and contains 'Command' but does not end with 'CommandHandler'"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void QueryHandlers_ShouldEndWith_QueryHandler(string service)
        {
            var assembly = GetAssembly(service);

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith($"{service}.Application")
                    && t.IsClass && !t.IsAbstract
                    && t.Name.Contains("Query")
                    && t.GetInterfaces().Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

            handlerTypes.Should().AllSatisfy(t =>
                t.Name.Should().EndWith("QueryHandler",
                    $"Type '{t.FullName}' implements IRequestHandler and contains 'Query' but does not end with 'QueryHandler'"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Consumers_ShouldEndWith_Consumer(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining("Messaging.Consumers")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Consumer")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, "All consumer classes should end with 'Consumer'"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DomainExceptions_ShouldEndWith_Exception(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Domain.Exceptions")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Exception")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, "All domain exception classes should end with 'Exception'"));
        }

        private static Assembly GetAssembly(string service) => service switch
        {
            "CatalogService" => ServiceAssemblies.Catalog,
            "OrderService" => ServiceAssemblies.Order,
            "PaymentService" => ServiceAssemblies.Payment,
            "InventoryService" => ServiceAssemblies.Inventory,
            "IdentityService" => ServiceAssemblies.Identity,
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
