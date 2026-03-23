using System.Reflection;

using FluentAssertions;

using MassTransit;

using MediatR;

using NetArchTest.Rules;

namespace Architecture.Tests
{
    public class CqrsPatternTests
    {
        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Commands_ShouldImplement_IRequest(string service)
        {
            var assembly = GetAssembly(service);

            var commandTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith($"{service}.Application")
                    && t.Name.EndsWith("Command")
                    && !t.Name.EndsWith("CommandHandler")
                    && (t.IsClass || t.IsValueType) && !t.IsAbstract && !t.IsEnum);

            commandTypes.Should().NotBeEmpty(
                $"{service} should have command types");

            commandTypes.Should().AllSatisfy(t =>
                t.GetInterfaces().Any(i =>
                    i == typeof(IRequest)
                    || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
                .Should().BeTrue(
                    $"Command '{t.FullName}' should implement IRequest or IRequest<T>"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("InventoryService")]
        public void Queries_ShouldImplement_IRequestOfT(string service)
        {
            var assembly = GetAssembly(service);

            var queryTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith($"{service}.Application")
                    && t.Name.EndsWith("Query")
                    && !t.Name.EndsWith("QueryHandler")
                    && (t.IsClass || t.IsValueType) && !t.IsAbstract && !t.IsEnum);

            queryTypes.Should().NotBeEmpty(
                $"{service} should have query types");

            queryTypes.Should().AllSatisfy(t =>
                t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .Should().BeTrue(
                    $"Query '{t.FullName}' should implement IRequest<T>"));
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void CommandHandlers_ShouldImplement_ExactlyOne_IRequestHandler(string service)
        {
            var assembly = GetAssembly(service);

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith($"{service}.Application")
                    && t.Name.EndsWith("CommandHandler")
                    && t.IsClass && !t.IsAbstract);

            handlerTypes.Should().NotBeEmpty(
                $"{service} should have command handler types");

            handlerTypes.Should().AllSatisfy(t =>
            {
                var handlerInterfaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType
                        && (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                            || i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
                    .ToList();

                handlerInterfaces.Should().HaveCount(1,
                    $"Handler '{t.FullName}' should implement exactly one IRequestHandler interface (SRP)");
            });
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("InventoryService")]
        public void QueryHandlers_ShouldImplement_ExactlyOne_IRequestHandler(string service)
        {
            var assembly = GetAssembly(service);

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith($"{service}.Application")
                    && t.Name.EndsWith("QueryHandler")
                    && t.IsClass && !t.IsAbstract);

            handlerTypes.Should().NotBeEmpty(
                $"{service} should have query handler types");

            handlerTypes.Should().AllSatisfy(t =>
            {
                var handlerInterfaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    .ToList();

                handlerInterfaces.Should().HaveCount(1,
                    $"Query handler '{t.FullName}' should implement exactly one IRequestHandler<,> interface");
            });
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Handlers_ShouldNotDependOn_OtherHandlers(string service)
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
                    .Where(p => p.ParameterType.Name.EndsWith("CommandHandler")
                             || p.ParameterType.Name.EndsWith("QueryHandler"))
                    .ToList();

                constructorParams.Should().BeEmpty(
                    $"Handler '{handler.Name}' should not depend on other handlers directly. " +
                    $"Use MediatR.IMediator instead. Offending params: [{string.Join(", ", constructorParams.Select(p => p.ParameterType.Name))}]");
            }
        }

        [Theory]
        [InlineData("CatalogService")]
        [InlineData("OrderService")]
        [InlineData("PaymentService")]
        [InlineData("InventoryService")]
        public void Handlers_ShouldResideIn_ApplicationNamespace(string service)
        {
            var assembly = GetAssembly(service);

            var result = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("CommandHandler")
                .Or()
                .HaveNameEndingWith("QueryHandler")
                .Should()
                .ResideInNamespaceStartingWith($"{service}.Application")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                FormatFailingTypes(result, $"All handlers in {service} should reside in {service}.Application namespace"));
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
