// Queries/GetProductById/GetProductByIdQuery.cs
namespace TradeFlow.Application.Inventory.Products.Queries.GetProductById;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDto>>;
