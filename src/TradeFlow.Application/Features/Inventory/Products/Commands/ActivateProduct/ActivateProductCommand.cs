// Commands/ActivateProduct/ActivateProductCommand.cs
namespace TradeFlow.Application.Inventory.Products.Commands.ActivateProduct;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ActivateProductCommand(Guid ProductId) : IRequest<Result<Success>>;
