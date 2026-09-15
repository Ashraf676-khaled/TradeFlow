namespace TradeFlow.Domain.Sales;

using TradeFlow.Domain.Common.Results;

public static class SalesOrderErrors
{
  public static readonly Error NotInDraftStatus = Error.Conflict(
      "SalesOrder.NotInDraftStatus", "This operation is only allowed while the order is in Draft status.");

  public static readonly Error EmptyOrder = Error.Validation(
      "SalesOrder.EmptyOrder", "Order must contain at least one item to be confirmed.");

  public static readonly Error ItemNotFound = Error.NotFound(
      "SalesOrder.ItemNotFound", "The specified product is not part of this order.");

  public static readonly Error InvalidUnitPrice = Error.Validation(
      "SalesOrder.InvalidUnitPrice", "Unit price must be greater than zero.");

  public static readonly Error CannotCancelCompletedOrder = Error.Conflict(
      "SalesOrder.CannotCancelCompletedOrder", "A completed order cannot be cancelled.");

  public static readonly Error AlreadyCancelled = Error.Conflict(
      "SalesOrder.AlreadyCancelled", "Order is already cancelled.");

  public static readonly Error NotConfirmed = Error.Conflict(
      "SalesOrder.NotConfirmed", "Only confirmed orders can be completed.");

  public static readonly Error NotFound = Error.NotFound(
      "SalesOrder.NotFound", "Sales order not found.");
}
