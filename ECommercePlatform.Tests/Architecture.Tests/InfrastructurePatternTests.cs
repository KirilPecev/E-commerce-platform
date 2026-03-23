using System.Reflection;

using FluentAssertions;

using MediatR;

using Microsoft.EntityFrameworkCore;

using NetArchTest.Rules;

namespace Architecture.Tests
{
    public class InfrastructurePatternTests
    {
        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DbContext_ShouldResideIn_InfrastructurePersistence(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(DbContext))
                .Should()
                .ResideInNamespaceContaining("Infrastructure.Persistence")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"DbContext classes in {service} should reside in Infrastructure.Persistence"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void ApplicationInterfaces_ShouldOnlyContainInterfaces(string service)
        {
            var assembly = GetAssembly(service);

            var types = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace == $"{service}.Application.Interfaces"
                    && !t.IsNested);

            types.Should().NotBeEmpty(
                $"{service} should have types in Application.Interfaces");

            types.Should().AllSatisfy(t =>
                t.IsInterface.Should().BeTrue(
                    $"Type '{t.FullName}' in Application.Interfaces should be an interface"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void ApplicationHandlers_ShouldNotReference_ConcreteDbContext(string service)
        {
            var assembly = GetAssembly(service);

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith($"{service}.Application")
                    && (t.Name.EndsWith("CommandHandler") || t.Name.EndsWith("QueryHandler"))
                    && t.IsClass && !t.IsAbstract);

            foreach (var handler in handlerTypes)
            {
                var constructorParams = handler.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .ToList();

                constructorParams.Should().NotContain(
                    p => typeof(DbContext).IsAssignableFrom(p.ParameterType) && !p.ParameterType.IsInterface,
                    $"Handler '{handler.Name}' should depend on a DbContext interface, not a concrete DbContext class. " +
                    "Use the Application.Interfaces abstraction instead.");
            }
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        [InlineData("IdentityService")]
        public void Controllers_ShouldDependOn_IMediator(string service)
        {
            var assembly = GetAssembly(service);

            var controllerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains("Controllers")
                    && t.IsClass && !t.IsAbstract
                    && typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(t));

            controllerTypes.Should().NotBeEmpty(
                $"{service} should have controllers");

            controllerTypes.Should().AllSatisfy(t =>
            {
                var constructorParams = t.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .Select(p => p.ParameterType);

                constructorParams.Should().Contain(
                    p => p == typeof(IMediator),
                    $"Controller '{t.Name}' should depend on IMediator for dispatching commands/queries");
            });
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        [InlineData("IdentityService")]
        public void Controllers_ShouldNotDependOn_DbContext(string service)
        {
            var assembly = GetAssembly(service);

            var controllerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains("Controllers")
                    && t.IsClass && !t.IsAbstract
                    && typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(t));

            foreach (var controller in controllerTypes)
            {
                var constructorParams = controller.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .ToList();

                constructorParams.Should().NotContain(
                    p => typeof(DbContext).IsAssignableFrom(p.ParameterType),
                    $"Controller '{controller.Name}' should not depend on DbContext directly. " +
                    "Use MediatR commands/queries instead.");
            }
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DependencyInjection_ShouldResideIn_InfrastructureOrApplicationNamespace(string service)
        {
            var assembly = GetAssembly(service);

            var diTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("DependencyInjection")
                    && t.IsClass && !t.IsNested);

            diTypes.Should().NotBeEmpty(
                $"{service} should have DependencyInjection classes");

            diTypes.Should().AllSatisfy(t =>
                (t.Namespace == $"{service}.Infrastructure" || t.Namespace == $"{service}.Application")
                    .Should().BeTrue(
                        $"DependencyInjection class '{t.FullName}' should reside in {service}.Infrastructure or {service}.Application"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void DbContext_ShouldImplement_ApplicationInterface(string service)
        {
            var assembly = GetAssembly(service);

            var dbContextTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.Contains("Infrastructure.Persistence")
                    && typeof(DbContext).IsAssignableFrom(t)
                    && t.IsClass && !t.IsAbstract);

            dbContextTypes.Should().NotBeEmpty(
                $"{service} should have a DbContext in Infrastructure.Persistence");

            dbContextTypes.Should().AllSatisfy(t =>
            {
                var appInterfaces = t.GetInterfaces()
                    .Where(i => i.Namespace is not null
                        && i.Namespace.Contains("Application.Interfaces"));

                appInterfaces.Should().NotBeEmpty(
                    $"DbContext '{t.Name}' should implement an interface from Application.Interfaces " +
                    "to allow the Application layer to depend on an abstraction");
            });
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
