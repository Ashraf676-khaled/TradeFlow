namespace TradeFlow.Application.Settings;

using TradeFlow.Domain.Settings;

/// <summary>
/// Flattened, strongly-typed view of all known system settings.
/// Serialized camelCase by the API (taxEnabled, taxPercentage, ...).
/// </summary>
public sealed record SystemSettingsDto(
    bool TaxEnabled,
    decimal TaxPercentage,
    bool CreditSalesEnabled,
    int LowStockThreshold,
    string InvoiceLayoutStyle);

/// <summary>
/// Resolves persisted key/value rows into a <see cref="SystemSettingsDto"/>,
/// falling back to safe defaults for missing or corrupted values so reads
/// never fail and dropdowns/toggles are never blocked.
/// </summary>
public static class SystemSettingsResolver
{
  public const bool DefaultTaxEnabled = false;
  public const decimal DefaultTaxPercentage = 15m;
  public const bool DefaultCreditSalesEnabled = true;
  public const int DefaultLowStockThreshold = 5;
  public const string DefaultInvoiceLayoutStyle = InvoiceLayoutStyles.A4;

  public static SystemSettingsDto Resolve(IEnumerable<SystemSetting> settings)
      => Resolve(settings.Select(s => new KeyValuePair<string, string>(s.Key, s.Value)));

  public static SystemSettingsDto Resolve(IEnumerable<KeyValuePair<string, string>> pairs)
  {
    var map = pairs.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

    return new SystemSettingsDto(
        TaxEnabled: ReadBool(map, SystemSettingKeys.TaxEnabled, DefaultTaxEnabled),
        TaxPercentage: ReadDecimal(map, SystemSettingKeys.TaxPercentage, DefaultTaxPercentage, 0m, 100m),
        CreditSalesEnabled: ReadBool(map, SystemSettingKeys.CreditSalesEnabled, DefaultCreditSalesEnabled),
        LowStockThreshold: (int)ReadDecimal(map, SystemSettingKeys.LowStockThreshold, DefaultLowStockThreshold, 0m, 1_000_000m),
        InvoiceLayoutStyle: ReadLayout(map));
  }

  private static bool ReadBool(IReadOnlyDictionary<string, string> map, string key, bool fallback)
      => map.TryGetValue(key, out var raw) && bool.TryParse(raw, out var value) ? value : fallback;

  private static decimal ReadDecimal(
      IReadOnlyDictionary<string, string> map, string key, decimal fallback, decimal min, decimal max)
  {
    if (!map.TryGetValue(key, out var raw)
        || !decimal.TryParse(raw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var value))
    {
      return fallback;
    }

    return Math.Clamp(value, min, max);
  }

  private static string ReadLayout(IReadOnlyDictionary<string, string> map)
  {
    if (!map.TryGetValue(SystemSettingKeys.InvoiceLayoutStyle, out var raw))
      return DefaultInvoiceLayoutStyle;

    var normalized = raw?.Trim() ?? string.Empty;
    var match = InvoiceLayoutStyles.All.FirstOrDefault(
        l => string.Equals(l, normalized, StringComparison.OrdinalIgnoreCase));

    return match ?? DefaultInvoiceLayoutStyle;
  }
}