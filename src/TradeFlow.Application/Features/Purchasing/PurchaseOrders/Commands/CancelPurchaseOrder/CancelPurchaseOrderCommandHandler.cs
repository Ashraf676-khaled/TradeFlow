namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CancelPurchaseOrder;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Purchasing;

public sealed class CancelPurchaseOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelPurchaseOrderCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(CancelPurchaseOrderCommand request, CancellationToken ct)
  {
    var order = await context.PurchaseOrders
        .FirstOrDefaultAsync(o => o.Id == new PurchaseOrderId(request.PurchaseOrderId), ct);

    if (order is null)
      return PurchaseOrderErrors.NotFound;

    var result = order.Cancel();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
