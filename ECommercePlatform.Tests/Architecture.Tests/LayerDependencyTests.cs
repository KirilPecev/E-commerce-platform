using FluentAssertions;

using NetArchTest.Rules;

namespace Architecture.Tests
{
    public class LayerDependencyTests
    {
        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Domain_ShouldNotDependOn_Application(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Domain")
                .ShouldNot()
                .HaveDependencyOn($"{service}.Application")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"{service}.Domain should not depend on {service}.Application"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Domain_ShouldNotDependOn_Infrastructure(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Domain")
                .ShouldNot()
                .HaveDependencyOn($"{service}.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"{service}.Domain should not depend on {service}.Infrastructure"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Domain_ShouldNotDependOn_Controllers(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Domain")
                .ShouldNot()
                .HaveDependencyOn($"{service}.Controllers")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"{service}.Domain should not depend on {service}.Controllers"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Application_ShouldNotDependOn_Infrastructure(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Application")
                .ShouldNot()
                .HaveDependencyOn($"{service}.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"{service}.Application should not depend on {service}.Infrastructure"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Application_ShouldNotDependOn_Controllers(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Application")
                .ShouldNot()
                .HaveDependencyOn($"{service}.Controllers")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"{service}.Application should not depend on {service}.Controllers"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Domain_ShouldNotDependOn_MassTransit(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Domain")
                .And()
                .DoNotResideInNamespace($"{service}.Domain.Events")
                .ShouldNot()
                .HaveDependencyOn("MassTransit")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"{service}.Domain should not depend on MassTransit"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Domain_ShouldNotDependOn_EntityFramework(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace($"{service}.Domain")
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"{service}.Domain should not depend on EntityFrameworkCore"));
        }

        private static System.Reflection.Assembly GetAssembly(string service) => service switch
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
