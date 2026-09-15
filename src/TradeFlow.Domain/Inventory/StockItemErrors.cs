namespace TradeFlow.Domain.Inventory;

using TradeFlow.Domain.Common.Results;

public static class StockItemErrors
{
  public static readonly Error InsufficientStock = Error.Conflict(
      "StockItem.InsufficientStock", "Insufficient available stock for this operation.");

  public static readonly Error CannotReleaseMoreThanReserved = Error.Validation(
      "StockItem.CannotReleaseMoreThanReserved", "Cannot release more quantity than currently reserved.");

  public static readonly Error CannotDeductMoreThanReserved = Error.Validation(
      "StockItem.CannotDeductMoreThanReserved", "Cannot deduct more quantity than currently reserved.");

  public static readonly Error NotFound = Error.NotFound(
      "StockItem.NotFound", "Stock item not found for this product in this warehouse.");
}
