namespace TradeFlow.Application.Purchasing.Suppliers.Queries.GetSuppliers;

using MediatR;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Features.Purchasing.Suppliers.Dtos;
using TradeFlow.Application.Purchasing.Suppliers;

public sealed record GetSuppliersQuery(int PageNumber = 1, int PageSize = 20, bool? IsActive = null)
    : IRequest<PaginatedList<SupplierDto>>;
