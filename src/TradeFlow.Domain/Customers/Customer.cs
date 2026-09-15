namespace TradeFlow.Domain.Customers;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class Customer : AggregateRoot, IAuditableEntity
{
  public new CustomerId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public string Name { get; private set; } = string.Empty;
  public PhoneNumber Phone { get; private set; } = null!;
  public Email? Email { get; private set; }
  public Money CreditLimit { get; private set; } = null!;
  public Money CurrentBalance { get; private set; } = null!;
  public bool IsActive { get; private set; }

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private Customer() { } // EF Core

  private Customer(
      CustomerId id,
      TenantId tenantId,
      string name,
      PhoneNumber phone,
      Email? email,
      Money creditLimit)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    Name = name;
    Phone = phone;
    Email = email;
    CreditLimit = creditLimit;
    CurrentBalance = Money.Zero();
    IsActive = true;
  }

  public static Result<Customer> Create(
      TenantId tenantId,
      string name,
      PhoneNumber phone,
      Money creditLimit,
      Email? email = null)
  {
    if (string.IsNullOrWhiteSpace(name))
      return CustomerErrors.NameRequired;

    if (name.Length > 200)
      return CustomerErrors.NameTooLong;

    return new Customer(CustomerId.New(), tenantId, name, phone, email, creditLimit);
  }

  public Result<Success> ValidateCredit(Money orderAmount)
  {
    if (!IsActive)
      return CustomerErrors.InactiveCustomer;

    var projectedBalance = CurrentBalance.Add(orderAmount);
    if (projectedBalance.IsError)
      return projectedBalance.Errors;

    if (projectedBalance.Value.Amount > CreditLimit.Amount)
      return CustomerErrors.CreditLimitExceeded;

    return Result.Success;
  }

  public Result<Success> IncreaseBalance(Money amount)
  {
    var result = CurrentBalance.Add(amount);
    if (result.IsError)
      return result.Errors;

    CurrentBalance = result.Value;
    return Result.Success;
  }

  public Result<Success> DecreaseBalance(Money amount)
  {
    var result = CurrentBalance.Subtract(amount);
    if (result.IsError)
      return result.Errors;

    CurrentBalance = result.Value;
    return Result.Success;
  }

  public Result<Success> ChangeCreditLimit(Money newLimit)
  {
    CreditLimit = newLimit;
    return Result.Success;
  }

  public Result<Success> UpdateContactInfo(PhoneNumber phone, Email? email)
  {
    Phone = phone;
    Email = email;
    return Result.Success;
  }

  public Result<Success> Deactivate()
  {
    if (!IsActive)
      return CustomerErrors.AlreadyInactive;

    IsActive = false;
    return Result.Success;
  }

  public Result<Success> Activate()
  {
    if (IsActive)
      return CustomerErrors.AlreadyActive;

    IsActive = true;
    return Result.Success;
  }
}
