namespace TradeFlow.Api.IntegrationTests.Endpoints;

using System.Net;
using TradeFlow.Api.IntegrationTests.Common;
using Xunit;

public sealed class RateLimitEndpointsTests : IntegrationTestBase
{
  public RateLimitEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

  [Fact]
  public async Task AuthenticationEndpoints_ReturnTooManyRequestsAfterLimit()
  {
    for (var requestNumber = 0; requestNumber < 10; requestNumber++)
    {
      using var response = await Client.PostAsync("/api/auth/revoke", content: null);
      Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    using var rateLimitedResponse = await Client.PostAsync("/api/auth/revoke", content: null);
    Assert.Equal(HttpStatusCode.TooManyRequests, rateLimitedResponse.StatusCode);
    Assert.True(rateLimitedResponse.Headers.TryGetValues("Retry-After", out _));
  }
}
