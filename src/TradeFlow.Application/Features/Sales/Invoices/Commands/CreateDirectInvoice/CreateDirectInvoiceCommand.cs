namespace TradeFlow.Application.Sales.Invoices.Commands.CreateDirectInvoice;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record DirectInvoiceItemInput(Guid ProductId, int Quantity, decimal UnitPrice);

public sealed record CreateDirectInvoiceCommand(
    Guid CustomerId,
    Guid WarehouseId,
    List<DirectInvoiceItemInput> Items,
    bool IsCreditSale) : IRequest<Result<Guid>>;