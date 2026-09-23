namespace TradeFlow.Application.UnitTests.Settings;

using FluentValidation;
using FluentValidation.Results;
using TradeFlow.Application.Settings;
using TradeFlow.Application.Settings.Commands.UpdateSystemSettings;
using TradeFlow.Application.UnitTests.Helpers;
using Xunit;

public class UpdateSystemSettingsCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_ShouldUpsertAllSettingsInSingleSave()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new UpdateSystemSettingsCommandHandler(context, _currentUser);

    var result = await handler.Handle(
        new UpdateSystemSettingsCommand(
            TaxEnabled: true,
            TaxPercentage: 14.5m,
            CreditSalesEnabled: false,
            LowStockThreshold: 12,
            InvoiceLayoutStyle: "Thermal"),
        CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.TaxEnabled);
    Assert.Equal(14.5m, result.Value.TaxPercentage);
    Assert.False(result.Value.CreditSalesEnabled);
    Assert.Equal(12, result.Value.LowStockThreshold);
    Assert.Equal(InvoiceLayoutStyles.Thermal, result.Value.InvoiceLayoutStyle);
    Assert.Equal(SystemSettingKeys.All.Count, context.SystemSettings.Count());

    // Second update must modify existing rows, never duplicate keys.
    var second = await handler.Handle(
        new UpdateSystemSettingsCommand(
            TaxEnabled: false,
            TaxPercentage: 0m,
            CreditSalesEnabled: true,
            LowStockThreshold: 5,
            InvoiceLayoutStyle: "A4"),
        CancellationToken.None);

    Assert.True(second.IsSuccess);
    Assert.False(second.Value.TaxEnabled);
    Assert.Equal(InvoiceLayoutStyles.A4, second.Value.InvoiceLayoutStyle);
    Assert.Equal(SystemSettingKeys.All.Count, context.SystemSettings.Count());
  }

  [Fact]
  public async Task Handle_WithoutTenant_ShouldFail()
  {
    var noTenantUser = new FakeCurrentUserService { TenantId = null };
    await using var context = TestDbContextFactory.Create(noTenantUser);
    var handler = new UpdateSystemSettingsCommandHandler(context, noTenantUser);

    var result = await handler.Handle(
        new UpdateSystemSettingsCommand(false, 15m, true, 5, "A4"), CancellationToken.None);

    Assert.True(result.IsError);
  }

  [Fact]
  public void Validator_WithInvalidPercentageAndLayout_ShouldFail()
  {
    var validator = new UpdateSystemSettingsCommandValidator();

    var invalid = validator.Validate(new UpdateSystemSettingsCommand(
        TaxEnabled: false, TaxPercentage: 150m, CreditSalesEnabled: true,
        LowStockThreshold: 5, InvoiceLayoutStyle: "Png"));

    Assert.False(invalid.IsValid);
    Assert.Contains(invalid.Errors, e => e.PropertyName == nameof(UpdateSystemSettingsCommand.TaxPercentage));
    Assert.Contains(invalid.Errors, e => e.PropertyName == nameof(UpdateSystemSettingsCommand.InvoiceLayoutStyle));
  }

  [Fact]
  public void Validator_WithTaxEnabledAndZeroPercentage_ShouldFail()
  {
    var validator = new UpdateSystemSettingsCommandValidator();

    var invalid = validator.Validate(new UpdateSystemSettingsCommand(
        TaxEnabled: true, TaxPercentage: 0m, CreditSalesEnabled: true,
        LowStockThreshold: 5, InvoiceLayoutStyle: "A4"));

    Assert.False(invalid.IsValid);
  }

  [Fact]
  public void Validator_WithValidSettings_ShouldPass()
  {
    var validator = new UpdateSystemSettingsCommandValidator();

    var valid = validator.Validate(new UpdateSystemSettingsCommand(
        TaxEnabled: true, TaxPercentage: 15m, CreditSalesEnabled: false,
        LowStockThreshold: 10, InvoiceLayoutStyles.Thermal));

    Assert.True(valid.IsValid);
  }
}