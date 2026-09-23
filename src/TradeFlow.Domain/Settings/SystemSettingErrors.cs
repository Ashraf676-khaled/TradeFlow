namespace TradeFlow.Domain.Settings;

using TradeFlow.Domain.Common.Results;

public static class SystemSettingErrors
{
  public static readonly Error KeyRequired = Error.Validation(
      "Setting.KeyRequired", "Setting key cannot be empty.");

  public static readonly Error KeyTooLong = Error.Validation(
      "Setting.KeyTooLong", $"Setting key cannot exceed {SystemSetting.MaxKeyLength} characters.");

  public static readonly Error ValueRequired = Error.Validation(
      "Setting.ValueRequired", "Setting value cannot be empty.");

  public static readonly Error ValueTooLong = Error.Validation(
      "Setting.ValueTooLong", $"Setting value cannot exceed {SystemSetting.MaxValueLength} characters.");

  public static readonly Error NotFound = Error.NotFound(
      "Setting.NotFound", "System setting not found.");
}