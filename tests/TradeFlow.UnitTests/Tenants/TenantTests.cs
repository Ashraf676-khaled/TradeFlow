namespace TradeFlow.Domain.Tests.Tenants;

using TradeFlow.Domain.Tenants;
using Xunit;

public sealed class TenantTests
{
  [Fact]
  public void Create_StartsActiveWithThirtyDayTrial()
  {
    var createdAt = DateTimeOffset.UtcNow;

    var tenant = Tenant.Create("Trial Company").Value;

    Assert.True(tenant.IsActive);
    Assert.InRange(
        (tenant.SubscriptionEndDate - createdAt).TotalDays,
        29.99,
        30.01);
  }

  [Fact]
  public void UpdateSubscription_ActiveWithPastEndDate_Fails()
  {
    var tenant = Tenant.Create("Trial Company").Value;

    var result = tenant.UpdateSubscription(true, DateTimeOffset.UtcNow.AddDays(-1));

    Assert.True(result.IsError);
    Assert.Equal(TenantErrors.SubscriptionEndDateMustBeInFuture, result.TopError);
    Assert.True(tenant.IsActive);
  }

  [Fact]
  public void UpdateSubscription_InactiveWithPastEndDate_Succeeds()
  {
    var tenant = Tenant.Create("Trial Company").Value;
    var endDate = DateTimeOffset.UtcNow.AddDays(-1);

    var result = tenant.UpdateSubscription(false, endDate);

    Assert.True(result.IsSuccess);
    Assert.False(tenant.IsActive);
    Assert.Equal(endDate.ToUniversalTime(), tenant.SubscriptionEndDate);
  }
}
