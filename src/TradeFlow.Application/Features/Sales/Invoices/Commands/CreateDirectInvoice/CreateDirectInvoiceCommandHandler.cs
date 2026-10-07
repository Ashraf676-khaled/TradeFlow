namespace TradeFlow.Application.Sales.Invoices.Commands.CreateDirectInvoice;

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

public sealed class CreateDirectInvoiceCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateDirectInvoiceCommand, Result<Guid>>
{
  private const int DefaultInvoiceDueDays = 30;

  public async Task<Result<Guid>> Handle(CreateDirectInvoiceCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null || currentUser.UserId is null)
      return Error.Unauthorized("Auth.NoTenant", "Unable to determine the current user or company.");

    if (request.Items.Count == 0)
      return SalesOrderErrors.EmptyOrder;

    var settings = SystemSettingsResolver.Resolve(await context.SystemSettings.ToListAsync(ct));
    if (request.IsCreditSale && !settings.CreditSalesEnabled)
      return Error.Conflict("Settings.CreditSalesDisabled", "البيع الآجل غير مفعل.");

    var customerId = new CustomerId(request.CustomerId);
    var warehouseId = new WarehouseId(request.WarehouseId);
    var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == customerId, ct);
    if (customer is null)
      return CustomerErrors.NotFound;
    if (!customer.IsActive)
      return CustomerErrors.InactiveCustomer;

    var warehouse = await context.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId, ct);
    if (warehouse is null)
      return WarehouseErrors.NotFound;
    var operationalResult = warehouse.EnsureOperational();
    if (operationalResult.IsError)
      return operationalResult.Errors;

    var order = SalesOrder.Create(
        new TenantId(currentUser.TenantId.Value), customerId, new UserId(currentUser.UserId.Value), warehouseId);

    foreach (var input in request.Items)
    {
      var productId = new ProductId(input.ProductId);
      var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
      if (product is null)
        return ProductErrors.NotFound;

      var sellableResult = product.EnsureSellable();
      if (sellableResult.IsError)
        return sellableResult.Errors;
      if (input.UnitPrice < product.Cost.Amount)
        return Error.Conflict("Sales.PriceBelowCost", $"سعر بيع {product.Name} أقل من التكلفة.");

      var quantityResult = Quantity.Create(input.Quantity);
      if (quantityResult.IsError)
        return quantityResult.Errors;
      var priceResult = Money.EGP(input.UnitPrice);
      if (priceResult.IsError)
        return priceResult.Errors;

      var stockItem = await context.StockItems.FirstOrDefaultAsync(
          stock => stock.WarehouseId == warehouseId && stock.ProductId == productId, ct);
      if (stockItem is null || !stockItem.AvailableQuantity.IsGreaterThanOrEqual(quantityResult.Value))
        return StockItemErrors.InsufficientStock;

      var addItemResult = order.AddItem(productId, quantityResult.Value, priceResult.Value);
      if (addItemResult.IsError)
        return addItemResult.Errors;
    }

    var totalResult = order.CalculateTotal();
    if (totalResult.IsError)
      return totalResult.Errors;

    if (request.IsCreditSale)
    {
      var creditResult = customer.ValidateCredit(totalResult.Value);
      if (creditResult.IsError)
        return creditResult.Errors;
      var increaseBalanceResult = customer.IncreaseBalance(totalResult.Value);
      if (increaseBalanceResult.IsError)
        return increaseBalanceResult.Errors;
    }

    var confirmResult = order.Confirm(immediateSale: true);
    if (confirmResult.IsError)
      return confirmResult.Errors;
    var completeResult = order.Complete();
    if (completeResult.IsError)
      return completeResult.Errors;

    var sequence = await context.Invoices.CountAsync(ct) + 1;
    var invoiceNumberResult = DocumentNumber.Generate("INV", sequence);
    if (invoiceNumberResult.IsError)
      return invoiceNumberResult.Errors;

    var invoiceResult = Invoice.Create(
        new TenantId(currentUser.TenantId.Value), order.Id, customerId,
        invoiceNumberResult.Value, totalResult.Value, DateTimeOffset.UtcNow.AddDays(DefaultInvoiceDueDays));
    if (invoiceResult.IsError)
      return invoiceResult.Errors;

    if (!request.IsCreditSale)
    {
      var paymentResult = invoiceResult.Value.RegisterPayment(totalResult.Value);
      if (paymentResult.IsError)
        return paymentResult.Errors;
    }

    context.SalesOrders.Add(order);
    context.Invoices.Add(invoiceResult.Value);
    try
    {
      await context.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
      return Error.Conflict("Inventory.ConcurrentUpdate", "المخزون تغير أثناء إصدار الفاتورة. أعد تحميل البيانات وحاول مرة أخرى.");
    }

    return invoiceResult.Value.Id.Value;
  }
}