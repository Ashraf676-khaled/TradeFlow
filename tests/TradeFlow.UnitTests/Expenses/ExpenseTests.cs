namespace TradeFlow.Domain.Tests.Expenses;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Expenses;
using Xunit;

public sealed class ExpenseTests
{
  [Fact]
  public void Create_WithValidData_ShouldSucceed()
  {
    var incurredAt = DateTimeOffset.UtcNow;
    var result = Expense.Create(
        TenantId.New(),
        "فاتورة كهرباء",
        "كهرباء ومياه وإنترنت",
        "ثابت",
        Money.EGP(1500).Value,
        incurredAt);

    Assert.True(result.IsSuccess);
    Assert.Equal("فاتورة كهرباء", result.Value.Description);
    Assert.Equal(1500, result.Value.Amount.Amount);
    Assert.Equal(incurredAt, result.Value.IncurredAt);
  }

  [Fact]
  public void Create_WithZeroAmount_ShouldFail()
  {
    var result = Expense.Create(
        TenantId.New(),
        "صيانة",
        "صيانة",
        "متغير",
        Money.Zero(),
        DateTimeOffset.UtcNow);

    Assert.True(result.IsError);
    Assert.Equal(ExpenseErrors.InvalidAmount, result.TopError);
  }

  [Fact]
  public void Create_WithUnsupportedClassification_ShouldFail()
  {
    var result = Expense.Create(
        TenantId.New(),
        "مصاريف",
        "أخرى",
        "غير معروف",
        Money.EGP(10).Value,
        DateTimeOffset.UtcNow);

    Assert.True(result.IsError);
    Assert.Equal(ExpenseErrors.InvalidClassification, result.TopError);
  }
}
