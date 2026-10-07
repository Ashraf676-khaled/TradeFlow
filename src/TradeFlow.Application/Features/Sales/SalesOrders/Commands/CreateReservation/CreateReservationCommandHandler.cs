namespace TradeFlow.Application.Sales.SalesOrders.Commands.CreateReservation;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Customers;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Sales;

public sealed class CreateReservationCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateReservationCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateReservationCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null || currentUser.UserId is null)
      return Error.Unauthorized("Auth.NoTenant", "Unable to determine the current user or company.");
    if (request.Items.Count == 0)
      return SalesOrderErrors.EmptyOrder;

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

    var reserveResult = order.Confirm();
    if (reserveResult.IsError)
      return reserveResult.Errors;

    context.SalesOrders.Add(order);
    try
    {
      await context.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
      return Error.Conflict("Inventory.ConcurrentUpdate", "المخزون تغير أثناء الحجز. أعد تحميل البيانات وحاول مرة أخرى.");
    }

    return order.Id.Value;
  }
}