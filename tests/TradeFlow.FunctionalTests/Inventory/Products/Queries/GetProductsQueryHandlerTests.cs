namespace TradeFlow.Application.UnitTests.Inventory.Products.Queries;

using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using TradeFlow.Application.Common.Mappings;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Products.Commands.DeactivateProduct;
using TradeFlow.Application.Inventory.Products.Queries.GetProducts;
using TradeFlow.Application.UnitTests.Helpers;
using Xunit;

public class GetProductsQueryHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };
  private readonly IMapper _mapper;

  public GetProductsQueryHandlerTests()
  {
    var config = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingProfile>(),
        NullLoggerFactory.Instance);

    _mapper = config.CreateMapper();
  }

  [Fact]
  public async Task Handle_ShouldReturnAllProducts_WhenNoFilterApplied()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateProductCommandHandler(context, _currentUser);

    await createHandler.Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None);
    await createHandler.Handle(new CreateProductCommand("Window", "W-001", 200, 100, 5), CancellationToken.None);

    var handler = new GetProductsQueryHandler(context, _mapper);
    var result = await handler.Handle(new GetProductsQuery(1, 20, null), CancellationToken.None);

    Assert.Equal(2, result.TotalCount);
  }

  [Fact]
  public async Task Handle_ShouldFilterByIsActive()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateProductCommandHandler(context, _currentUser);

    var product1 = await createHandler.Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None);
    await createHandler.Handle(new CreateProductCommand("Window", "W-001", 200, 100, 5), CancellationToken.None);

    await new DeactivateProductCommandHandler(context)
        .Handle(new DeactivateProductCommand(product1.Value), CancellationToken.None);

    var handler = new GetProductsQueryHandler(context, _mapper);
    var result = await handler.Handle(new GetProductsQuery(1, 20, true), CancellationToken.None);

    Assert.Single(result.Items);
    Assert.Equal("Window", result.Items[0].Name);
  }

  [Fact]
  public async Task Handle_ShouldRespectPageSize()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateProductCommandHandler(context, _currentUser);

    for (var i = 0; i < 5; i++)
      await createHandler.Handle(new CreateProductCommand($"Product{i}", $"SKU-{i}", 100, 50, 5), CancellationToken.None);

    var handler = new GetProductsQueryHandler(context, _mapper);
    var result = await handler.Handle(new GetProductsQuery(1, 2, null), CancellationToken.None);

    Assert.Equal(2, result.Items.Count);
    Assert.Equal(5, result.TotalCount);
    Assert.Equal(3, result.TotalPages);
  }
}
