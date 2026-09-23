namespace TradeFlow.Api.IntegrationTests.Common;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
  protected readonly HttpClient Client;
  private readonly CustomWebApplicationFactory _factory;

  protected IntegrationTestBase(CustomWebApplicationFactory factory)
  {
    _factory = factory;

    // AllowAutoRedirect = false يمنع نهائيًا أي احتمال إن الـ Authorization header
    // يتشال بسبب اتباع Redirect (زي HTTPS Redirection) بين Origins مختلفة
    Client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false
    });
  }

  protected async Task<string> RegisterAndLoginAsync(string? emailOverride = null)
  {
    var email = emailOverride ?? $"user{Guid.NewGuid():N}@test.com";

    var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new
    {
      companyName = "Test Company",
      fullName = "Test User",
      email,
      password = "P@ssw0rd1"
    });

    if (!registerResponse.IsSuccessStatusCode)
    {
      var errorBody = await registerResponse.Content.ReadAsStringAsync();
      Assert.Fail($"Register failed with {registerResponse.StatusCode}: {errorBody}");
    }

    var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

    if (authResponse is null || string.IsNullOrEmpty(authResponse.AccessToken))
      Assert.Fail("Register succeeded but AccessToken was null or empty.");

    Client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

    return authResponse.AccessToken;
  }

  public void Dispose() => Client.Dispose();
}

public sealed record AuthResponseDto(string AccessToken, DateTimeOffset AccessTokenExpiresUtc);
