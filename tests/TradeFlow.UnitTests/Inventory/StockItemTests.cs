namespace TradeFlow.Domain.Tests.Inventory;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;
using Xunit;

public class StockItemTests
{
  private static readonly TenantId TestTenant = TenantId.New();
  private static readonly WarehouseId TestWarehouse = WarehouseId.New();
  private static readonly ProductId TestProduct = ProductId.New();

  private static StockItem CreateEmptyStockItem()
      => StockItem.Create(TestTenant, TestWarehouse, TestProduct).Value;

  private static StockItem CreateStockItemWithAvailable(int quantity)
  {
    var stockItem = CreateEmptyStockItem();
    stockItem.ReceiveStock(Quantity.Create(quantity).Value);
    return stockItem;
  }

  [Fact]
  public void Create_ShouldStartWithZeroQuantities()
  {
    var stockItem = CreateEmptyStockItem();

    Assert.Equal(0, stockItem.AvailableQuantity.Value);
    Assert.Equal(0, stockItem.ReservedQuantity.Value);
  }

  [Fact]
  public void ReceiveStock_ShouldIncreaseAvailableQuantity()
  {
    var stockItem = CreateEmptyStockItem();
    var quantity = Quantity.Create(50).Value;

    var result = stockItem.ReceiveStock(quantity);

    Assert.True(result.IsSuccess);
    Assert.Equal(50, stockItem.AvailableQuantity.Value);
  }

  [Fact]
  public void ReceiveStock_CalledMultipleTimes_ShouldAccumulate()
  {
    var stockItem = CreateEmptyStockItem();

    stockItem.ReceiveStock(Quantity.Create(30).Value);
    stockItem.ReceiveStock(Quantity.Create(20).Value);

    Assert.Equal(50, stockItem.AvailableQuantity.Value);
  }

  [Fact]
  public void Reserve_WithSufficientStock_ShouldMoveFromAvailableToReserved()
  {
    var stockItem = CreateStockItemWithAvailable(50);
    var quantity = Quantity.Create(10).Value;

    var result = stockItem.Reserve(quantity);

    Assert.True(result.IsSuccess);
    Assert.Equal(40, stockItem.AvailableQuantity.Value);
    Assert.Equal(10, stockItem.ReservedQuantity.Value);
  }

  [Fact]
  public void Reserve_WithInsufficientStock_ShouldFail()
  {
    var stockItem = CreateStockItemWithAvailable(5);
    var quantity = Quantity.Create(10).Value;

    var result = stockItem.Reserve(quantity);

    Assert.True(result.IsError);
    Assert.Equal(StockItemErrors.InsufficientStock, result.TopError);
    Assert.Equal(5, stockItem.AvailableQuantity.Value); // متأثرتش
  }

  [Fact]
  public void Reserve_WithExactAvailableQuantity_ShouldSucceedAndZeroOutAvailable()
  {
    var stockItem = CreateStockItemWithAvailable(10);
    var quantity = Quantity.Create(10).Value;

    var result = stockItem.Reserve(quantity);

    Assert.True(result.IsSuccess);
    Assert.Equal(0, stockItem.AvailableQuantity.Value);
    Assert.Equal(10, stockItem.ReservedQuantity.Value);
  }

  [Fact]
  public void ReleaseReservation_WithValidQuantity_ShouldMoveBackToAvailable()
  {
    var stockItem = CreateStockItemWithAvailable(50);
    stockItem.Reserve(Quantity.Create(20).Value);

    var result = stockItem.ReleaseReservation(Quantity.Create(10).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(40, stockItem.AvailableQuantity.Value);
    Assert.Equal(10, stockItem.ReservedQuantity.Value);
  }

  [Fact]
  public void ReleaseReservation_MoreThanReserved_ShouldFail()
  {
    var stockItem = CreateStockItemWithAvailable(50);
    stockItem.Reserve(Quantity.Create(10).Value);

    var result = stockItem.ReleaseReservation(Quantity.Create(20).Value);

    Assert.True(result.IsError);
    Assert.Equal(StockItemErrors.CannotReleaseMoreThanReserved, result.TopError);
  }

  [Fact]
  public void ConfirmDeduction_WithValidQuantity_ShouldReduceReservedOnly()
  {
    var stockItem = CreateStockItemWithAvailable(50);
    stockItem.Reserve(Quantity.Create(20).Value);

    var result = stockItem.ConfirmDeduction(Quantity.Create(20).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(0, stockItem.ReservedQuantity.Value);
    Assert.Equal(30, stockItem.AvailableQuantity.Value); // متأثرتش، اتخصمت وقت الـReserve
  }

  [Fact]
  public void ConfirmDeduction_MoreThanReserved_ShouldFail()
  {
    var stockItem = CreateStockItemWithAvailable(50);
    stockItem.Reserve(Quantity.Create(10).Value);

    var result = stockItem.ConfirmDeduction(Quantity.Create(20).Value);

    Assert.True(result.IsError);
    Assert.Equal(StockItemErrors.CannotDeductMoreThanReserved, result.TopError);
  }

  [Fact]
  public void TransferOut_WithSufficientStock_ShouldReduceAvailable()
  {
    var stockItem = CreateStockItemWithAvailable(50);

    var result = stockItem.TransferOut(Quantity.Create(15).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(35, stockItem.AvailableQuantity.Value);
  }

  [Fact]
  public void TransferOut_WithInsufficientStock_ShouldFail()
  {
    var stockItem = CreateStockItemWithAvailable(10);

    var result = stockItem.TransferOut(Quantity.Create(15).Value);

    Assert.True(result.IsError);
    Assert.Equal(StockItemErrors.InsufficientStock, result.TopError);
  }

  [Fact]
  public void FullLifecycle_ReceiveReserveConfirm_ShouldMaintainCorrectTotals()
  {
    var stockItem = CreateEmptyStockItem();

    stockItem.ReceiveStock(Quantity.Create(100).Value);
    stockItem.Reserve(Quantity.Create(30).Value);
    stockItem.ConfirmDeduction(Quantity.Create(30).Value);

    // البضاعة خرجت فعلياً، الـTotal دلوقتي 70 مش 100
    Assert.Equal(70, stockItem.AvailableQuantity.Value);
    Assert.Equal(0, stockItem.ReservedQuantity.Value);
  }
}
