namespace TradeFlow.Domain.Tests.Purchasing;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Purchasing;
using TradeFlow.Domain.Purchasing.Events;
using Xunit;

public class PurchaseOrderTests
{
  private static readonly TenantId TestTenant = TenantId.New();
  private static readonly SupplierId TestSupplier = SupplierId.New();
  private static readonly WarehouseId TestWarehouse = WarehouseId.New();
  private static readonly ProductId TestProduct = ProductId.New();

  private static PurchaseOrder CreateDraftOrder()
  {
    var orderNumber = DocumentNumber.Generate("PO", 1).Value;
    return PurchaseOrder.Create(TestTenant, TestSupplier, TestWarehouse, orderNumber);
  }

  private static PurchaseOrder CreateApprovedOrderWithItem(int quantity = 100, decimal unitCost = 50)
  {
    var order = CreateDraftOrder();
    order.AddItem(TestProduct, Quantity.Create(quantity).Value, Money.EGP(unitCost).Value);
    order.Submit();
    order.Approve();
    return order;
  }

  [Fact]
  public void Create_ShouldStartInDraftStatus()
  {
    var order = CreateDraftOrder();

    Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
    Assert.Empty(order.Items);
  }

  [Fact]
  public void AddItem_SameProductTwice_ShouldMergeQuantities()
  {
    var order = CreateDraftOrder();

    order.AddItem(TestProduct, Quantity.Create(50).Value, Money.EGP(50).Value);
    order.AddItem(TestProduct, Quantity.Create(30).Value, Money.EGP(50).Value);

    Assert.Single(order.Items);
    Assert.Equal(80, order.Items.First().OrderedQuantity.Value);
  }

  [Fact]
  public void Submit_WithItems_ShouldSucceed()
  {
    var order = CreateDraftOrder();
    order.AddItem(TestProduct, Quantity.Create(50).Value, Money.EGP(50).Value);

    var result = order.Submit();

    Assert.True(result.IsSuccess);
    Assert.Equal(PurchaseOrderStatus.Submitted, order.Status);
  }

  [Fact]
  public void Submit_EmptyOrder_ShouldFail()
  {
    var order = CreateDraftOrder();

    var result = order.Submit();

    Assert.True(result.IsError);
    Assert.Equal(PurchaseOrderErrors.EmptyOrder, result.TopError);
  }

  [Fact]
  public void Approve_SubmittedOrder_ShouldSucceed()
  {
    var order = CreateDraftOrder();
    order.AddItem(TestProduct, Quantity.Create(50).Value, Money.EGP(50).Value);
    order.Submit();

    var result = order.Approve();

    Assert.True(result.IsSuccess);
    Assert.Equal(PurchaseOrderStatus.Approved, order.Status);
  }

  [Fact]
  public void Approve_DraftOrder_ShouldFail()
  {
    var order = CreateDraftOrder();

    var result = order.Approve();

    Assert.True(result.IsError);
    Assert.Equal(PurchaseOrderErrors.NotSubmitted, result.TopError);
  }

  [Fact]
  public void ReceiveItem_FullQuantity_ShouldCompleteOrderAndRaiseEvent()
  {
    var order = CreateApprovedOrderWithItem(quantity: 100);

    var result = order.ReceiveItem(TestProduct, Quantity.Create(100).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(PurchaseOrderStatus.Completed, order.Status);
    Assert.True(order.Items.First().IsFullyReceived);

    var domainEvent = Assert.Single(order.DomainEvents);
    var receivedEvent = Assert.IsType<PurchaseOrderReceivedDomainEvent>(domainEvent);
    Assert.Equal(100, receivedEvent.Items.First().ReceivedQuantity);
  }

  [Fact]
  public void ReceiveItem_PartialQuantity_ShouldStayInReceivedStatusNotCompleted()
  {
    var order = CreateApprovedOrderWithItem(quantity: 100);

    var result = order.ReceiveItem(TestProduct, Quantity.Create(60).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(PurchaseOrderStatus.Received, order.Status);
    Assert.False(order.Items.First().IsFullyReceived);
  }

  [Fact]
  public void ReceiveItem_MultiplePartialDeliveries_ShouldAccumulateAndComplete()
  {
    var order = CreateApprovedOrderWithItem(quantity: 100);

    order.ReceiveItem(TestProduct, Quantity.Create(60).Value);
    var result = order.ReceiveItem(TestProduct, Quantity.Create(40).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(PurchaseOrderStatus.Completed, order.Status);
    Assert.Equal(100, order.Items.First().ReceivedQuantity.Value);
  }

  [Fact]
  public void ReceiveItem_ExceedingOrderedQuantity_ShouldFail()
  {
    var order = CreateApprovedOrderWithItem(quantity: 100);
    order.ReceiveItem(TestProduct, Quantity.Create(80).Value);

    var result = order.ReceiveItem(TestProduct, Quantity.Create(30).Value); // 80+30=110 > 100

    Assert.True(result.IsError);
    Assert.Equal(PurchaseOrderErrors.ReceivedExceedsOrdered, result.TopError);
  }

  [Fact]
  public void ReceiveItem_BeforeApproval_ShouldFail()
  {
    var order = CreateDraftOrder();
    order.AddItem(TestProduct, Quantity.Create(50).Value, Money.EGP(50).Value);

    var result = order.ReceiveItem(TestProduct, Quantity.Create(10).Value);

    Assert.True(result.IsError);
    Assert.Equal(PurchaseOrderErrors.NotApproved, result.TopError);
  }

  [Fact]
  public void Cancel_DraftOrder_ShouldSucceed()
  {
    var order = CreateDraftOrder();

    var result = order.Cancel();

    Assert.True(result.IsSuccess);
    Assert.Equal(PurchaseOrderStatus.Cancelled, order.Status);
  }

  [Fact]
  public void Cancel_ReceivedOrder_ShouldFail()
  {
    var order = CreateApprovedOrderWithItem(quantity: 100);
    order.ReceiveItem(TestProduct, Quantity.Create(50).Value);

    var result = order.Cancel();

    Assert.True(result.IsError);
    Assert.Equal(PurchaseOrderErrors.CannotCancelReceivedOrder, result.TopError);
  }
}
