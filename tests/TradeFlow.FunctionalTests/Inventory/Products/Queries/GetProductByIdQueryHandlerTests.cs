namespace TradeFlow.Application.UnitTests.Inventory.Products.Queries;

using AutoMapper;
using TradeFlow.Application.Common.Mappings;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Products.Queries.GetProductById;
using Microsoft.Extensions.Logging.Abstractions;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class GetProductByIdQueryHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };
  private readonly IMapper _mapper;

  public GetProductByIdQueryHandlerTests()
  {
    var config = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingProfile>(),
        NullLoggerFactory.Instance);

    _mapper = config.CreateMapper();
  }

  [Fact]
  public async Task Handle_WithExistingProduct_ShouldReturnDto()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var createResult = await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None);

    var handler = new GetProductByIdQueryHandler(context, _mapper);
    var result = await handler.Handle(new GetProductByIdQuery(createResult.Value), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("Door", result.Value.Name);
    Assert.Equal("D-001", result.Value.Sku);
  }

  [Fact]
  public async Task Handle_WithNonExistingProduct_ShouldReturnNotFound()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new GetProductByIdQueryHandler(context, _mapper);

    var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.NotFound, result.TopError);
  }
}
