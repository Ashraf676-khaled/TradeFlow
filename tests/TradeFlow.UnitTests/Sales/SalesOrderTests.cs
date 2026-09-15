namespace TradeFlow.Domain.Tests.Sales;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Sales;
using TradeFlow.Domain.Sales.Events;
using Xunit;

public class SalesOrderTests
{
  private static readonly TenantId TestTenant = TenantId.New();
  private static readonly CustomerId TestCustomer = CustomerId.New();
  private static readonly UserId TestSalesRep = UserId.New();
  private static readonly WarehouseId TestWarehouse = WarehouseId.New();
  private static readonly ProductId TestProduct1 = ProductId.New();
  private static readonly ProductId TestProduct2 = ProductId.New();

  private static SalesOrder CreateDraftOrder()
      => SalesOrder.Create(TestTenant, TestCustomer, TestSalesRep, TestWarehouse);

  private static SalesOrder CreateOrderWithOneItem(int quantity = 2, decimal price = 100)
  {
    var order = CreateDraftOrder();
    order.AddItem(TestProduct1, Quantity.Create(quantity).Value, Money.EGP(price).Value);
    return order;
  }

  [Fact]
  public void Create_ShouldStartInDraftStatusWithNoItems()
  {
    var order = CreateDraftOrder();

    Assert.Equal(OrderStatus.Draft, order.Status);
    Assert.Empty(order.Items);
  }

  [Fact]
  public void AddItem_NewProduct_ShouldAddAsNewLine()
  {
    var order = CreateDraftOrder();

    var result = order.AddItem(TestProduct1, Quantity.Create(2).Value, Money.EGP(100).Value);

    Assert.True(result.IsSuccess);
    Assert.Single(order.Items);
    Assert.Equal(2, order.Items.First().Quantity.Value);
  }

  [Fact]
  public void AddItem_SameProductTwice_ShouldMergeQuantitiesNotCreateNewLine()
  {
    var order = CreateDraftOrder();

    order.AddItem(TestProduct1, Quantity.Create(2).Value, Money.EGP(100).Value);
    var result = order.AddItem(TestProduct1, Quantity.Create(3).Value, Money.EGP(100).Value);

    Assert.True(result.IsSuccess);
    Assert.Single(order.Items); // مش 2 سطور
    Assert.Equal(5, order.Items.First().Quantity.Value);
  }

  [Fact]
  public void AddItem_DifferentProducts_ShouldCreateSeparateLines()
  {
    var order = CreateDraftOrder();

    order.AddItem(TestProduct1, Quantity.Create(2).Value, Money.EGP(100).Value);
    order.AddItem(TestProduct2, Quantity.Create(1).Value, Money.EGP(50).Value);

    Assert.Equal(2, order.Items.Count);
  }

  [Fact]
  public void AddItem_AfterConfirm_ShouldFail()
  {
    var order = CreateOrderWithOneItem();
    order.Confirm();

    var result = order.AddItem(TestProduct2, Quantity.Create(1).Value, Money.EGP(50).Value);

    Assert.True(result.IsError);
    Assert.Equal(SalesOrderErrors.NotInDraftStatus, result.TopError);
  }

  [Fact]
  public void RemoveItem_ExistingProduct_ShouldRemoveLine()
  {
    var order = CreateOrderWithOneItem();

    var result = order.RemoveItem(TestProduct1);

    Assert.True(result.IsSuccess);
    Assert.Empty(order.Items);
  }

  [Fact]
  public void RemoveItem_NonExistingProduct_ShouldFail()
  {
    var order = CreateOrderWithOneItem();

    var result = order.RemoveItem(TestProduct2);

    Assert.True(result.IsError);
    Assert.Equal(SalesOrderErrors.ItemNotFound, result.TopError);
  }

  [Fact]
  public void CalculateTotal_WithoutDiscounts_ShouldSumLineItems()
  {
    var order = CreateDraftOrder();
    order.AddItem(TestProduct1, Quantity.Create(2).Value, Money.EGP(100).Value); // 200
    order.AddItem(TestProduct2, Quantity.Create(1).Value, Money.EGP(50).Value);  // 50

    var result = order.CalculateTotal();

    Assert.True(result.IsSuccess);
    Assert.Equal(250, result.Value.Amount);
  }

