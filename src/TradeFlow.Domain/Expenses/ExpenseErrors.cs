using TradeFlow.Domain.Common.Results;

public static class ExpenseErrors
{
  public static readonly Error InvalidDescription = Error.Validation(
      "Expense.InvalidDescription", "Expense description is required and must not exceed 200 characters.");
  public static readonly Error InvalidCategory = Error.Validation(
      "Expense.InvalidCategory", "Expense category is required and must not exceed 60 characters.");
  public static readonly Error InvalidClassification = Error.Validation(
      "Expense.InvalidClassification", "Expense classification must be either ثابت or متغير.");
  public static readonly Error InvalidAmount = Error.Validation(
      "Expense.InvalidAmount", "Expense amount must be greater than zero.");
  public static readonly Error InvalidDate = Error.Validation(
      "Expense.InvalidDate", "Expense date is invalid.");
}
