using FluentAssertions;

using NetArchTest.Rules;

namespace Architecture.Tests
{
    public class CrossServiceBoundaryTests
    {
        public static TheoryData<string, string> ServicePairs
        {
            get
            {
                var services = new[] { "CatalogService", "OrderService", "PaymentService", "InventoryService", "IdentityService" };
                var data = new TheoryData<string, string>();

                for (int i = 0; i < services.Length; i++)
                {
                    for (int j = 0; j < services.Length; j++)
                    {
                        if (i != j)
                            data.Add(services[i], services[j]);
                    }
                }

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(ServicePairs))]
        public void Service_ShouldNotDependOn_AnotherService(string source, string target)
        {
            var assembly = source switch
            {
                "CatalogService" => ServiceAssemblies.Catalog,
                "OrderService" => ServiceAssemblies.Order,
                "PaymentService" => ServiceAssemblies.Payment,
                "InventoryService" => ServiceAssemblies.Inventory,
                "IdentityService" => ServiceAssemblies.Identity,
                _ => throw new ArgumentException($"Unknown service: {source}")
            };

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceStartingWith(source)
                .ShouldNot()
                .HaveDependencyOn(target)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{source} should not depend on {target}. " +
                (result.FailingTypes is not null
                    ? $"Offending types: [{string.Join(", ", result.FailingTypes.Select(t => t.FullName))}]"
                    : string.Empty));
        }
    }
}
