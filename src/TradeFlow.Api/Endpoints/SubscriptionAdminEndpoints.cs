namespace TradeFlow.Api.Endpoints;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TradeFlow.Api.Extensions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Infrastructure.Data;

public sealed class SubscriptionAdminEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/admin/tenants")
        .WithTags("Platform Administration")
        .RequireAuthorization("PlatformAdmin");

    group.MapGet("/", async (AppDbContext dbContext, CancellationToken ct) =>
    {
      var tenants = await dbContext.Tenants
          .AsNoTracking()
          .OrderBy(tenant => tenant.Name)
          .Select(tenant => new TenantSubscriptionResponse(
              tenant.Id.Value,
              tenant.Name,
              tenant.IsActive,
              tenant.SubscriptionEndDate))
          .ToListAsync(ct);

      var tenantIds = tenants.Select(tenant => tenant.Id).ToArray();
      var usersByTenant = await dbContext.Users
          .IgnoreQueryFilters()
          .AsNoTracking()
          .Where(user => tenantIds.Contains(user.TenantId.Value))
          .Select(user => new
          {
            TenantId = user.TenantId.Value,
            User = new PlatformUserResponse(
                user.Id.Value,
                user.FullName,
                user.Email.Value,
                user.Role.ToString(),
                user.IsActive)
          })
          .ToListAsync(ct);
      var usersByTenantId = usersByTenant
          .GroupBy(item => item.TenantId)
          .ToDictionary(group => group.Key, group => (IReadOnlyList<PlatformUserResponse>)group
              .Select(item => item.User)
              .OrderBy(user => user.FullName)
              .ToList());

      return Results.Ok(tenants.Select(tenant => tenant with
      {
        Users = usersByTenantId.GetValueOrDefault(tenant.Id, Array.Empty<PlatformUserResponse>())
      }));
    });

    group.MapPut("/{tenantId:guid}/subscription", async (
        Guid tenantId,
        UpdateTenantSubscriptionRequest request,
        AppDbContext dbContext,
        IMemoryCache cache,
        CancellationToken ct) =>
    {
      var tenant = await dbContext.Tenants.SingleOrDefaultAsync(
          entity => entity.Id == new TenantId(tenantId),
          ct);
      if (tenant is null)
        return Results.NotFound(new { message = "الحساب غير موجود." });

      var updateResult = tenant.UpdateSubscription(request.IsActive, request.SubscriptionEndDate);
      if (updateResult.IsError)
        return updateResult.Errors.ToProblem();

      await dbContext.SaveChangesAsync(ct);
      cache.Remove($"tenant-subscription:{tenantId}");

      return Results.Ok(new TenantSubscriptionResponse(
          tenant.Id.Value,
          tenant.Name,
          tenant.IsActive,
          tenant.SubscriptionEndDate));
    });
  }
}

public sealed record UpdateTenantSubscriptionRequest(
    bool IsActive,
    DateTimeOffset SubscriptionEndDate);

public sealed record TenantSubscriptionResponse(
    Guid Id,
    string Name,
    bool IsActive,
    DateTimeOffset SubscriptionEndDate)
{
  public IReadOnlyList<PlatformUserResponse> Users { get; init; } = [];
}

public sealed record PlatformUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive);
