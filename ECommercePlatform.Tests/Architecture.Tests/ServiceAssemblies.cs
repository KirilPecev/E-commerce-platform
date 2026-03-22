using System.Reflection;

using CatalogService.Domain.Aggregates;
using InventoryService.Domain.Aggregates;
using OrderService.Domain.Aggregates;
using PaymentService.Domain.Aggregates;

namespace Architecture.Tests
{
    public static class ServiceAssemblies
    {
        public static readonly Assembly Catalog = typeof(Product).Assembly;
        public static readonly Assembly Order = typeof(Order).Assembly;
        public static readonly Assembly Payment = typeof(Payment).Assembly;
        public static readonly Assembly Inventory = typeof(ProductStock).Assembly;
        public static readonly Assembly Identity = typeof(IdentityService.Infrastructure.User).Assembly;
        public static readonly Assembly SharedKernel = typeof(ECommercePlatform.Domain.Entity).Assembly;

        public static readonly Assembly[] DddServices = [Catalog, Order, Payment, Inventory];
        public static readonly Assembly[] AllServices = [Catalog, Order, Payment, Inventory, Identity];

        public static readonly string[] DddServiceNamespaces =
        [
            "CatalogService",
            "OrderService",
            "PaymentService",
            "InventoryService"
        ];
    }
}
