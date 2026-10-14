namespace TradeFlow.Application.UnitTests.Inventory.StockItems.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Inventory.StockItems;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Inventory;
using TradeFlow.Infrastructure.Data;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Documents the exact SQL Server SQL produced for the stock-item queries.
/// Guards against the two known performance regressions:
/// 1) Value-object comparisons translating into CONVERT() around the COLUMN
///    (which would make the seek on IX_StockItems_WarehouseId_ProductId unusable).
/// 2) A non-index-backed ORDER BY in the warehouse pagination query.
/// </summary>
public class StockItemQueryTranslationTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };
  private readonly IMapper _mapper = TestMapperFactory.Create();
  private readonly ITestOutputHelper _output;

  public StockItemQueryTranslationTests(ITestOutputHelper output) => _output = output;

  // ToQueryString() only translates the query — the connection is never opened,
  // so a dummy SQL Server connection string is safe here.
  private AppDbContext CreateSqlServerContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=(translation-only);Database=(translation-only);")
        .Options;
    return new AppDbContext(options, _currentUser);
  }

  [Fact]
  public void GetStockItemQuery_TranslatesToParameterizedSeek_WithoutConvertOnColumns()
  {
    var productId = Guid.NewGuid();
    var warehouseId = Guid.NewGuid();

    using var context = CreateSqlServerContext();

    var sql = context.StockItems
        .Where(s => s.ProductId == new ProductId(productId)
                 && s.WarehouseId == new WarehouseId(warehouseId))
        .ProjectTo<StockItemDto>(_mapper.ConfigurationProvider)
        .ToQueryString();

    _output.WriteLine("GetStockItem SQL:\n" + sql);

    // The value objects MUST be parameterized as raw Guids — a CONVERT()/CAST()
    // wrapping [s].[ProductId] or [s].[WarehouseId] would kill the index seek.
    Assert.DoesNotContain("CONVERT(", sql, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("CAST(", sql, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("WHERE", sql, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("[s].[ProductId] =", sql, StringComparison.Ordinal);
    Assert.Contains("[s].[WarehouseId] =", sql, StringComparison.Ordinal);
  }

  [Fact]
  public void GetStockByWarehouseQuery_OrdersByIndexedColumns_WithoutConvertOnColumns()
  {
    var warehouseId = Guid.NewGuid();

    using var context = CreateSqlServerContext();

    var sql = context.StockItems
        .Where(s => s.WarehouseId == new WarehouseId(warehouseId))
        .OrderBy(s => s.ProductId)
        .ThenBy(s => s.Id)
        .ProjectTo<StockItemDto>(_mapper.ConfigurationProvider)
        .ToQueryString();

    _output.WriteLine("GetStockByWarehouse SQL:\n" + sql);

    Assert.DoesNotContain("CONVERT(", sql, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("CAST(", sql, StringComparison.OrdinalIgnoreCase);
    // Ordering must start with the indexed column so SQL Server can satisfy it
    // straight from IX_StockItems_WarehouseId_ProductId (seek + ordered scan).
    Assert.Contains("ORDER BY [s].[ProductId], [s].[Id]", sql, StringComparison.Ordinal);
    Assert.Contains("[s].[WarehouseId] =", sql, StringComparison.Ordinal);
  }

  [Fact]
  public void StockItems_Model_ExposesUniqueCompositeIndexOnWarehouseAndProduct()
  {
    using var context = CreateSqlServerContext();

    var entityType = context.Model.FindEntityType(typeof(StockItem))!;
    var index = entityType.GetIndexes()
        .SingleOrDefault(i => i.Properties.Select(p => p.Name)
            .SequenceEqual(new[] { nameof(StockItem.WarehouseId), nameof(StockItem.ProductId) }));

    Assert.NotNull(index);
    Assert.True(index!.IsUnique);
  }
}
