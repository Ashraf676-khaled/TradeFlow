namespace TradeFlow.Domain.Inventory;

using TradeFlow.Domain.Common.Results;

public static class WarehouseErrors
{
  public static readonly Error NameRequired = Error.Validation(
      "Warehouse.NameRequired", "Warehouse name is required.");

  public static readonly Error NameTooLong = Error.Validation(
      "Warehouse.NameTooLong", "Warehouse name cannot exceed 150 characters.");

  public static readonly Error LocationRequired = Error.Validation(
      "Warehouse.LocationRequired", "Warehouse location is required.");

  public static readonly Error AlreadyInactive = Error.Conflict(
      "Warehouse.AlreadyInactive", "Warehouse is already inactive.");

  public static readonly Error AlreadyActive = Error.Conflict(
      "Warehouse.AlreadyActive", "Warehouse is already active.");

  public static readonly Error NotOperational = Error.Validation(
      "Warehouse.NotOperational", "Cannot operate on an inactive warehouse.");

  public static readonly Error HasStockOrActiveOrders = Error.Conflict(
      "Warehouse.HasStockOrActiveOrders", "Cannot deactivate a warehouse that still has stock or active orders.");

  public static readonly Error NotFound = Error.NotFound(
      "Warehouse.NotFound", "Warehouse not found.");
}
