// Queries/GetProducts/GetProductsQuery.cs
namespace TradeFlow.Application.Inventory.Products.Queries.GetProducts;

using MediatR;
using TradeFlow.Application.Common.Models;

public sealed record GetProductsQuery(int PageNumber = 1, int PageSize = 20, bool? IsActive = null)
    : IRequest<PaginatedList<ProductDto>>;
