// Commands/ChangeSellingPrice/ChangeSellingPriceCommandHandler.cs
namespace TradeFlow.Application.Inventory.Products.Commands.ChangeSellingPrice;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;

public sealed class ChangeSellingPriceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ChangeSellingPriceCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ChangeSellingPriceCommand request, CancellationToken ct)
  {
    var product = await context.Products
        .FirstOrDefaultAsync(p => p.Id == new ProductId(request.ProductId), ct);

    if (product is null)
      return ProductErrors.NotFound;

    var priceResult = Money.EGP(request.NewPrice);
    if (priceResult.IsError)
      return priceResult.Errors;

    var result = product.ChangeSellingPrice(priceResult.Value);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
