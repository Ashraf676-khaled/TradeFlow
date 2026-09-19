// Commands/ActivateProduct/ActivateProductCommandHandler.cs
namespace TradeFlow.Application.Inventory.Products.Commands.ActivateProduct;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class ActivateProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateProductCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ActivateProductCommand request, CancellationToken ct)
  {
    var product = await context.Products
        .FirstOrDefaultAsync(p => p.Id == new ProductId(request.ProductId), ct);

    if (product is null)
      return ProductErrors.NotFound;

    var result = product.Activate();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
