namespace TradeFlow.Domain.UnitTests.Inventory;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class WarehouseTests
{
  private static readonly TenantId TestTenant = TenantId.New();

  private static Warehouse CreateValidWarehouse()
      => Warehouse.Create(TestTenant, "Main Warehouse", "Cairo, 10th of Ramadan").Value;

  [Fact]
  public void Create_WithValidData_ShouldSucceed()
  {
    var result = Warehouse.Create(TestTenant, "Main Warehouse", "Cairo");

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.IsActive);
    Assert.Equal("Main Warehouse", result.Value.Name);
  }

  [Fact]
  public void Create_WithEmptyName_ShouldFail()
  {
    var result = Warehouse.Create(TestTenant, "", "Cairo");

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NameRequired, result.TopError);
  }

  [Fact]
  public void Create_WithNameExceedingMaxLength_ShouldFail()
  {
    var longName = new string('A', 151);

    var result = Warehouse.Create(TestTenant, longName, "Cairo");

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NameTooLong, result.TopError);
  }

  [Fact]
  public void Create_WithEmptyLocation_ShouldFail()
  {
    var result = Warehouse.Create(TestTenant, "Main Warehouse", "");

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.LocationRequired, result.TopError);
  }

  [Fact]
  public void Rename_WithValidName_ShouldSucceed()
  {
    var warehouse = CreateValidWarehouse();

    var result = warehouse.Rename("Secondary Warehouse");

    Assert.True(result.IsSuccess);
    Assert.Equal("Secondary Warehouse", warehouse.Name);
  }

  [Fact]
  public void Rename_WithEmptyName_ShouldFail()
  {
    var warehouse = CreateValidWarehouse();

    var result = warehouse.Rename("");

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NameRequired, result.TopError);
  }

  [Fact]
  public void ChangeLocation_WithValidLocation_ShouldSucceed()
  {
    var warehouse = CreateValidWarehouse();

    var result = warehouse.ChangeLocation("Alexandria");

    Assert.True(result.IsSuccess);
    Assert.Equal("Alexandria", warehouse.Location);
  }

  [Fact]
  public void ChangeLocation_WithEmptyLocation_ShouldFail()
  {
    var warehouse = CreateValidWarehouse();

    var result = warehouse.ChangeLocation("");

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.LocationRequired, result.TopError);
  }

  [Fact]
  public void Deactivate_WhenActive_ShouldSucceed()
  {
    var warehouse = CreateValidWarehouse();

    var result = warehouse.Deactivate();

    Assert.True(result.IsSuccess);
    Assert.False(warehouse.IsActive);
  }

  [Fact]
  public void Deactivate_WhenAlreadyInactive_ShouldFail()
  {
    var warehouse = CreateValidWarehouse();
    warehouse.Deactivate();

    var result = warehouse.Deactivate();

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.AlreadyInactive, result.TopError);
  }

  [Fact]
  public void Activate_WhenInactive_ShouldSucceed()
  {
    var warehouse = CreateValidWarehouse();
    warehouse.Deactivate();

    var result = warehouse.Activate();

    Assert.True(result.IsSuccess);
    Assert.True(warehouse.IsActive);
  }

  [Fact]
  public void Activate_WhenAlreadyActive_ShouldFail()
  {
    var warehouse = CreateValidWarehouse();

    var result = warehouse.Activate();

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.AlreadyActive, result.TopError);
  }

  [Fact]
  public void EnsureOperational_WhenActive_ShouldSucceed()
  {
    var warehouse = CreateValidWarehouse();

    var result = warehouse.EnsureOperational();

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void EnsureOperational_WhenInactive_ShouldFail()
  {
    var warehouse = CreateValidWarehouse();
    warehouse.Deactivate();

    var result = warehouse.EnsureOperational();

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NotOperational, result.TopError);
  }
}
