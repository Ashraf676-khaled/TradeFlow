namespace TradeFlow.Application.UnitTests.Settings;

using TradeFlow.Application.Settings;
using TradeFlow.Application.Settings.Queries.GetSystemSettings;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Settings;
using Xunit;

public class GetSystemSettingsQueryHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithNoStoredSettings_ShouldReturnDefaults()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new GetSystemSettingsQueryHandler(context);

    var result = await handler.Handle(new GetSystemSettingsQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(result.Value.TaxEnabled);
    Assert.Equal(15m, result.Value.TaxPercentage);
    Assert.True(result.Value.CreditSalesEnabled);
    Assert.Equal(5, result.Value.LowStockThreshold);
    Assert.Equal(InvoiceLayoutStyles.A4, result.Value.InvoiceLayoutStyle);
  }

  [Fact]
  public async Task Handle_WithStoredSettings_ShouldReturnPersistedValues()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var tenantId = new TenantId(_currentUser.TenantId!.Value);

    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.TaxEnabled, "true").Value);
    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.TaxPercentage, "14.5").Value);
    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.CreditSalesEnabled, "false").Value);
    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.LowStockThreshold, "12").Value);
    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.InvoiceLayoutStyle, "Thermal").Value);
    await context.SaveChangesAsync(CancellationToken.None);

    var handler = new GetSystemSettingsQueryHandler(context);
    var result = await handler.Handle(new GetSystemSettingsQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.TaxEnabled);
    Assert.Equal(14.5m, result.Value.TaxPercentage);
    Assert.False(result.Value.CreditSalesEnabled);
    Assert.Equal(12, result.Value.LowStockThreshold);
    Assert.Equal(InvoiceLayoutStyles.Thermal, result.Value.InvoiceLayoutStyle);
  }

  [Fact]
  public async Task Handle_WithCorruptedStoredValues_ShouldFallBackToDefaults()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var tenantId = new TenantId(_currentUser.TenantId!.Value);

    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.TaxEnabled, "not-a-bool").Value);
    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.TaxPercentage, "abc").Value);
    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.LowStockThreshold, "-7").Value);
    context.SystemSettings.Add(SystemSetting.Create(tenantId, SystemSettingKeys.InvoiceLayoutStyle, "Png").Value);
    await context.SaveChangesAsync(CancellationToken.None);

    var handler = new GetSystemSettingsQueryHandler(context);
    var result = await handler.Handle(new GetSystemSettingsQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(result.Value.TaxEnabled);
    Assert.Equal(15m, result.Value.TaxPercentage);
    Assert.Equal(0, result.Value.LowStockThreshold); // clamped to >= 0
    Assert.Equal(InvoiceLayoutStyles.A4, result.Value.InvoiceLayoutStyle);
  }
}