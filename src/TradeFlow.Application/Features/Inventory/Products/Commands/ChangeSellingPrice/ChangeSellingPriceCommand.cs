// Commands/ChangeSellingPrice/ChangeSellingPriceCommand.cs
namespace TradeFlow.Application.Inventory.Products.Commands.ChangeSellingPrice;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ChangeSellingPriceCommand(Guid ProductId, decimal NewPrice) : IRequest<Result<Success>>;
