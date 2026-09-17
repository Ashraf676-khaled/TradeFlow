namespace TradeFlow.Domain.Customers;

using TradeFlow.Domain.Common.Results;

public static class CustomerErrors
{
  public static readonly Error NameRequired = Error.Validation(
      "Customer.NameRequired", "Customer name is required.");

  public static readonly Error NameTooLong = Error.Validation(
      "Customer.NameTooLong", "Customer name cannot exceed 200 characters.");

  public static readonly Error CreditLimitExceeded = Error.Conflict(
      "Customer.CreditLimitExceeded", "This order would exceed the customer's credit limit.");

  public static readonly Error InactiveCustomer = Error.Validation(
      "Customer.InactiveCustomer", "Cannot create an order for an inactive customer.");

  public static readonly Error AlreadyInactive = Error.Conflict(
      "Customer.AlreadyInactive", "Customer is already inactive.");

  public static readonly Error AlreadyActive = Error.Conflict(
      "Customer.AlreadyActive", "Customer is already active.");

  public static readonly Error NotFound = Error.NotFound(
      "Customer.NotFound", "Customer not found.");
}
