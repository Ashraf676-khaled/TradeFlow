// Commands/DeactivateProduct/DeactivateProductCommandHandler.cs
namespace TradeFlow.Application.Inventory.Products.Commands.DeactivateProduct;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class DeactivateProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateProductCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(DeactivateProductCommand request, CancellationToken ct)
  {
    var product = await context.Products
        .FirstOrDefaultAsync(p => p.Id == new ProductId(request.ProductId), ct);

    if (product is null)
      return ProductErrors.NotFound;

    // Cross-Aggregate Check حقيقي: هل فيه SalesOrders نشطة على المنتج ده؟
    // ده هيتفعل فعليًا لما نبني Sales module — دلوقتي بس تحقق الحالة الأساسية
    var result = product.Deactivate();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
