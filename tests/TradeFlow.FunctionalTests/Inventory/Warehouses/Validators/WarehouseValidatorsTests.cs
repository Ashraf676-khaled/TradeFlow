namespace TradeFlow.Application.UnitTests.Inventory.Warehouses.Validators;

using TradeFlow.Application.Inventory.Warehouses.Commands.ChangeWarehouseLocation;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.RenameWarehouse;
using Xunit;

public class CreateWarehouseCommandValidatorTests
{
  private readonly CreateWarehouseCommandValidator _validator = new();

  [Fact]
  public void Validate_WithEmptyName_ShouldHaveError()
  {
    var result = _validator.Validate(new CreateWarehouseCommand("", "Cairo"));
    Assert.False(result.IsValid);
  }

  [Fact]
  public void Validate_WithNameTooLong_ShouldHaveError()
  {
    var result = _validator.Validate(new CreateWarehouseCommand(new string('a', 151), "Cairo"));
    Assert.False(result.IsValid);
  }

  [Fact]
  public void Validate_WithEmptyLocation_ShouldHaveError()
  {
    var result = _validator.Validate(new CreateWarehouseCommand("Main Warehouse", ""));
    Assert.False(result.IsValid);
  }

  [Fact]
  public void Validate_WithValidData_ShouldNotHaveError()
  {
    var result = _validator.Validate(new CreateWarehouseCommand("Main Warehouse", "Cairo"));
    Assert.True(result.IsValid);
  }
}

public class RenameWarehouseCommandValidatorTests
{
  private readonly RenameWarehouseCommandValidator _validator = new();

  [Fact]
  public void Validate_WithEmptyWarehouseId_ShouldHaveError()
  {
    var result = _validator.Validate(new RenameWarehouseCommand(Guid.Empty, "New Name"));
    Assert.False(result.IsValid);
  }

  [Fact]
  public void Validate_WithEmptyNewName_ShouldHaveError()
  {
    var result = _validator.Validate(new RenameWarehouseCommand(Guid.NewGuid(), ""));
    Assert.False(result.IsValid);
  }
}

public class ChangeWarehouseLocationCommandValidatorTests
{
  private readonly ChangeWarehouseLocationCommandValidator _validator = new();

  [Fact]
  public void Validate_WithEmptyNewLocation_ShouldHaveError()
  {
    var result = _validator.Validate(new ChangeWarehouseLocationCommand(Guid.NewGuid(), ""));
    Assert.False(result.IsValid);
  }

  [Fact]
  public void Validate_WithLocationTooLong_ShouldHaveError()
  {
    var result = _validator.Validate(new ChangeWarehouseLocationCommand(Guid.NewGuid(), new string('a', 301)));
    Assert.False(result.IsValid);
  }
}
