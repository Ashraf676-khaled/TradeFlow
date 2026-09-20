namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.ReceivePurchaseOrderItem;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Purchasing;

public sealed class ReceivePurchaseOrderItemCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReceivePurchaseOrderItemCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ReceivePurchaseOrderItemCommand request, CancellationToken ct)
  {
    var order = await context.PurchaseOrders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == new PurchaseOrderId(request.PurchaseOrderId), ct);

    if (order is null)
      return PurchaseOrderErrors.NotFound;

    var quantityResult = Quantity.Create(request.ReceivedQuantity);
    if (quantityResult.IsError)
      return quantityResult.Errors;

    // ReceiveItem بتطلق PurchaseOrderReceivedDomainEvent تلقائيًا،
    // وده اللي بيحرك تحديث الـStockItem عبر Handler منفصل (Loose Coupling بين الـAggregates)
    var result = order.ReceiveItem(new ProductId(request.ProductId), quantityResult.Value);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
