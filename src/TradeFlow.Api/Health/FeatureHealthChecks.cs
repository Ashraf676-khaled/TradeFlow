using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TradeFlow.Infrastructure.Data;

namespace TradeFlow.Api.Health;

public static class FeatureHealthChecks
{
  public static IHealthChecksBuilder AddTradeFlowFeatureHealthChecks(
      this IHealthChecksBuilder builder)
  {
    builder.Add(CreateRegistration("authentication", "Authentication", [
      new("users", (db, token) => CanQueryAsync(db.Users, token)),
      new("tenants", (db, token) => CanQueryAsync(db.Tenants, token)),
      new("refresh-tokens", (db, token) => CanQueryAsync(db.RefreshTokens, token))
    ]));

    builder.Add(CreateRegistration("customers", "Customers", [
      new("customers", (db, token) => CanQueryAsync(db.Customers, token))
    ]));

    builder.Add(CreateRegistration("expenses", "Expenses", [
      new("expenses", (db, token) => CanQueryAsync(db.Expenses, token))
    ]));

    builder.Add(CreateRegistration("inventory", "Inventory", [
      new("products", (db, token) => CanQueryAsync(db.Products, token)),
      new("warehouses", (db, token) => CanQueryAsync(db.Warehouses, token)),
      new("stock-items", (db, token) => CanQueryAsync(db.StockItems, token))
    ]));

    builder.Add(CreateRegistration("sales", "Sales", [
      new("sales-orders", (db, token) => CanQueryAsync(db.SalesOrders, token)),
      new("invoices", (db, token) => CanQueryAsync(db.Invoices, token))
    ]));

    builder.Add(CreateRegistration("purchasing", "Purchasing", [
      new("suppliers", (db, token) => CanQueryAsync(db.Suppliers, token)),
      new("purchase-orders", (db, token) => CanQueryAsync(db.PurchaseOrders, token))
    ]));

    builder.Add(CreateRegistration("settings", "Settings", [
      new("system-settings", (db, token) => CanQueryAsync(db.SystemSettings, token))
    ]));

    return builder;
  }

  private static HealthCheckRegistration CreateRegistration(
      string name,
      string displayName,
      FeatureComponent[] components)
  {
    return new HealthCheckRegistration(
        name,
        serviceProvider => new FeatureHealthCheck(
            serviceProvider.GetRequiredService<AppDbContext>(), displayName, components),
        HealthStatus.Unhealthy,
        ["ready"]);
  }

  private static async Task CanQueryAsync<TEntity>(
      IQueryable<TEntity> query,
      CancellationToken cancellationToken)
      where TEntity : class
  {
    await query.IgnoreQueryFilters().AnyAsync(cancellationToken);
  }

  private sealed record FeatureComponent(
      string Name,
      Func<AppDbContext, CancellationToken, Task> Check);

  private sealed class FeatureHealthCheck(
      AppDbContext dbContext,
      string feature,
      IReadOnlyList<FeatureComponent> components) : IHealthCheck
  {
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
      var componentResults = new List<object>(components.Count);
      var failedComponents = new List<string>();

      foreach (var component in components)
      {
        var startedAt = System.Diagnostics.Stopwatch.GetTimestamp();
        var status = "Healthy";

        try
        {
          await component.Check(dbContext, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
          throw;
        }
        catch (Exception)
        {
          status = "Unhealthy";
          failedComponents.Add(component.Name);
        }

        componentResults.Add(new
        {
          name = component.Name,
          status,
          durationMs = Math.Round(
              System.Diagnostics.Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 2)
        });
      }

      var data = new Dictionary<string, object>
      {
        ["feature"] = feature,
        ["components"] = componentResults,
        ["healthyComponents"] = components.Count - failedComponents.Count,
        ["unhealthyComponents"] = failedComponents.Count
      };

      return failedComponents.Count == 0
          ? HealthCheckResult.Healthy($"{feature} feature tables are reachable.", data: data)
          : HealthCheckResult.Unhealthy(
              $"{feature} feature has unavailable components.", data: data);
    }
  }
}