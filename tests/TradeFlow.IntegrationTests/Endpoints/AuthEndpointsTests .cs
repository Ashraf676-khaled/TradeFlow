namespace TradeFlow.Api.IntegrationTests.Auth;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.IntegrationTests.Common;
using TradeFlow.Domain.Users;
using TradeFlow.Infrastructure.Data;
using Xunit;

public class AuthEndpointsTests : IntegrationTestBase
{
  public AuthEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

  [Fact]
  public async Task Register_WithValidData_ShouldRequireEmailOtpWithoutIssuingTokens()
  {
    var response = await Client.PostAsJsonAsync("/api/auth/register", new
    {
      companyName = "Acme Inc",
      fullName = "Ahmed Ali",
      email = $"ahmed{Guid.NewGuid():N}@test.com",
      password = "P@ssw0rd1"
    });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    Assert.NotNull(body);
    Assert.True(body!.RequiresTwoFactor);
    Assert.Null(body.AccessToken);
    Assert.False(response.Headers.Contains("Set-Cookie"));
  }

  [Fact]
  public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
  {
    var email = $"dup{Guid.NewGuid():N}@test.com";

    await Client.PostAsJsonAsync("/api/auth/register", new
    {
      companyName = "Acme Inc",
      fullName = "Ahmed Ali",
      email,
      password = "P@ssw0rd1"
    });

    var response = await Client.PostAsJsonAsync("/api/auth/register", new
    {
      companyName = "Another Co",
      fullName = "Sara",
      email,
      password = "P@ssw0rd1"
    });

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
  }

  [Fact]
  public async Task Login_WithValidCredentials_ShouldRequireEmailOtpBeforeIssuingTokens()
  {
    var email = $"login{Guid.NewGuid():N}@test.com";

    await Client.PostAsJsonAsync("/api/auth/register", new
    {
      companyName = "Acme Inc",
      fullName = "Ahmed Ali",
      email,
      password = "P@ssw0rd1"
    });

    var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, password = "P@ssw0rd1" });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    Assert.NotNull(body);
    Assert.True(body!.RequiresTwoFactor);
    Assert.Null(body.AccessToken);
  }

  [Fact]
  public async Task Login_WithTwoFactorEnabled_ShouldIssueTokensOnlyAfterOtpVerification()
  {
    var email = $"twofactor{Guid.NewGuid():N}@test.com";
    await RegisterAndLoginAsync(email);

    using (var scope = Factory.Services.CreateScope())
    {
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      var user = await dbContext.Users
          .IgnoreQueryFilters()
          .SingleAsync(entity => entity.Email.Value == email);
      user.EnableTwoFactor();
      await dbContext.SaveChangesAsync();
    }

    using var loginResponse = await Client.PostAsJsonAsync(
        "/api/auth/login",
        new { email, password = "P@ssw0rd1" });
    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    var challenge = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
    Assert.NotNull(challenge);
    Assert.True(challenge!.RequiresTwoFactor);
    Assert.Null(challenge.AccessToken);
    Assert.False(loginResponse.Headers.Contains("Set-Cookie"));

    using (var scope = Factory.Services.CreateScope())
    {
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      var user = await dbContext.Users
          .IgnoreQueryFilters()
          .SingleAsync(entity => entity.Email.Value == email);
      var hasher = new PasswordHasher<User>();
      user.SetEmailOtp(
          hasher.HashPassword(user, "654321"),
          DateTimeOffset.UtcNow.AddMinutes(5),
          EmailOtpPurpose.TwoFactor);
      await dbContext.SaveChangesAsync();
    }

    using var verifyResponse = await Client.PostAsJsonAsync(
        "/api/auth/verify-otp",
        new { email, code = "654321" });
    Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
    var auth = await verifyResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
    Assert.NotNull(auth);
    Assert.False(auth!.RequiresTwoFactor);
    Assert.False(string.IsNullOrEmpty(auth.AccessToken));
    Assert.Contains(verifyResponse.Headers.GetValues("Set-Cookie"), cookie => cookie.Contains("refreshToken="));
  }

  [Fact]
  public async Task Login_WithWrongPassword_ShouldReturnNotFound()
  {
    var email = $"wrongpass{Guid.NewGuid():N}@test.com";

    await Client.PostAsJsonAsync("/api/auth/register", new
    {
      companyName = "Acme Inc",
      fullName = "Ahmed Ali",
      email,
      password = "P@ssw0rd1"
    });

    var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, password = "WrongPass1" });

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
  {
    var response = await Client.GetAsync("/api/products");
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }
}
