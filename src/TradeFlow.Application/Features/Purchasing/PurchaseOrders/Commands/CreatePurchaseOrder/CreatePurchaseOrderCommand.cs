namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CreatePurchaseOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record PurchaseOrderItemInput(Guid ProductId, int Quantity, decimal UnitCost);

public sealed record CreatePurchaseOrderCommand(
    Guid SupplierId,
    Guid WarehouseId,
    List<PurchaseOrderItemInput> Items) : IRequest<Result<Guid>>;
