namespace TradeFlow.Application.UnitTests.Inventory.Products.Commands;

using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class CreateProductCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithValidData_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateProductCommandHandler(context, _currentUser);

    var command = new CreateProductCommand("Door", "D-001", 100, 50, 5);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(context.Products);
  }

  [Fact]
  public async Task Handle_WithDuplicateSku_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateProductCommandHandler(context, _currentUser);

    await handler.Handle(new CreateProductCommand("Door1", "D-001", 100, 50, 5), CancellationToken.None);
    var result = await handler.Handle(new CreateProductCommand("Door2", "D-001", 200, 100, 5), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.SkuAlreadyExists, result.TopError);
  }

  [Fact]
  public async Task Handle_WithCostGreaterThanSellingPrice_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateProductCommandHandler(context, _currentUser);

    var command = new CreateProductCommand("Window", "W-001", 50, 100, 5);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.CostExceedsSellingPrice, result.TopError);
  }

  [Fact]
  public async Task Handle_WithoutTenant_ShouldFail()
  {
    var noTenantUser = new FakeCurrentUserService { TenantId = null };
    await using var context = TestDbContextFactory.Create(noTenantUser);
    var handler = new CreateProductCommandHandler(context, noTenantUser);

    var result = await handler.Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None);

    Assert.True(result.IsError);
  }
}
