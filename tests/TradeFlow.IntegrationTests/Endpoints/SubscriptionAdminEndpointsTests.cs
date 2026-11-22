namespace TradeFlow.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.IntegrationTests.Common;
using TradeFlow.Infrastructure.Data;
using TradeFlow.Infrastructure.Identity;
using Xunit;

public sealed class SubscriptionAdminEndpointsTests : IntegrationTestBase
{
  public SubscriptionAdminEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

  [Fact]
  public async Task Register_CreatesTenantWithThirtyDayTrial_AndProtectedRequestsWork()
  {
    var email = $"trial{Guid.NewGuid():N}@test.com";
    var createdAt = DateTimeOffset.UtcNow;
    await RegisterAndLoginAsync(email, "Trial Company", "Trial Owner");

    using var scope = Factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var tenant = await dbContext.Tenants.SingleAsync(entity => entity.Name == "Trial Company");

    Assert.True(tenant.IsActive);
    Assert.InRange((tenant.SubscriptionEndDate - createdAt).TotalDays, 29.99, 30.01);

    var protectedResponse = await Client.GetAsync("/api/settings");
    Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
  }

  [Fact]
  public async Task RefreshTokenAlias_RotatesCookieAndReturnsNewAccessToken()
  {
    var originalAuth = await RegisterAndLoginAsync(
        $"refresh{Guid.NewGuid():N}@test.com",
        "Refresh Company",
        "Refresh Owner");
    var refreshCookie = RefreshCookie!;

    using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh-token")
    {
      Content = JsonContent.Create(new { accessToken = originalAuth.AccessToken })
    };
    refreshRequest.Headers.Add("Cookie", refreshCookie);
    using var refreshResponse = await Client.SendAsync(refreshRequest);

    Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
    var refreshedAuth = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
    Assert.NotNull(refreshedAuth);
    Assert.NotEqual(originalAuth.AccessToken, refreshedAuth!.AccessToken);
    Assert.Contains(refreshResponse.Headers.GetValues("Set-Cookie"), cookie => cookie.Contains("refreshToken="));
  }

  [Fact]
  public async Task ExpiredSubscription_AllowsAdminReadOnlyAccessAndBlocksWrites()
  {
    var email = $"expired{Guid.NewGuid():N}@test.com";
    await RegisterAndLoginAsync(email, "Expired Company", "Expired Owner");

    using (var scope = Factory.Services.CreateScope())
    {
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      await dbContext.Database.ExecuteSqlInterpolatedAsync(
          $"UPDATE Tenants SET SubscriptionEndDate = {DateTimeOffset.UtcNow.AddDays(-1)} WHERE Name = {"Expired Company"}");
    }

    using var response = await Client.GetAsync("/api/settings");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    using var updateResponse = await Client.PutAsJsonAsync("/api/settings", new { currencySymbol = "EGP" });
    Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    var body = await updateResponse.Content.ReadFromJsonAsync<SubscriptionErrorDto>();
    Assert.Contains("انتهت الفترة التجريبية", body?.Message);

    using var verificationScope = Factory.Services.CreateScope();
    var verificationContext = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
    var tenant = await verificationContext.Tenants.SingleAsync(entity => entity.Name == "Expired Company");
    Assert.False(tenant.IsActive);
  }

  [Fact]
  public async Task PlatformAdmin_CanListAndExtendSubscriptions_ButTenantAdminCannot()
  {
    await RegisterAndLoginAsync("platform-admin@test.com", "Platform Admin Company", "Platform Admin");

    using (var scope = Factory.Services.CreateScope())
    {
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      await dbContext.Database.ExecuteSqlInterpolatedAsync(
          $"UPDATE Tenants SET SubscriptionEndDate = {DateTimeOffset.UtcNow.AddDays(-1)} WHERE Name = {"Platform Admin Company"}");
    }

    using var expiredAdminTenantResponse = await Client.GetAsync("/api/settings");
    Assert.Equal(HttpStatusCode.OK, expiredAdminTenantResponse.StatusCode);

    using var tenantListResponse = await Client.GetAsync("/api/admin/tenants");
    Assert.Equal(HttpStatusCode.OK, tenantListResponse.StatusCode);
    var tenants = await tenantListResponse.Content.ReadFromJsonAsync<List<TenantSubscriptionDto>>();
    Assert.NotNull(tenants);
    var tenant = Assert.Single(tenants!, item => item.Name == "Platform Admin Company");

    var endDate = DateTimeOffset.UtcNow.AddDays(60);
    using var updateResponse = await Client.PutAsJsonAsync(
        $"/api/admin/tenants/{tenant.Id}/subscription",
        new { isActive = true, subscriptionEndDate = endDate });

    Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    var updatedTenant = await updateResponse.Content.ReadFromJsonAsync<TenantSubscriptionDto>();
    Assert.NotNull(updatedTenant);
    Assert.True(updatedTenant!.IsActive);
    Assert.Equal(endDate.Date, updatedTenant.SubscriptionEndDate.Date);

    var regularClient = Factory.CreateClient();
    var regularEmail = $"tenantadmin{Guid.NewGuid():N}@test.com";
    await RegisterAndLoginAsync(regularEmail, "Regular Company", "Tenant Admin", regularClient);

    using var forbiddenResponse = await regularClient.GetAsync("/api/admin/tenants");
    Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
  }

  [Fact]
  public void PlatformAdminOptions_MatchesConfiguredEmailIgnoringCaseAndWhitespace()
  {
    var options = new PlatformAdminOptions { Email = "owner@example.com" };

    Assert.True(options.IsAdmin(" OWNER@example.com "));
    Assert.False(options.IsAdmin("other@example.com"));
    Assert.False(options.IsAdmin(""));
  }

  [Fact]
  public void PlatformAdminOptions_MatchesConfiguredUserId_AndPrefersItOverEmail()
  {
    var configuredUserId = Guid.NewGuid();
    var options = new PlatformAdminOptions
    {
      UserId = configuredUserId,
      Email = "owner@example.com"
    };

    Assert.True(options.IsAdmin(configuredUserId));
    Assert.False(options.IsAdmin(Guid.NewGuid(), "owner@example.com"));
    Assert.False(options.IsAdmin(null, "owner@example.com"));
  }

  private sealed record TenantSubscriptionDto(
      Guid Id,
      string Name,
      bool IsActive,
      DateTimeOffset SubscriptionEndDate);

  private sealed record SubscriptionErrorDto(string Message);
}
