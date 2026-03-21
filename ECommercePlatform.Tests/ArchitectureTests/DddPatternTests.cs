using System.Reflection;

using ECommercePlatform.Domain;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using NetArchTest.Rules;

namespace ArchitectureTests
{
    public class DddPatternTests
    {
        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void AggregateClasses_ShouldInheritFrom_AggregateRootOrEntity(string service)
        {
            var assembly = GetAssembly(service);

            var aggregateTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains($"{service}.Domain.Aggregates")
                    && t.IsClass && !t.IsAbstract && !t.IsEnum && !t.IsNested);

            aggregateTypes.Should().NotBeEmpty(
                $"{service} should have aggregate/entity types in Domain.Aggregates");

            aggregateTypes.Should().AllSatisfy(t =>
                (typeof(AggregateRoot).IsAssignableFrom(t) || typeof(Entity).IsAssignableFrom(t))
                    .Should().BeTrue(
                        $"Type '{t.FullName}' in Domain.Aggregates should inherit from AggregateRoot or Entity"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        public void ValueObjects_ShouldInheritFrom_ValueObject(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Domain.ValueObjects")
                .And()
                .AreClasses()
                .Should()
                .Inherit(typeof(ValueObject))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"All value objects in {service} should inherit from ValueObject"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DomainEvents_ShouldImplement_IDomainEvent(string service)
        {
            var assembly = GetAssembly(service);

            var eventTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains($"{service}.Domain.Events")
                    && t.IsClass && !t.IsAbstract);

            eventTypes.Should().NotBeEmpty(
                $"{service} should have domain event types");

            eventTypes.Should().AllSatisfy(t =>
                typeof(IDomainEvent).IsAssignableFrom(t).Should().BeTrue(
                    $"Type '{t.FullName}' should implement IDomainEvent"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DomainEvents_ShouldImplement_INotification(string service)
        {
            var assembly = GetAssembly(service);

            var eventTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains($"{service}.Domain.Events")
                    && t.IsClass && !t.IsAbstract);

            eventTypes.Should().AllSatisfy(t =>
                typeof(INotification).IsAssignableFrom(t).Should().BeTrue(
                    $"Domain event '{t.FullName}' should implement INotification for MediatR dispatch"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DomainEventHandlers_ShouldImplement_INotificationHandler(string service)
        {
            var assembly = GetAssembly(service);

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains($"{service}.Application.DomainEventHandlers")
                    && t.IsClass && !t.IsAbstract && !t.IsNested);

            handlerTypes.Should().NotBeEmpty(
                $"{service} should have domain event handlers");

            handlerTypes.Should().AllSatisfy(t =>
                t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Should().BeTrue(
                    $"Type '{t.FullName}' should implement INotificationHandler<T>"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        [InlineData("IdentityService")]
        public void Controllers_ShouldInheritFrom_ControllerBase(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Controllers")
                .And()
                .AreClasses()
                .Should()
                .Inherit(typeof(ControllerBase))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"All classes in {service}.Controllers should inherit from ControllerBase"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        [InlineData("IdentityService")]
        public void Controllers_ShouldHave_ApiControllerAttribute(string service)
        {
            var assembly = GetAssembly(service);

            var controllerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains($"{service}.Controllers")
                    && t.IsClass && !t.IsAbstract
                    && typeof(ControllerBase).IsAssignableFrom(t));

            controllerTypes.Should().NotBeEmpty(
                $"{service} should have at least one controller");

            controllerTypes.Should().AllSatisfy(t =>
                t.GetCustomAttributes(typeof(ApiControllerAttribute), true)
                    .Should().NotBeEmpty(
                        $"Controller '{t.FullName}' should have [ApiController] attribute"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DomainAggregates_ShouldNotHavePublicSetters(string service)
        {
            var assembly = GetAssembly(service);

            var aggregateTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains($"{service}.Domain.Aggregates")
                    && t.IsClass && !t.IsAbstract && !t.IsEnum
                    && typeof(Entity).IsAssignableFrom(t));

            foreach (var type in aggregateTypes)
            {
                var publicSetters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(p => p.SetMethod is not null && p.SetMethod.IsPublic)
                    .ToList();

                publicSetters.Should().BeEmpty(
                    $"Aggregate '{type.Name}' should not expose public setters. " +
                    $"Properties with public setters: [{string.Join(", ", publicSetters.Select(p => p.Name))}]");
            }
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        public void ValueObjects_ShouldNotHavePublicSetters(string service)
        {
            var assembly = GetAssembly(service);

            var valueObjectTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains($"{service}.Domain.ValueObjects")
                    && t.IsClass && !t.IsAbstract
                    && typeof(ValueObject).IsAssignableFrom(t));

            foreach (var type in valueObjectTypes)
            {
                var publicSetters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(p => p.SetMethod is not null && p.SetMethod.IsPublic)
                    .ToList();

                publicSetters.Should().BeEmpty(
                    $"Value object '{type.Name}' should not expose public setters. " +
                    $"Properties with public setters: [{string.Join(", ", publicSetters.Select(p => p.Name))}]");
            }
        }

        [Fact]
        public void SharedKernel_ShouldNotDependOn_AnyService()
        {
            var result = Types.InAssembly(ServiceAssemblies.SharedKernel)
                .That()
                .ResideInNamespaceStartingWith("ECommercePlatform")
                .ShouldNot()
                .HaveDependencyOnAny(
                    "CatalogService",
                    "OrderService",
                    "PaymentService",
                    "InventoryService",
                    "IdentityService")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, "ECommercePlatform shared kernel should not depend on any service"));
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
