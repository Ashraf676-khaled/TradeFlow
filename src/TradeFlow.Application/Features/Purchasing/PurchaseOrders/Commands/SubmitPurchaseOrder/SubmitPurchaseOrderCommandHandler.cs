namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.SubmitPurchaseOrder;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Purchasing;

public sealed class SubmitPurchaseOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitPurchaseOrderCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(SubmitPurchaseOrderCommand request, CancellationToken ct)
  {
    var order = await context.PurchaseOrders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == new PurchaseOrderId(request.PurchaseOrderId), ct);

    if (order is null)
      return PurchaseOrderErrors.NotFound;

    var result = order.Submit();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
