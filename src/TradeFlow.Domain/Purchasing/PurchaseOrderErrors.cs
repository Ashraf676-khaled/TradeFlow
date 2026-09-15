namespace TradeFlow.Domain.Purchasing;

using TradeFlow.Domain.Common.Results;

public static class PurchaseOrderErrors
{
  public static readonly Error NotInDraftStatus = Error.Conflict(
      "PurchaseOrder.NotInDraftStatus", "This operation is only allowed while the order is in Draft status.");

  public static readonly Error EmptyOrder = Error.Validation(
      "PurchaseOrder.EmptyOrder", "Purchase order must contain at least one item to be submitted.");

  public static readonly Error NotSubmitted = Error.Conflict(
      "PurchaseOrder.NotSubmitted", "Only submitted orders can be approved.");

  public static readonly Error NotApproved = Error.Conflict(
      "PurchaseOrder.NotApproved", "Only approved orders can receive stock.");

  public static readonly Error ItemNotFound = Error.NotFound(
      "PurchaseOrder.ItemNotFound", "The specified product is not part of this purchase order.");

  public static readonly Error InvalidUnitCost = Error.Validation(
      "PurchaseOrder.InvalidUnitCost", "Unit cost must be greater than zero.");

  public static readonly Error ReceivedExceedsOrdered = Error.Validation(
      "PurchaseOrder.ReceivedExceedsOrdered", "Received quantity cannot exceed ordered quantity.");

  public static readonly Error CannotCancelReceivedOrder = Error.Conflict(
      "PurchaseOrder.CannotCancelReceivedOrder", "Cannot cancel an order that has already received stock.");

  public static readonly Error AlreadyCancelled = Error.Conflict(
      "PurchaseOrder.AlreadyCancelled", "Purchase order is already cancelled.");

  public static readonly Error NotFound = Error.NotFound(
      "PurchaseOrder.NotFound", "Purchase order not found.");
}
