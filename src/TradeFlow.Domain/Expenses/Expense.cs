namespace TradeFlow.Domain.Expenses;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class Expense : AggregateRoot, IAuditableEntity
{
  public new ExpenseId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public string Description { get; private set; } = string.Empty;
  public string Category { get; private set; } = string.Empty;
  public string Classification { get; private set; } = string.Empty;
  public Money Amount { get; private set; } = null!;
  public DateTimeOffset IncurredAt { get; private set; }

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private Expense() { }

  private Expense(
      ExpenseId id,
      TenantId tenantId,
      string description,
      string category,
      string classification,
      Money amount,
      DateTimeOffset incurredAt)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    Description = description;
    Category = category;
    Classification = classification;
    Amount = amount;
    IncurredAt = incurredAt;
  }

  public static Result<Expense> Create(
      TenantId tenantId,
      string description,
      string category,
      string classification,
      Money amount,
      DateTimeOffset incurredAt)
  {
    if (string.IsNullOrWhiteSpace(description) || description.Length > 200)
      return ExpenseErrors.InvalidDescription;
    if (string.IsNullOrWhiteSpace(category) || category.Length > 60)
      return ExpenseErrors.InvalidCategory;
    if (classification is not ("ثابت" or "متغير"))
      return ExpenseErrors.InvalidClassification;
    if (amount.Amount <= 0)
      return ExpenseErrors.InvalidAmount;
    if (incurredAt == default)
      return ExpenseErrors.InvalidDate;

    return new Expense(
        ExpenseId.New(),
        tenantId,
        description.Trim(),
        category.Trim(),
        classification,
        amount,
        incurredAt);
  }
}

public static class ExpenseErrors
{
  public static readonly Error InvalidDescription = Error.Validation(
      "Expense.InvalidDescription", "وصف المصروف مطلوب وبحد أقصى 200 حرف.");
  public static readonly Error InvalidCategory = Error.Validation(
      "Expense.InvalidCategory", "تصنيف المصروف مطلوب وبحد أقصى 60 حرفًا.");
  public static readonly Error InvalidClassification = Error.Validation(
      "Expense.InvalidClassification", "نوع المصروف يجب أن يكون ثابتًا أو متغيرًا.");
  public static readonly Error InvalidAmount = Error.Validation(
      "Expense.InvalidAmount", "قيمة المصروف يجب أن تكون أكبر من صفر.");
  public static readonly Error InvalidDate = Error.Validation(
      "Expense.InvalidDate", "تاريخ المصروف غير صالح.");
}
