namespace TradeFlow.Domain.Common.ValueObjects;

using System.Text.RegularExpressions;
using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;

public sealed class Sku : ValueObject
{
  private const int MaxLength = 50;

  private static readonly Regex SkuFormatRegex =
      new(@"^[A-Z0-9\-]+$", RegexOptions.Compiled);

  public string Value { get; }

  private Sku(string value)
  {
    Value = value;
  }

  public static Result<Sku> Create(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      return SkuErrors.Empty;

    var normalized = value.Trim().ToUpperInvariant();

    if (normalized.Length > MaxLength)
      return SkuErrors.TooLong;

    if (!SkuFormatRegex.IsMatch(normalized))
      return SkuErrors.InvalidFormat;

    return new Sku(normalized);
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Value;
  }

  public override string ToString() => Value;
}

public static class SkuErrors
{
  public static readonly Error Empty = Error.Validation(
      "Sku.Empty", "SKU cannot be empty.");

  public static readonly Error TooLong = Error.Validation(
      "Sku.TooLong", "SKU cannot exceed 50 characters.");

  public static readonly Error InvalidFormat = Error.Validation(
      "Sku.InvalidFormat", "SKU can only contain letters, numbers, and hyphens.");
}
