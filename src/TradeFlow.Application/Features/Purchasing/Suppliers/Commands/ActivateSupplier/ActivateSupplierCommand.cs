namespace TradeFlow.Application.Purchasing.Suppliers.Commands.ActivateSupplier;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ActivateSupplierCommand(Guid SupplierId) : IRequest<Result<Success>>;
