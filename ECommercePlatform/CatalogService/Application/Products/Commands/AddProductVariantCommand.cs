
using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record AddProductVariantCommand(
        Guid Id,
        string Sku,
        decimal Amount,
        string Currency,
        int StockQuantity,
        string? Size,
        string? Color) : IRequest<Guid>;
}