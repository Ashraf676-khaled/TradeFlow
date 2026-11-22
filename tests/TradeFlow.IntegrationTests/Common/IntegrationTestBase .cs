namespace TradeFlow.Api.IntegrationTests.Common;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Domain.Users;
using TradeFlow.Infrastructure.Data;
using Xunit;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
  protected readonly HttpClient Client;
  protected readonly CustomWebApplicationFactory Factory;
  protected string? RefreshCookie { get; private set; }

  protected IntegrationTestBase(CustomWebApplicationFactory factory)
  {
    Factory = factory;

    // AllowAutoRedirect = false يمنع نهائيًا أي احتمال إن الـ Authorization header
    // يتشال بسبب اتباع Redirect (زي HTTPS Redirection) بين Origins مختلفة
    Client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false
    });
  }

  protected async Task<AuthResponseDto> RegisterAndLoginAsync(
      string? emailOverride = null,
      string companyName = "Test Company",
      string fullName = "Test User",
      HttpClient? targetClient = null)
  {
    var client = targetClient ?? Client;
    var email = emailOverride ?? $"user{Guid.NewGuid():N}@test.com";

    var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
    {
      companyName,
      fullName,
      email,
      password = "P@ssw0rd1"
    });

    if (!registerResponse.IsSuccessStatusCode)
    {
      var errorBody = await registerResponse.Content.ReadAsStringAsync();
      Assert.Fail($"Register failed with {registerResponse.StatusCode}: {errorBody}");
    }

    var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
    if (authResponse is null || !authResponse.RequiresTwoFactor)
      Assert.Fail("Register succeeded but did not require email confirmation.");

    using (var scope = Factory.Services.CreateScope())
    {
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      var user = await dbContext.Users
          .IgnoreQueryFilters()
          .SingleAsync(entity => entity.Email.Value == email.ToLowerInvariant());
      var hasher = new PasswordHasher<User>();
      user.SetEmailOtp(
          hasher.HashPassword(user, "123456"),
          DateTimeOffset.UtcNow.AddMinutes(5),
          EmailOtpPurpose.ConfirmEmail);
      await dbContext.SaveChangesAsync();
    }

    using var verifyResponse = await client.PostAsJsonAsync(
        "/api/auth/verify-otp",
        new { email, code = "123456" });
    if (!verifyResponse.IsSuccessStatusCode)
    {
      var errorBody = await verifyResponse.Content.ReadAsStringAsync();
      Assert.Fail($"Email confirmation failed with {verifyResponse.StatusCode}: {errorBody}");
    }

    var verifiedAuth = await verifyResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
    if (verifiedAuth is null || string.IsNullOrEmpty(verifiedAuth.AccessToken))
      Assert.Fail("Email confirmation succeeded but AccessToken was null or empty.");

    RefreshCookie = verifyResponse.Headers.GetValues("Set-Cookie").Single().Split(';', 2)[0];
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", verifiedAuth!.AccessToken);

    return verifiedAuth;
  }

  public void Dispose() => Client.Dispose();
}

public sealed record AuthResponseDto(
    string? AccessToken,
    DateTimeOffset? AccessTokenExpiresUtc,
    bool RequiresTwoFactor = false,
    string? Email = null);
