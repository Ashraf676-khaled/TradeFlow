namespace TradeFlow.Application.Settings;

/// <summary>
/// Canonical setting keys stored in the SystemSettings table.
/// </summary>
public static class SystemSettingKeys
{
  public const string TaxEnabled = "Tax.Enabled";
  public const string TaxPercentage = "Tax.Percentage";
  public const string CreditSalesEnabled = "CreditSales.Enabled";
  public const string LowStockThreshold = "LowStock.Threshold";
  public const string InvoiceLayoutStyle = "Invoice.LayoutStyle";

  public static readonly IReadOnlyList<string> All =
  [
    TaxEnabled,
    TaxPercentage,
    CreditSalesEnabled,
    LowStockThreshold,
    InvoiceLayoutStyle,
  ];
}

/// <summary>
/// Allowed values for <see cref="SystemSettingKeys.InvoiceLayoutStyle"/>.
/// </summary>
public static class InvoiceLayoutStyles
{
  public const string A4 = "A4";
  public const string Thermal = "Thermal";

  public static readonly IReadOnlyList<string> All = [A4, Thermal];
}