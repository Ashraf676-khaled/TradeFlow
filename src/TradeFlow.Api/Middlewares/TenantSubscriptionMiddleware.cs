namespace TradeFlow.Api.Middlewares;

using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Constants;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Infrastructure.Data;

public sealed class TenantSubscriptionMiddleware(
    RequestDelegate next,
    ILogger<TenantSubscriptionMiddleware> logger)
{
  private const string ExpiredSubscriptionMessage =
      "الحساب غير مفعل أو انتهت الفترة التجريبية (30 يوماً)، يرجى التواصل مع الإدارة للتجديد.";

  public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
  {
    if (context.User.Identity?.IsAuthenticated != true)
    {
      await next(context);
      return;
    }

    if (context.User.HasClaim(AppClaimTypes.PlatformAdmin, "true")
        || context.Request.Path.StartsWithSegments("/api/auth"))
    {
      await next(context);
      return;
    }

    var tenantIdValue = context.User.FindFirst(AppClaimTypes.TenantId)?.Value;
    if (!Guid.TryParse(tenantIdValue, out var tenantId))
    {
      await RejectRequest(context);
      return;
    }

    var tenant = await dbContext.Tenants.SingleOrDefaultAsync(
        entity => entity.Id == new TenantId(tenantId),
        context.RequestAborted);

    if (tenant is null)
    {
      logger.LogWarning("Authenticated request references missing tenant {TenantId}.", tenantId);
      await RejectRequest(context);
      return;
    }

    if (!tenant.IsActive || tenant.SubscriptionEndDate <= DateTimeOffset.UtcNow)
    {
      if (tenant.IsActive)
      {
        tenant.Deactivate();
        await dbContext.SaveChangesAsync(context.RequestAborted);
      }

      await RejectRequest(context);
      return;
    }

    await next(context);
  }

  private static async Task RejectRequest(HttpContext context)
  {
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    await context.Response.WriteAsJsonAsync(
        new { message = ExpiredSubscriptionMessage },
        context.RequestAborted);
  }
}
