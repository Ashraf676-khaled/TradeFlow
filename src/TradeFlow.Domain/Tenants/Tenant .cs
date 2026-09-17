namespace TradeFlow.Domain.Tenants;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;

public sealed class Tenant : AggregateRoot, IAuditableEntity
{
  public new TenantId Id { get; private set; }
  public string Name { get; private set; } = string.Empty;
  public bool IsActive { get; private set; }

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private Tenant() { } // EF Core

  private Tenant(TenantId id, string name) : base(id.Value)
  {
    Id = id;
    Name = name;
    IsActive = true;
  }

  public static Result<Tenant> Create(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return TenantErrors.NameRequired;

    if (name.Length > 200)
      return TenantErrors.NameTooLong;

    return new Tenant(TenantId.New(), name);
  }

  public Result<Success> Deactivate()
  {
    if (!IsActive)
      return TenantErrors.AlreadyInactive;

    IsActive = false;
    return Result.Success;
  }
}
