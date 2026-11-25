using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TradeFlow.Api.IntegrationTests.Configurations;

public sealed class AuthenticationConfigTests
{
  [Fact]
  public void AddAuthenticationConfig_RejectsPreviouslyCommittedDefaultSecret()
  {
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["Jwt:Secret"] = "SUPER_SECRET_KEY_EXCEEDING_32_BYTES_FOR_HS256"
        })
        .Build();

    var services = new ServiceCollection();

    var exception = Assert.Throws<InvalidOperationException>(
        () => AuthenticationConfig.AddAuthenticationConfig(services, configuration));

    Assert.Contains("random key of at least 32 bytes", exception.Message);
  }
}
