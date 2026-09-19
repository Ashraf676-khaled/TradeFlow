// Commands/DeactivateProduct/DeactivateProductCommand.cs
namespace TradeFlow.Application.Inventory.Products.Commands.DeactivateProduct;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record DeactivateProductCommand(Guid ProductId) : IRequest<Result<Success>>;
