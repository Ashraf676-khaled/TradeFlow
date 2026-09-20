namespace TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CreateSupplierCommand(string Name, string Phone) : IRequest<Result<Guid>>;
