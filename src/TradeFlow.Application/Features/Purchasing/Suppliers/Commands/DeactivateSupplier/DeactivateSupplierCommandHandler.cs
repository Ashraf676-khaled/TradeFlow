namespace TradeFlow.Application.Purchasing.Suppliers.Commands.DeactivateSupplier;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Purchasing;

public sealed class DeactivateSupplierCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateSupplierCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(DeactivateSupplierCommand request, CancellationToken ct)
  {
    var supplier = await context.Suppliers
        .FirstOrDefaultAsync(s => s.Id == new SupplierId(request.SupplierId), ct);

    if (supplier is null)
      return SupplierErrors.NotFound;

    var result = supplier.Deactivate();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