  [Fact]
  public void CalculateTotal_WithItemLevelDiscount_ShouldApplyOnThatLineOnly()
  {
    var order = CreateDraftOrder();
    order.AddItem(TestProduct1, Quantity.Create(2).Value, Money.EGP(100).Value); // 200
    order.ApplyItemDiscount(TestProduct1, Discount.Create(10).Value); // -> 180

    var result = order.CalculateTotal();

    Assert.True(result.IsSuccess);
    Assert.Equal(180, result.Value.Amount);
  }

  [Fact]
  public void CalculateTotal_WithOrderLevelDiscount_ShouldApplyOnGrandTotal()
  {
    var order = CreateDraftOrder();
    order.AddItem(TestProduct1, Quantity.Create(2).Value, Money.EGP(100).Value); // 200
    order.ApplyOrderDiscount(Discount.Create(10).Value); // -> 180

    var result = order.CalculateTotal();

    Assert.True(result.IsSuccess);
    Assert.Equal(180, result.Value.Amount);
  }

  [Fact]
  public void Confirm_WithItems_ShouldSucceedAndRaiseDomainEvent()
  {
    var order = CreateOrderWithOneItem(quantity: 5);

    var result = order.Confirm();

    Assert.True(result.IsSuccess);
    Assert.Equal(OrderStatus.Confirmed, order.Status);

    var domainEvent = Assert.Single(order.DomainEvents);
    var confirmedEvent = Assert.IsType<SalesOrderConfirmedDomainEvent>(domainEvent);
    Assert.Equal(order.Id.Value, confirmedEvent.SalesOrderId);
    Assert.Single(confirmedEvent.Items);
    Assert.Equal(5, confirmedEvent.Items.First().Quantity);
  }

  [Fact]
  public void Confirm_WithEmptyOrder_ShouldFail()
  {
    var order = CreateDraftOrder();

    var result = order.Confirm();

    Assert.True(result.IsError);
    Assert.Equal(SalesOrderErrors.EmptyOrder, result.TopError);
    Assert.Empty(order.DomainEvents);
  }

  [Fact]
  public void Confirm_WhenAlreadyConfirmed_ShouldFail()
  {
    var order = CreateOrderWithOneItem();
    order.Confirm();

    var result = order.Confirm();

    Assert.True(result.IsError);
    Assert.Equal(SalesOrderErrors.NotInDraftStatus, result.TopError);
  }

  [Fact]
  public void Cancel_WhileDraft_ShouldSucceedWithoutRaisingEvent()
  {
    var order = CreateOrderWithOneItem();

    var result = order.Cancel();

    Assert.True(result.IsSuccess);
    Assert.Equal(OrderStatus.Cancelled, order.Status);
    Assert.Empty(order.DomainEvents); // مفيش حجز كان موجود أصلاً
  }

  [Fact]
  public void Cancel_AfterConfirm_ShouldSucceedAndRaiseCancelledEvent()
  {
    var order = CreateOrderWithOneItem();
    order.Confirm();
    order.ClearDomainEvents(); // نمسح الـConfirmed event عشان نركز على Cancel

    var result = order.Cancel();

    Assert.True(result.IsSuccess);
    Assert.Equal(OrderStatus.Cancelled, order.Status);

    var domainEvent = Assert.Single(order.DomainEvents);
    Assert.IsType<SalesOrderCancelledDomainEvent>(domainEvent);
  }

  [Fact]
  public void Cancel_CompletedOrder_ShouldFail()
  {
    var order = CreateOrderWithOneItem();
    order.Confirm();
    order.Complete();

    var result = order.Cancel();

    Assert.True(result.IsError);
    Assert.Equal(SalesOrderErrors.CannotCancelCompletedOrder, result.TopError);
  }

  [Fact]
  public void Complete_ConfirmedOrder_ShouldSucceed()
  {
    var order = CreateOrderWithOneItem();
    order.Confirm();

    var result = order.Complete();

    Assert.True(result.IsSuccess);
    Assert.Equal(OrderStatus.Completed, order.Status);
  }

  [Fact]
  public void Complete_DraftOrder_ShouldFail()
  {
    var order = CreateOrderWithOneItem();

    var result = order.Complete();

    Assert.True(result.IsError);
    Assert.Equal(SalesOrderErrors.NotConfirmed, result.TopError);
  }
}
