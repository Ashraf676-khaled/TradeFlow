namespace TradeFlow.Api.IntegrationTests.Settings;

using System.Net;
using System.Net.Http.Json;
using TradeFlow.Api.IntegrationTests.Common;
using Xunit;

public class SettingsEndpointsTests : IntegrationTestBase
{
  public SettingsEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

  [Fact]
  public async Task GetSettings_ForNewTenant_ShouldReturnOkWithDefaults()
  {
    await RegisterAndLoginAsync();

    var response = await Client.GetAsync("/api/settings");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var settings = await response.Content.ReadFromJsonAsync<SettingsResponse>();
    Assert.NotNull(settings);
    Assert.False(settings!.TaxEnabled);
    Assert.Equal(15m, settings.TaxPercentage);
    Assert.True(settings.CreditSalesEnabled);
    Assert.Equal(5, settings.LowStockThreshold);
    Assert.Equal("A4", settings.InvoiceLayoutStyle);
  }

  [Fact]
  public async Task PutSettings_ThenGet_ShouldPersistChanges()
  {
    await RegisterAndLoginAsync();

    var putResponse = await Client.PutAsJsonAsync("/api/settings", new
    {
      taxEnabled = true,
      taxPercentage = 10,
      creditSalesEnabled = false,
      lowStockThreshold = 9,
      invoiceLayoutStyle = "Thermal"
    });
    Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

    var getResponse = await Client.GetAsync("/api/settings");
    Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

    var settings = await getResponse.Content.ReadFromJsonAsync<SettingsResponse>();
    Assert.NotNull(settings);
    Assert.True(settings!.TaxEnabled);
    Assert.Equal(10m, settings.TaxPercentage);
    Assert.False(settings.CreditSalesEnabled);
    Assert.Equal(9, settings.LowStockThreshold);
    Assert.Equal("Thermal", settings.InvoiceLayoutStyle);
  }

  [Fact]
  public async Task GetSettings_WithoutToken_ShouldReturnUnauthorized()
  {
    var response = await Client.GetAsync("/api/settings");

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  private sealed record SettingsResponse(
      bool TaxEnabled,
      decimal TaxPercentage,
      bool CreditSalesEnabled,
      int LowStockThreshold,
      string InvoiceLayoutStyle);
}
