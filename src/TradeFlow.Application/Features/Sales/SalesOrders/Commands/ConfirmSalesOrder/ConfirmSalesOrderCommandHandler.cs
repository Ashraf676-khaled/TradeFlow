namespace TradeFlow.Application.Sales.SalesOrders.Commands.ConfirmSalesOrder;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Settings;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Customers;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Sales;

public sealed class ConfirmSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ConfirmSalesOrderCommand, Result<ConfirmSalesOrderResult>>
{
  private const int DefaultInvoiceDueDays = 30;

  public async Task<Result<ConfirmSalesOrderResult>> Handle(ConfirmSalesOrderCommand request, CancellationToken ct)
  {
    var order = await context.SalesOrders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == new SalesOrderId(request.SalesOrderId), ct);

    if (order is null)
      return SalesOrderErrors.NotFound;

    if (order.Items.Count == 0)
      return SalesOrderErrors.EmptyOrder;

    var totalResult = order.CalculateTotal();
    if (totalResult.IsError)
      return totalResult.Errors;

    var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId, ct);
    if (customer is null)
      return CustomerErrors.NotFound;

    // 1) Credit check before any mutation — must happen before Confirm().
    var creditResult = customer.ValidateCredit(totalResult.Value);
    if (creditResult.IsError)
      return creditResult.Errors;

    // 2) Stock availability pre-check: a confirmed order can never exist without
    //    enough reservable inventory in the selected warehouse.
    var stockResult = await ValidateStockAvailabilityAsync(order, ct);
    if (stockResult.IsError)
      return stockResult.Errors;

    // 3) Domain confirm → raises SalesOrderConfirmedDomainEvent; its handler reserves
    //    stock inside the SaveChanges triggered below (same unit of work).
    var confirmResult = order.Confirm();
    if (confirmResult.IsError)
      return confirmResult.Errors;

    var increaseBalanceResult = customer.IncreaseBalance(totalResult.Value);
    if (increaseBalanceResult.IsError)
      return increaseBalanceResult.Errors;

    // 4) Invoice is generated automatically — no extra API round-trip or UI friction.
    var invoiceResult = await CreateInvoiceAsync(order, totalResult.Value, ct);
    if (invoiceResult.IsError)
      return invoiceResult.Errors;

    // 5) Credit/deferred sales setting: when disabled this is a cash/POS sale —
    //    collect the full amount immediately so the invoice is issued as Paid,
    //    inside the same transaction (no follow-up payment round-trip).
    var settings = SystemSettingsResolver.Resolve(await context.SystemSettings.ToListAsync(ct));
    if (!settings.CreditSalesEnabled)
    {
      var paymentResult = invoiceResult.Value.RegisterPayment(totalResult.Value);
      if (paymentResult.IsError)
        return paymentResult.Errors;
    }

    try
    {
      // Single SaveChanges: order status, customer balance, stock reservation
      // (via domain event), the invoice — and optional cash payment — are
      // persisted atomically; EF Core wraps this call in one database
      // transaction on relational providers.
      await context.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
      return Error.Conflict(
          "Inventory.ConcurrentUpdate",
          "المخزون تغير أثناء التأكيد. أعد تحميل البيانات وحاول مرة أخرى.");
    }

    return new ConfirmSalesOrderResult(order.Id.Value, invoiceResult.Value.Id.Value);
  }

  private async Task<Result<Success>> ValidateStockAvailabilityAsync(SalesOrder order, CancellationToken ct)
  {
    foreach (var item in order.Items)
    {
      var stockItem = await context.StockItems
          .FirstOrDefaultAsync(s => s.WarehouseId == order.WarehouseId && s.ProductId == item.ProductId, ct);

      if (stockItem is null || !stockItem.AvailableQuantity.IsGreaterThanOrEqual(item.Quantity))
        return StockItemErrors.InsufficientStock;
    }

    return Result.Success;
  }

  private async Task<Result<Invoice>> CreateInvoiceAsync(SalesOrder order, Money total, CancellationToken ct)
  {
    var sequence = await context.Invoices.CountAsync(ct) + 1;
    var invoiceNumberResult = DocumentNumber.Generate("INV", sequence);
    if (invoiceNumberResult.IsError)
      return invoiceNumberResult.Errors;

    var invoiceResult = Invoice.Create(
        order.TenantId,
        order.Id,
        order.CustomerId,
        invoiceNumberResult.Value,
        total,
        DateTimeOffset.UtcNow.AddDays(DefaultInvoiceDueDays));

    if (invoiceResult.IsError)
      return invoiceResult.Errors;

    context.Invoices.Add(invoiceResult.Value);
    return invoiceResult.Value;
  }
}
