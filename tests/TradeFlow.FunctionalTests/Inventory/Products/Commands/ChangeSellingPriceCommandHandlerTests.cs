namespace TradeFlow.Application.UnitTests.Inventory.Products.Commands;

using TradeFlow.Application.Inventory.Products.Commands.ChangeSellingPrice;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class ChangeSellingPriceCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  private async Task<Guid> CreateProductAsync(TradeFlow.Infrastructure.Data.AppDbContext context)
  {
    var createHandler = new CreateProductCommandHandler(context, _currentUser);
    var result = await createHandler.Handle(
        new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None);
    return result.Value;
  }

  [Fact]
  public async Task Handle_WithValidPrice_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var productId = await CreateProductAsync(context);

    var handler = new ChangeSellingPriceCommandHandler(context);
    var result = await handler.Handle(new ChangeSellingPriceCommand(productId, 150), CancellationToken.None);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task Handle_WithPriceLowerThanCost_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var productId = await CreateProductAsync(context);

    var handler = new ChangeSellingPriceCommandHandler(context);
    var result = await handler.Handle(new ChangeSellingPriceCommand(productId, 10), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.CostExceedsSellingPrice, result.TopError);
  }

  [Fact]
  public async Task Handle_WithNonExistingProduct_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new ChangeSellingPriceCommandHandler(context);

    var result = await handler.Handle(new ChangeSellingPriceCommand(Guid.NewGuid(), 150), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.NotFound, result.TopError);
  }
}
