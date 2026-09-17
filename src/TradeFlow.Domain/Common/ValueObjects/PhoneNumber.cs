namespace TradeFlow.Domain.Common.ValueObjects;

using System.Text.RegularExpressions;
using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;

public sealed class PhoneNumber : ValueObject
{
  private static readonly Regex EgyptianMobileRegex =
      new(@"^01[0125][0-9]{8}$", RegexOptions.Compiled);

  public string Value { get; }

  private PhoneNumber(string value)
  {
    Value = value;
  }

  public static Result<PhoneNumber> Create(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      return PhoneNumberErrors.Empty;

    var normalized = value.Trim().Replace(" ", "").Replace("-", "");

    if (!EgyptianMobileRegex.IsMatch(normalized))
      return PhoneNumberErrors.InvalidFormat;

    return new PhoneNumber(normalized);
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Value;
  }

  public override string ToString() => Value;
}

public static class PhoneNumberErrors
{
  public static readonly Error Empty = Error.Validation(
      "PhoneNumber.Empty", "Phone number cannot be empty.");

  public static readonly Error InvalidFormat = Error.Validation(
      "PhoneNumber.InvalidFormat", "Phone number must be a valid Egyptian mobile number.");
}
