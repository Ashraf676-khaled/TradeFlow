namespace TradeFlow.Domain.Common.ValueObjects;

using System.Text.RegularExpressions;
using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;

public sealed class DocumentNumber : ValueObject
{
  private static readonly Regex FormatRegex = new(
      @"^[A-Z]{2,5}-\d{4,8}$",
      RegexOptions.Compiled);

  public string Value { get; }

  private DocumentNumber(string value)
  {
    Value = value;
  }

  public static Result<DocumentNumber> Create(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      return DocumentNumberErrors.Empty;

    var normalized = value.Trim().ToUpperInvariant();

    if (!FormatRegex.IsMatch(normalized))
      return DocumentNumberErrors.InvalidFormat;

    return new DocumentNumber(normalized);
  }

  public static Result<DocumentNumber> Generate(string prefix, long sequence)
  {
    if (string.IsNullOrWhiteSpace(prefix))
      return DocumentNumberErrors.EmptyPrefix;

    if (prefix.Length is < 2 or > 5 || !prefix.All(char.IsLetter))
      return DocumentNumberErrors.InvalidPrefix;

    if (sequence <= 0)
      return DocumentNumberErrors.InvalidSequence;

    var value = $"{prefix.ToUpperInvariant()}-{sequence:D4}";

    return new DocumentNumber(value);
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Value;
  }

  public override string ToString() => Value;
}

public static class DocumentNumberErrors
{
  public static readonly Error Empty = Error.Validation(
      "DocumentNumber.Empty", "Document number cannot be empty.");

  public static readonly Error InvalidFormat = Error.Validation(
      "DocumentNumber.InvalidFormat", "Document number must match the pattern PREFIX-0000 (e.g. ORD-1024).");

  public static readonly Error EmptyPrefix = Error.Validation(
      "DocumentNumber.EmptyPrefix", "Prefix cannot be empty.");

  public static readonly Error InvalidPrefix = Error.Validation(
      "DocumentNumber.InvalidPrefix", "Prefix must be 2-5 letters only.");

  public static readonly Error InvalidSequence = Error.Validation(
      "DocumentNumber.InvalidSequence", "Sequence must be greater than zero.");
}
