namespace TradeFlow.Application.Inventory.Warehouses;

using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Inventory;

/// <summary>
/// Ensures every tenant always has at least one usable warehouse so that
/// warehouse dropdowns and stock-in flows are never blocked by an empty list.
/// The default warehouse is created lazily the first time it is needed.
/// </summary>
public static class DefaultWarehouse
{
  public const string DefaultName = "المستودع الرئيسي";
  public const string DefaultLocation = "المقر الرئيسي";

  /// <summary>
  /// Returns an existing warehouse for the current tenant (preferring an active one),
  /// or creates and persists the default warehouse when the tenant has none.
  /// </summary>
  public static async Task<Warehouse> EnsureAsync(
      IApplicationDbContext context,
      TenantId tenantId,
      CancellationToken ct = default)
  {
    var existing = await context.Warehouses
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .FirstOrDefaultAsync(ct)
        ?? await context.Warehouses
            .OrderBy(w => w.Name)
            .FirstOrDefaultAsync(ct);

    if (existing is not null)
      return existing;

    var result = Warehouse.Create(tenantId, DefaultName, DefaultLocation);
    if (result.IsError)
      throw new InvalidOperationException(
          $"Unable to create the default warehouse: {result.TopError.Description}");

    context.Warehouses.Add(result.Value);
    await context.SaveChangesAsync(ct);

    return result.Value;
  }
}