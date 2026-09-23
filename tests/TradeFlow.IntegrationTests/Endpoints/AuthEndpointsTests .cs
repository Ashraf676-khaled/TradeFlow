namespace TradeFlow.Api.IntegrationTests.Auth;

using System.Net;
using System.Net.Http.Json;
using TradeFlow.Api.IntegrationTests.Common;
using Xunit;

public class AuthEndpointsTests : IntegrationTestBase
{
  public AuthEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

  [Fact]
  public async Task Register_WithValidData_ShouldReturnOkWithAccessToken()
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
    Assert.False(string.IsNullOrEmpty(body!.AccessToken));
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
  public async Task Login_WithValidCredentials_ShouldReturnAccessToken()
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
