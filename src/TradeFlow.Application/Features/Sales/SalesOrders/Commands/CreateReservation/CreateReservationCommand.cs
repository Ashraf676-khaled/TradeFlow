namespace TradeFlow.Application.Sales.SalesOrders.Commands.CreateReservation;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ReservationItemInput(Guid ProductId, int Quantity, decimal UnitPrice);

public sealed record CreateReservationCommand(
    Guid CustomerId,
    Guid WarehouseId,
    List<ReservationItemInput> Items) : IRequest<Result<Guid>>;