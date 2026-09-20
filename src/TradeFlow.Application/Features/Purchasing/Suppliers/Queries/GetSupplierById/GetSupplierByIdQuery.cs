namespace TradeFlow.Application.Purchasing.Suppliers.Queries.GetSupplierById;

using MediatR;
using TradeFlow.Application.Features.Purchasing.Suppliers.Dtos;
using TradeFlow.Application.Purchasing.Suppliers;
using TradeFlow.Domain.Common.Results;

public sealed record GetSupplierByIdQuery(Guid SupplierId) : IRequest<Result<SupplierDto>>;
