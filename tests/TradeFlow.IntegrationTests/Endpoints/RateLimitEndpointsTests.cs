namespace TradeFlow.Api.IntegrationTests.Endpoints;

using System.Net;
using TradeFlow.Api.IntegrationTests.Common;
using Xunit;

public sealed class RateLimitEndpointsTests : IClassFixture<RateLimitWebApplicationFactory>, IDisposable
{
  private readonly HttpClient _client;

  public RateLimitEndpointsTests(RateLimitWebApplicationFactory factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task AuthenticationEndpoints_ReturnTooManyRequestsAfterLimit()
  {
    for (var requestNumber = 0; requestNumber < 10; requestNumber++)
    {
      using var response = await _client.PostAsync("/api/auth/revoke", content: null);
      Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    using var rateLimitedResponse = await _client.PostAsync("/api/auth/revoke", content: null);
    Assert.Equal(HttpStatusCode.TooManyRequests, rateLimitedResponse.StatusCode);
    Assert.True(rateLimitedResponse.Headers.TryGetValues("Retry-After", out _));
  }

  public void Dispose() => _client.Dispose();
}

public sealed class RateLimitWebApplicationFactory : CustomWebApplicationFactory
{
  public RateLimitWebApplicationFactory() : base(10) { }
}
