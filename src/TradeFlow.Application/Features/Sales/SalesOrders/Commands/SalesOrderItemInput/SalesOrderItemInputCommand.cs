namespace TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record SalesOrderItemInput(Guid ProductId, int Quantity, decimal UnitPrice);

public sealed record CreateSalesOrderCommand(
    Guid CustomerId,
    Guid WarehouseId,
    List<SalesOrderItemInput> Items) : IRequest<Result<Guid>>;
