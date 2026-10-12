namespace TradeFlow.Api.Endpoints;

using Microsoft.EntityFrameworkCore;
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

      return Results.Ok(tenants);
    });

    group.MapPut("/{tenantId:guid}/subscription", async (
        Guid tenantId,
        UpdateTenantSubscriptionRequest request,
        AppDbContext dbContext,
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
    DateTimeOffset SubscriptionEndDate);
