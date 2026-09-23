// Commands/CreateProduct/CreateProductCommand.cs
namespace TradeFlow.Application.Inventory.Products.Commands.CreateProduct;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    decimal SellingPrice,
    decimal Cost,
    int MinimumStock,
    int OpeningStockQuantity = 0,
    Guid? OpeningStockWarehouseId = null) : IRequest<Result<Guid>>;
