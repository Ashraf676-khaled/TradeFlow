namespace TradeFlow.Domain.Settings;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;

/// <summary>
/// Tenant-scoped key/value system setting. Known keys are defined in
/// TradeFlow.Application.Settings.SystemSettingKeys; unknown keys are tolerated
/// so new preferences can be added without schema changes.
/// </summary>
public sealed class SystemSetting : AggregateRoot, IAuditableEntity
{
  public const int MaxKeyLength = 100;
  public const int MaxValueLength = 500;

  public new SettingId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public string Key { get; private set; } = string.Empty;
  public string Value { get; private set; } = string.Empty;

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private SystemSetting() { } // EF Core

  private SystemSetting(SettingId id, TenantId tenantId, string key, string value)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    Key = key;
    Value = value;
  }

  public static Result<SystemSetting> Create(TenantId tenantId, string key, string value)
  {
    var keyResult = NormalizeKey(key);
    if (keyResult.IsError)
      return keyResult.Errors;

    var valueResult = NormalizeValue(value);
    if (valueResult.IsError)
      return valueResult.Errors;

    return new SystemSetting(SettingId.New(), tenantId, keyResult.Value, valueResult.Value);
  }

  public Result<Success> ChangeValue(string newValue)
  {
    var valueResult = NormalizeValue(newValue);
    if (valueResult.IsError)
      return valueResult.Errors;

    Value = valueResult.Value;
    return Result.Success;
  }

  public Result<Success> Rename(string newKey)
  {
    var keyResult = NormalizeKey(newKey);
    if (keyResult.IsError)
      return keyResult.Errors;

    Key = keyResult.Value;
    return Result.Success;
  }

  private static Result<string> NormalizeKey(string key)
  {
    var normalized = key?.Trim() ?? string.Empty;
    if (normalized.Length == 0)
      return SystemSettingErrors.KeyRequired;

    if (normalized.Length > MaxKeyLength)
      return SystemSettingErrors.KeyTooLong;

    return normalized;
  }

  private static Result<string> NormalizeValue(string value)
  {
    var normalized = value?.Trim() ?? string.Empty;
    if (normalized.Length == 0)
      return SystemSettingErrors.ValueRequired;

    if (normalized.Length > MaxValueLength)
      return SystemSettingErrors.ValueTooLong;

    return normalized;
  }
}