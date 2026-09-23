namespace TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Customers;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Sales;

public sealed class CreateSalesOrderCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateSalesOrderCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateSalesOrderCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null || currentUser.UserId is null)
      return Error.Unauthorized("Auth.NoTenant", "Unable to determine the current user or company.");

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

    foreach (var itemInput in request.Items)
    {
      var productId = new ProductId(itemInput.ProductId);

      var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
      if (product is null)
        return ProductErrors.NotFound;

      var sellableResult = product.EnsureSellable();
      if (sellableResult.IsError)
        return sellableResult.Errors;

      var quantityResult = Quantity.Create(itemInput.Quantity);
      if (quantityResult.IsError)
        return quantityResult.Errors;

      var priceResult = Money.EGP(itemInput.UnitPrice);
      if (priceResult.IsError)
        return priceResult.Errors;

      var addItemResult = order.AddItem(productId, quantityResult.Value, priceResult.Value);
      if (addItemResult.IsError)
        return addItemResult.Errors;
    }

    context.SalesOrders.Add(order);
    await context.SaveChangesAsync(ct);

    return order.Id.Value;
  }
}
