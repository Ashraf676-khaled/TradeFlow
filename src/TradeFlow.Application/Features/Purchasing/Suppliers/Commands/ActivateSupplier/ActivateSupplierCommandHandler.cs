namespace TradeFlow.Application.Purchasing.Suppliers.Commands.ActivateSupplier;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Purchasing;

public sealed class ActivateSupplierCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateSupplierCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ActivateSupplierCommand request, CancellationToken ct)
  {
    var supplier = await context.Suppliers
        .FirstOrDefaultAsync(s => s.Id == new SupplierId(request.SupplierId), ct);

    if (supplier is null)
      return SupplierErrors.NotFound;

    var result = supplier.Activate();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
