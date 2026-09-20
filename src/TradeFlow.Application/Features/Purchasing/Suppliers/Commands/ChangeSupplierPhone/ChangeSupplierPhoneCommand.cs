namespace TradeFlow.Application.Purchasing.Suppliers.Commands.ChangeSupplierPhone;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ChangeSupplierPhoneCommand(Guid SupplierId, string NewPhone) : IRequest<Result<Success>>;
