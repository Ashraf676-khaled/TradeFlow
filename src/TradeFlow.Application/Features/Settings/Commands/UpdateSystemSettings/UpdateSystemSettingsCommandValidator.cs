namespace TradeFlow.Application.Settings.Commands.UpdateSystemSettings;

using FluentValidation;

public sealed class UpdateSystemSettingsCommandValidator : AbstractValidator<UpdateSystemSettingsCommand>
{
  public UpdateSystemSettingsCommandValidator()
  {
    RuleFor(x => x.TaxPercentage).InclusiveBetween(0, 100);
    RuleFor(x => x.LowStockThreshold).InclusiveBetween(0, 1_000_000);
    RuleFor(x => x.InvoiceLayoutStyle)
        .Must(style => InvoiceLayoutStyles.All.Contains(style?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase))
        .WithMessage("Invoice layout style must be 'A4' or 'Thermal'.");

    When(x => x.TaxEnabled, () =>
    {
      RuleFor(x => x.TaxPercentage).GreaterThan(0)
          .WithMessage("Tax percentage must be greater than zero when tax is enabled.");
    });
  }
}