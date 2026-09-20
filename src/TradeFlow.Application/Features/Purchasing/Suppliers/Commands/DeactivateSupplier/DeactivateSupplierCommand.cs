namespace TradeFlow.Application.Purchasing.Suppliers.Commands.DeactivateSupplier;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record DeactivateSupplierCommand(Guid SupplierId) : IRequest<Result<Success>>;
