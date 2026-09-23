namespace TradeFlow.Infrastructure.BackgroundJobs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Sales;

public class HangfireJobService(
    IApplicationDbContext context,
    ILogger<HangfireJobService> logger) : IHangfireJobService
{
  /// <summary>
  /// يحذف الـRefreshTokens الملغاة أو المنتهية من فترة طويلة (أكثر من 30 يوم)
  /// عشان جدول RefreshTokens ميكبرش من غير داعي بمرور الوقت.
  /// </summary>
  public async Task CleanupExpiredRefreshTokensAsync(CancellationToken ct)
  {
    var cutoffDate = DateTimeOffset.UtcNow.AddDays(-30);

    // Query مباشر على DbSet نفسه — مضاف .IgnoreQueryFilters() للاتساق وضمان الشمولية
    var expiredTokens = await context.RefreshTokens
        .IgnoreQueryFilters()
        .Where(rt => rt.IsRevoked || rt.ExpiresUtc < cutoffDate)
        .ToListAsync(ct);

    if (expiredTokens.Count == 0)
    {
      logger.LogInformation("No expired refresh tokens to clean up.");
      return;
    }

    context.RefreshTokens.RemoveRange(expiredTokens);
    await context.SaveChangesAsync(ct);

    logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens.", expiredTokens.Count);
  }

  /// <summary>
  /// يفحص كل الـStockItems عبر كل الشركات (Tenants) ويسجل تنبيه لأي منتج
  /// وصلت كميته المتاحة تحت الحد الأدنى (MinimumStock) المحدد عليه.
  /// </summary>
  public async Task CheckLowStockLevelsAsync(CancellationToken ct)
  {
    // استخدام .IgnoreQueryFilters() لضمان المرور على كافة الـ Tenants وعدم التأثر بغياب الـ HttpContext
    var lowStockItems = await (
        from stock in context.StockItems.IgnoreQueryFilters()
        join product in context.Products.IgnoreQueryFilters() on stock.ProductId equals product.Id
        where product.IsActive && stock.AvailableQuantity.Value <= product.MinimumStock
        select new
        {
          stock.TenantId,
          stock.WarehouseId,
          stock.ProductId,
          ProductName = product.Name,
          Available = stock.AvailableQuantity.Value,
          product.MinimumStock
        }).ToListAsync(ct);

    if (lowStockItems.Count == 0)
    {
      logger.LogInformation("No low-stock items found.");
      return;
    }

    foreach (var item in lowStockItems)
    {
      logger.LogWarning(
          "Low stock alert: Product '{ProductName}' in warehouse {WarehouseId} " +
          "has {Available} units available (minimum required: {MinimumStock}). Tenant: {TenantId}",
          item.ProductName, item.WarehouseId, item.Available, item.MinimumStock, item.TenantId);
    }

    // TODO: لاحقًا يمكن استبدال الـLogging دي بإرسال Domain Event حقيقي
    // (StockLevelLowDomainEvent الموجودة بالفعل في Domain/Inventory/Events) أو إشعار فعلي
    // للمستخدمين (Email/Push) بدل ما تقتصر على الـLog فقط.
  }

  /// <summary>
  /// يفحص الفواتير غير المدفوعة اللي تجاوزت تاريخ الاستحقاق (DueDate) ويسجلها للمتابعة.
  /// </summary>
  public async Task CheckOverdueInvoicesAsync(CancellationToken ct)
  {
    var now = DateTimeOffset.UtcNow;

    var overdueInvoices = await context.Invoices
        .AsNoTracking() // <-- الحل هنا لمنع مشكلة تتبع الـ Owned Entities
        .IgnoreQueryFilters()
        .Where(i => i.DueDate < now &&
                    (i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.PartiallyPaid))
        .Select(i => new { i.Id, i.TenantId, i.CustomerId, i.DueDate, i.TotalAmount })
        .ToListAsync(ct);

    if (overdueInvoices.Count == 0)
    {
      logger.LogInformation("No overdue invoices found.");
      return;
    }

    foreach (var invoice in overdueInvoices)
    {
      logger.LogWarning(
          "Overdue invoice alert: Invoice {InvoiceId} for customer {CustomerId} was due on {DueDate}. " +
          "Total amount: {TotalAmount}. Tenant: {TenantId}",
          invoice.Id, invoice.CustomerId, invoice.DueDate, invoice.TotalAmount.Amount, invoice.TenantId);
    }
  }
}
