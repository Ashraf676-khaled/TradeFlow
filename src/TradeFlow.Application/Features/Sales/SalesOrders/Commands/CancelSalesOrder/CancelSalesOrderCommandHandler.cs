namespace TradeFlow.Application.Sales.SalesOrders.Commands.CancelSalesOrder;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Sales;

public sealed class CancelSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelSalesOrderCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(CancelSalesOrderCommand request, CancellationToken ct)
  {
    var order = await context.SalesOrders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == new SalesOrderId(request.SalesOrderId), ct);

    if (order is null)
      return SalesOrderErrors.NotFound;

    var wasConfirmed = order.Status == OrderStatus.Confirmed;
    Domain.Common.ValueObjects.Money? total = null;

    if (wasConfirmed)
    {
      var totalResult = order.CalculateTotal();
      if (totalResult.IsError)
        return totalResult.Errors;
      total = totalResult.Value;
    }

    // بيطلق SalesOrderCancelledDomainEvent لو كان Confirmed (فك الحجز عبر Event Handler)
    var cancelResult = order.Cancel();
    if (cancelResult.IsError)
      return cancelResult.Errors;

    if (wasConfirmed && total is not null)
    {
      var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId, ct);
      if (customer is not null)
      {
        customer.DecreaseBalance(total);
      }
    }

    try
    {
      await context.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
      return Error.Conflict(
          "Inventory.ConcurrentUpdate",
          "المخزون تغير أثناء الإلغاء. أعد تحميل البيانات وحاول مرة أخرى.");
    }

    return Result.Success;
  }
}
