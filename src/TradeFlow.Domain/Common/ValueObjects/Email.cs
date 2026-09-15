namespace TradeFlow.Domain.Common.ValueObjects;

using System.Text.RegularExpressions;
using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;

public sealed class Email : ValueObject
{
  private const int MaxLength = 254;

  private static readonly Regex EmailFormatRegex = new(
      @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
      RegexOptions.Compiled);

  public string Value { get; }

  private Email(string value)
  {
    Value = value;
  }

  public static Result<Email> Create(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      return EmailErrors.Empty;

    var normalized = value.Trim().ToLowerInvariant();

    if (normalized.Length > MaxLength)
      return EmailErrors.TooLong;

    if (!EmailFormatRegex.IsMatch(normalized))
      return EmailErrors.InvalidFormat;

    return new Email(normalized);
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Value;
  }

  public override string ToString() => Value;
}

public static class EmailErrors
{
  public static readonly Error Empty = Error.Validation(
      "Email.Empty", "Email cannot be empty.");

  public static readonly Error TooLong = Error.Validation(
      "Email.TooLong", "Email cannot exceed 254 characters.");

  public static readonly Error InvalidFormat = Error.Validation(
      "Email.InvalidFormat", "Email format is invalid.");
}
