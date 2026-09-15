namespace TradeFlow.Domain.Purchasing;

public enum PurchaseOrderStatus
{
  Draft = 0,
  Submitted = 1,
  Approved = 2,
  Received = 3,
  Completed = 4,
  Cancelled = 5
}
