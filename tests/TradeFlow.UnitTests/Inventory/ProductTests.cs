namespace TradeFlow.Domain.Tests.Inventory;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;
using Xunit;

public class ProductTests
{
  private static readonly TenantId TestTenant = TenantId.New();

  private static Product CreateValidProduct(decimal price = 100, decimal cost = 60)
  {
    var sku = Sku.Create("PRD-0001").Value;
    var sellingPrice = Money.EGP(price).Value;
    var initialCost = Money.EGP(cost).Value;

    return Product.Create(TestTenant, "iPhone 15", sku, sellingPrice, initialCost, 5).Value;
  }

  [Fact]
  public void Create_WithValidData_ShouldSucceed()
  {
    var sku = Sku.Create("PRD-0001").Value;
    var price = Money.EGP(100).Value;
    var cost = Money.EGP(60).Value;

    var result = Product.Create(TestTenant, "iPhone 15", sku, price, cost, 5);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.IsActive);
    Assert.Equal("iPhone 15", result.Value.Name);
  }

  [Fact]
  public void Create_WithEmptyName_ShouldFail()
  {
    var sku = Sku.Create("PRD-0001").Value;
    var price = Money.EGP(100).Value;
    var cost = Money.EGP(60).Value;

    var result = Product.Create(TestTenant, "", sku, price, cost, 5);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.NameRequired, result.TopError);
  }

  [Fact]
  public void Create_WithZeroSellingPrice_ShouldFail()
  {
    var sku = Sku.Create("PRD-0001").Value;
    var price = Money.Zero();
    var cost = Money.EGP(60).Value;

    var result = Product.Create(TestTenant, "iPhone 15", sku, price, cost, 5);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.InvalidSellingPrice, result.TopError);
  }

  [Fact]
  public void Create_WithNegativeMinimumStock_ShouldFail()
  {
    var sku = Sku.Create("PRD-0001").Value;
    var price = Money.EGP(100).Value;
    var cost = Money.EGP(60).Value;

    var result = Product.Create(TestTenant, "iPhone 15", sku, price, cost, -1);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.InvalidMinimumStock, result.TopError);
  }

  [Fact]
  public void ChangeSellingPrice_WithValidPrice_ShouldSucceed()
  {
    var product = CreateValidProduct();
    var newPrice = Money.EGP(150).Value;

    var result = product.ChangeSellingPrice(newPrice);

    Assert.True(result.IsSuccess);
    Assert.Equal(150, product.SellingPrice.Amount);
  }

  [Fact]
  public void ChangeSellingPrice_WithZeroPrice_ShouldFail()
  {
    var product = CreateValidProduct();

    var result = product.ChangeSellingPrice(Money.Zero());

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.InvalidSellingPrice, result.TopError);
  }

  [Fact]
  public void ApplyPurchaseCost_WithValidCost_ShouldUpdateCost()
  {
    var product = CreateValidProduct();
    var newCost = Money.EGP(70).Value;

    var result = product.ApplyPurchaseCost(newCost);

    Assert.True(result.IsSuccess);
    Assert.Equal(70, product.Cost.Amount);
  }

  [Fact]
  public void ApplyPurchaseCost_WithZeroCost_ShouldFail()
  {
    var product = CreateValidProduct();

    var result = product.ApplyPurchaseCost(Money.Zero());

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.InvalidCost, result.TopError);
  }

  [Fact]
  public void Deactivate_WhenActive_ShouldSucceed()
  {
    var product = CreateValidProduct();

    var result = product.Deactivate();

    Assert.True(result.IsSuccess);
    Assert.False(product.IsActive);
  }

  [Fact]
  public void Deactivate_WhenAlreadyInactive_ShouldFail()
  {
    var product = CreateValidProduct();
    product.Deactivate();

    var result = product.Deactivate();

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.AlreadyInactive, result.TopError);
  }

  [Fact]
  public void Activate_WhenInactive_ShouldSucceed()
  {
    var product = CreateValidProduct();
    product.Deactivate();

    var result = product.Activate();

    Assert.True(result.IsSuccess);
    Assert.True(product.IsActive);
  }

  [Fact]
  public void Activate_WhenAlreadyActive_ShouldFail()
  {
    var product = CreateValidProduct();

    var result = product.Activate();

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.AlreadyActive, result.TopError);
  }

  [Fact]
  public void EnsureSellable_WhenActive_ShouldSucceed()
  {
    var product = CreateValidProduct();

    var result = product.EnsureSellable();

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void EnsureSellable_WhenInactive_ShouldFail()
  {
    var product = CreateValidProduct();
    product.Deactivate();

    var result = product.EnsureSellable();

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.NotSellable, result.TopError);
  }
}
