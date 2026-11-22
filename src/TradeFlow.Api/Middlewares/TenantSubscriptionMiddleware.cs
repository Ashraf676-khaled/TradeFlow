namespace TradeFlow.Api.Middlewares;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TradeFlow.Application.Common.Constants;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Users;
using TradeFlow.Infrastructure.Data;
using TradeFlow.Infrastructure.Identity;

public sealed class TenantSubscriptionMiddleware(
    RequestDelegate next,
    ILogger<TenantSubscriptionMiddleware> logger,
    IMemoryCache cache,
    IOptions<PlatformAdminOptions> platformAdminOptions)
{
  private static readonly TimeSpan SubscriptionCacheDuration = TimeSpan.FromSeconds(5);
  private const string ExpiredSubscriptionMessage =
      "انتهت الفترة التجريبية. يرجى التواصل مع مسؤول المنصة لتجديد الاشتراك.";

  public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
  {
    if (context.User.Identity?.IsAuthenticated != true)
    {
      await next(context);
      return;
    }

    var userIdValue = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? context.User.FindFirst("sub")?.Value;
    var userId = Guid.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : (Guid?)null;
    var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    if (platformAdminOptions.Value.IsAdmin(userId, email)
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

    var cacheKey = $"tenant-subscription:{tenantId}";
    var subscription = await cache.GetOrCreateAsync(cacheKey, async entry =>
    {
      entry.AbsoluteExpirationRelativeToNow = SubscriptionCacheDuration;
      return await dbContext.Tenants
          .AsNoTracking()
          .Where(entity => entity.Id == new TenantId(tenantId))
          .Select(entity => new TenantSubscriptionSnapshot(
              entity.IsActive,
              entity.SubscriptionEndDate))
          .SingleOrDefaultAsync(context.RequestAborted);
    });

    if (subscription is null)
    {
      logger.LogWarning("Authenticated request references missing tenant {TenantId}.", tenantId);
      await RejectRequest(context);
      return;
    }

    if (!subscription.IsActive || subscription.SubscriptionEndDate <= DateTimeOffset.UtcNow)
    {
      if (subscription.IsActive)
      {
        await dbContext.Tenants
            .Where(entity => entity.Id == new TenantId(tenantId) && entity.IsActive)
            .ExecuteUpdateAsync(
                update => update.SetProperty(entity => entity.IsActive, false),
                context.RequestAborted);
        cache.Remove(cacheKey);
      }

      if (context.Request.Method == HttpMethods.Get &&
          context.User.IsInRole(nameof(UserRole.Admin)))
      {
        await next(context);
        return;
      }

      await RejectRequest(context);
      return;
    }

    await next(context);
  }

  private static async Task RejectRequest(HttpContext context)
  {
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    context.Response.ContentType = "application/json; charset=utf-8";
    await context.Response.WriteAsJsonAsync(
        new { message = ExpiredSubscriptionMessage },
        context.RequestAborted);
  }

  private sealed record TenantSubscriptionSnapshot(
      bool IsActive,
      DateTimeOffset SubscriptionEndDate);
}