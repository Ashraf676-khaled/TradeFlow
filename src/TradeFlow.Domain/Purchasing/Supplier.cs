namespace TradeFlow.Domain.Purchasing;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class Supplier : AggregateRoot, IAuditableEntity
{
  public new SupplierId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public string Name { get; private set; } = string.Empty;
  public PhoneNumber Phone { get; private set; } = null!;
  public bool IsActive { get; private set; }

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private Supplier() { } // EF Core

  private Supplier(SupplierId id, TenantId tenantId, string name, PhoneNumber phone)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    Name = name;
    Phone = phone;
    IsActive = true;
  }

  public static Result<Supplier> Create(TenantId tenantId, string name, PhoneNumber phone)
  {
    if (string.IsNullOrWhiteSpace(name))
      return SupplierErrors.NameRequired;

    if (name.Length > 200)
      return SupplierErrors.NameTooLong;

    return new Supplier(SupplierId.New(), tenantId, name, phone);
  }

  public Result<Success> UpdateContactInfo(string name, PhoneNumber phone)
  {
    if (string.IsNullOrWhiteSpace(name))
      return SupplierErrors.NameRequired;

    Name = name;
    Phone = phone;
    return Result.Success;
  }

  public Result<Success> Deactivate()
  {
    if (!IsActive)
      return SupplierErrors.AlreadyInactive;

    IsActive = false;
    return Result.Success;
  }

  public Result<Success> Activate()
  {
    if (IsActive)
      return SupplierErrors.AlreadyActive;

    IsActive = true;
    return Result.Success;
  }
}
