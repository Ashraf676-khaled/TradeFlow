namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CreatePurchaseOrder;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Purchasing;

public sealed class CreatePurchaseOrderCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "Unable to determine the current company.");

    var supplierId = new SupplierId(request.SupplierId);
    var warehouseId = new WarehouseId(request.WarehouseId);

    var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId, ct);
    if (supplier is null)
      return SupplierErrors.NotFound;

    if (!supplier.IsActive)
      return SupplierErrors.AlreadyInactive;

    var warehouse = await context.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId, ct);
    if (warehouse is null)
      return WarehouseErrors.NotFound;

    var operationalResult = warehouse.EnsureOperational();
    if (operationalResult.IsError)
      return operationalResult.Errors;

    // توليد رقم الأمر تلقائيًا: PO-0001, PO-0002 ...
    var sequence = await context.PurchaseOrders.CountAsync(ct) + 1;
    var orderNumberResult = DocumentNumber.Generate("PO", sequence);
    if (orderNumberResult.IsError)
      return orderNumberResult.Errors;

    var order = PurchaseOrder.Create(
        new TenantId(currentUser.TenantId.Value), supplierId, warehouseId, orderNumberResult.Value);

    foreach (var itemInput in request.Items)
    {
      var productId = new ProductId(itemInput.ProductId);

      var productExists = await context.Products.AnyAsync(p => p.Id == productId, ct);
      if (!productExists)
        return ProductErrors.NotFound;

      var quantityResult = Quantity.Create(itemInput.Quantity);
      if (quantityResult.IsError)
        return quantityResult.Errors;

      var costResult = Money.EGP(itemInput.UnitCost);
      if (costResult.IsError)
        return costResult.Errors;

      var addItemResult = order.AddItem(productId, quantityResult.Value, costResult.Value);
      if (addItemResult.IsError)
        return addItemResult.Errors;
    }

    context.PurchaseOrders.Add(order);
    await context.SaveChangesAsync(ct);

    return order.Id.Value;
  }
}
