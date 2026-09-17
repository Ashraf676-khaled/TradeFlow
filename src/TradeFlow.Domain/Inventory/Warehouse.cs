namespace TradeFlow.Domain.Inventory;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;

public sealed class Warehouse : AggregateRoot, IAuditableEntity
{
  public new WarehouseId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public string Name { get; private set; } = string.Empty;
  public string Location { get; private set; } = string.Empty;
  public bool IsActive { get; private set; }

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private Warehouse() { } // EF Core

  private Warehouse(WarehouseId id, TenantId tenantId, string name, string location)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    Name = name;
    Location = location;
    IsActive = true;
  }

  public static Result<Warehouse> Create(TenantId tenantId, string name, string location)
  {
    if (string.IsNullOrWhiteSpace(name))
      return WarehouseErrors.NameRequired;

    if (name.Length > 150)
      return WarehouseErrors.NameTooLong;

    if (string.IsNullOrWhiteSpace(location))
      return WarehouseErrors.LocationRequired;

    return new Warehouse(WarehouseId.New(), tenantId, name, location);
  }

  public Result<Success> Rename(string newName)
  {
    if (string.IsNullOrWhiteSpace(newName))
      return WarehouseErrors.NameRequired;

    if (newName.Length > 150)
      return WarehouseErrors.NameTooLong;

    Name = newName;
    return Result.Success;
  }

  public Result<Success> ChangeLocation(string newLocation)
  {
    if (string.IsNullOrWhiteSpace(newLocation))
      return WarehouseErrors.LocationRequired;

    Location = newLocation;
    return Result.Success;
  }

  public Result<Success> Deactivate()
  {
    if (!IsActive)
      return WarehouseErrors.AlreadyInactive;

    IsActive = false;
    return Result.Success;
  }

  public Result<Success> Activate()
  {
    if (IsActive)
      return WarehouseErrors.AlreadyActive;

    IsActive = true;
    return Result.Success;
  }

  public Result<Success> EnsureOperational()
  {
    if (!IsActive)
      return WarehouseErrors.NotOperational;

    return Result.Success;
  }
}
