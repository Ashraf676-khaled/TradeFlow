namespace TradeFlow.Application.UnitTests.Purchasing.PurchaseOrders.Commands;

using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.ApprovePurchaseOrder;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CancelPurchaseOrder;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CreatePurchaseOrder;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.SubmitPurchaseOrder;
using TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Purchasing;
using Xunit;

public class PurchaseOrderFullFlowTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task FullFlow_DraftToApproved_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var supplierId = (await new CreateSupplierCommandHandler(context, _currentUser)
        .Handle(new CreateSupplierCommand("Al-Amal Trading", "01012345678"), CancellationToken.None)).Value;
    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;
    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var orderId = (await new CreatePurchaseOrderCommandHandler(context, _currentUser)
        .Handle(new CreatePurchaseOrderCommand(supplierId, warehouseId,
            [new PurchaseOrderItemInput(productId, 10, 40)]), CancellationToken.None)).Value;

    var submitResult = await new SubmitPurchaseOrderCommandHandler(context)
        .Handle(new SubmitPurchaseOrderCommand(orderId), CancellationToken.None);
    Assert.True(submitResult.IsSuccess);

    var approveResult = await new ApprovePurchaseOrderCommandHandler(context)
        .Handle(new ApprovePurchaseOrderCommand(orderId), CancellationToken.None);
    Assert.True(approveResult.IsSuccess);
  }

  [Fact]
  public async Task ApproveBeforeSubmit_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var supplierId = (await new CreateSupplierCommandHandler(context, _currentUser)
        .Handle(new CreateSupplierCommand("Al-Amal Trading", "01012345678"), CancellationToken.None)).Value;
    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;
    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var orderId = (await new CreatePurchaseOrderCommandHandler(context, _currentUser)
        .Handle(new CreatePurchaseOrderCommand(supplierId, warehouseId,
            [new PurchaseOrderItemInput(productId, 10, 40)]), CancellationToken.None)).Value;

    var result = await new ApprovePurchaseOrderCommandHandler(context)
        .Handle(new ApprovePurchaseOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(PurchaseOrderErrors.NotSubmitted, result.TopError);
  }

  [Fact]
  public async Task Cancel_AfterReceiving_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var supplierId = (await new CreateSupplierCommandHandler(context, _currentUser)
        .Handle(new CreateSupplierCommand("Al-Amal Trading", "01012345678"), CancellationToken.None)).Value;
    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;
    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var orderId = (await new CreatePurchaseOrderCommandHandler(context, _currentUser)
        .Handle(new CreatePurchaseOrderCommand(supplierId, warehouseId,
            [new PurchaseOrderItemInput(productId, 10, 40)]), CancellationToken.None)).Value;

    await new SubmitPurchaseOrderCommandHandler(context)
        .Handle(new SubmitPurchaseOrderCommand(orderId), CancellationToken.None);
    await new ApprovePurchaseOrderCommandHandler(context)
        .Handle(new ApprovePurchaseOrderCommand(orderId), CancellationToken.None);

    var order = context.PurchaseOrders.First(o => o.Id.Value == orderId);
    order.ReceiveItem(new TradeFlow.Domain.Common.Identifiers.ProductId(productId),
        TradeFlow.Domain.Common.ValueObjects.Quantity.Create(10).Value);
    await context.SaveChangesAsync(CancellationToken.None);

    var result = await new CancelPurchaseOrderCommandHandler(context)
        .Handle(new CancelPurchaseOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(PurchaseOrderErrors.CannotCancelReceivedOrder, result.TopError);
  }
}
