namespace TradeFlow.Application.Sales.SalesOrders.Commands.CompleteSalesOrder;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Sales;

public sealed class CompleteSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CompleteSalesOrderCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(CompleteSalesOrderCommand request, CancellationToken ct)
  {
    var order = await context.SalesOrders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == new SalesOrderId(request.SalesOrderId), ct);

    if (order is null)
      return SalesOrderErrors.NotFound;

    var completeResult = order.Complete();
    if (completeResult.IsError)
      return completeResult.Errors;

    // خصم فعلي نهائي من المخزون المحجوز — البضاعة خرجت فعليًا
    foreach (var item in order.Items)
    {
      var stockItem = await context.StockItems
          .FirstOrDefaultAsync(s => s.WarehouseId == order.WarehouseId && s.ProductId == item.ProductId, ct);

      if (stockItem is null)
        return StockItemErrors.NotFound;

      var deductionResult = stockItem.ConfirmDeduction(item.Quantity);
      if (deductionResult.IsError)
        return deductionResult.Errors;
    }

    try
    {
      await context.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
      return Error.Conflict(
          "Inventory.ConcurrentUpdate",
          "المخزون تغير أثناء الإكمال. أعد تحميل البيانات وحاول مرة أخرى.");
    }

    return Result.Success;
  }
}
