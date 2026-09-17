namespace TradeFlow.Domain.Purchasing;

using TradeFlow.Domain.Common.Results;

public static class SupplierErrors
{
  public static readonly Error NameRequired = Error.Validation(
      "Supplier.NameRequired", "Supplier name is required.");

  public static readonly Error NameTooLong = Error.Validation(
      "Supplier.NameTooLong", "Supplier name cannot exceed 200 characters.");

  public static readonly Error AlreadyInactive = Error.Conflict(
      "Supplier.AlreadyInactive", "Supplier is already inactive.");

  public static readonly Error AlreadyActive = Error.Conflict(
      "Supplier.AlreadyActive", "Supplier is already active.");

  public static readonly Error NotFound = Error.NotFound(
      "Supplier.NotFound", "Supplier not found.");
}
