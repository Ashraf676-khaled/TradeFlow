namespace TradeFlow.Application.Sales.Invoices.Commands.RegisterPayment;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record RegisterPaymentCommand(Guid InvoiceId, decimal Amount) : IRequest<Result<Success>>;
