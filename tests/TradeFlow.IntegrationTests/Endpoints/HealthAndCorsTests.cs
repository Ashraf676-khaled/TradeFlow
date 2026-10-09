namespace TradeFlow.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Text.Json;
using TradeFlow.Api.IntegrationTests.Common;
using Xunit;

public sealed class HealthAndCorsTests : IntegrationTestBase
{
  private const string FrontendOrigin = "https://trade-flow-dashboard-one.vercel.app";

  public HealthAndCorsTests(CustomWebApplicationFactory factory)
      : base(factory)
  {
  }

  [Fact]
  public async Task LiveHealthCheck_IsAnonymousAndDoesNotCheckDependencies()
  {
    using var response = await Client.GetAsync("/health/live");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
    Assert.Empty(body.RootElement.GetProperty("checks").EnumerateArray());
  }

  [Fact]
  public async Task ReadyHealthCheck_ReportsDatabaseStatus()
  {
    using var response = await Client.GetAsync("/health/ready");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
    var checks = body.RootElement.GetProperty("checks").EnumerateArray().ToArray();
    Assert.Equal(9, checks.Length);
    Assert.All(checks, check => Assert.Equal("Healthy", check.GetProperty("status").GetString()));
  }

  [Fact]
  public async Task CombinedHealthCheck_IncludesDatabaseStatus()
  {
    using var response = await Client.GetAsync("/health");

    var responseBody = await response.Content.ReadAsStringAsync();
    Assert.True(response.StatusCode == HttpStatusCode.OK, responseBody);
    using var body = JsonDocument.Parse(responseBody);
    Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
    var checks = body.RootElement.GetProperty("checks").EnumerateArray().ToArray();
    var checkNames = checks
      .Select(check => check.GetProperty("name").GetString())
      .ToHashSet(StringComparer.Ordinal);
    Assert.Equal(9, checkNames.Count);
    Assert.Contains("authentication", checkNames);
    Assert.Contains("customers", checkNames);
    Assert.Contains("expenses", checkNames);
    Assert.Contains("inventory", checkNames);
    Assert.Contains("sales", checkNames);
    Assert.Contains("purchasing", checkNames);
    Assert.Contains("settings", checkNames);
    Assert.Contains("background-jobs", checkNames);
    Assert.Contains("database", checkNames);
    Assert.All(checks, check =>
    {
      Assert.True(check.GetProperty("description").GetString()?.Length > 0);
      Assert.True(check.GetProperty("durationMs").GetDouble() >= 0);
      Assert.True(check.GetProperty("data").ValueKind == JsonValueKind.Object);
    });

    var inventory = checks.Single(check =>
      check.GetProperty("name").GetString() == "inventory");
    var components = inventory.GetProperty("data").GetProperty("components");
    Assert.Equal(3, components.GetArrayLength());
    Assert.Contains(components.EnumerateArray(), component =>
      component.GetProperty("name").GetString() == "products");
    Assert.Contains(components.EnumerateArray(), component =>
      component.GetProperty("name").GetString() == "warehouses");
    Assert.Contains(components.EnumerateArray(), component =>
      component.GetProperty("name").GetString() == "stock-items");
  }

  [Fact]
  public async Task CorsPreflight_AllowsConfiguredFrontendOrigin()
  {
    using var request = new HttpRequestMessage(HttpMethod.Options, "/api/products");
    request.Headers.Add("Origin", FrontendOrigin);
    request.Headers.Add("Access-Control-Request-Method", "GET");
    request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");

    using var response = await Client.SendAsync(request);

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    Assert.Equal(FrontendOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
  }

  [Fact]
  public async Task CorsPreflight_RejectsUnconfiguredOrigin()
  {
    using var request = new HttpRequestMessage(HttpMethod.Options, "/api/products");
    request.Headers.Add("Origin", "https://untrusted.example");
    request.Headers.Add("Access-Control-Request-Method", "GET");

    using var response = await Client.SendAsync(request);

    Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
  }

  [Theory]
  [InlineData("http://localhost:5173")]
  [InlineData("https://trade-flow-dashboard-one.vercel.app.attacker.example")]
  public async Task CorsPreflight_RejectsOriginsOtherThanTheFrontend(string origin)
  {
    using var request = new HttpRequestMessage(HttpMethod.Options, "/api/products");
    request.Headers.Add("Origin", origin);
    request.Headers.Add("Access-Control-Request-Method", "GET");

    using var response = await Client.SendAsync(request);

    Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
  }
}
