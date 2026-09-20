namespace TradeFlow.Application.Purchasing.Suppliers.Commands.RenameSupplier;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record RenameSupplierCommand(Guid SupplierId, string NewName) : IRequest<Result<Success>>;
