namespace TradeFlow.Application.Purchasing.Suppliers.Commands.RenameSupplier;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Purchasing;

public sealed class RenameSupplierCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RenameSupplierCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(RenameSupplierCommand request, CancellationToken ct)
  {
    var supplier = await context.Suppliers
        .FirstOrDefaultAsync(s => s.Id == new SupplierId(request.SupplierId), ct);

    if (supplier is null)
      return SupplierErrors.NotFound;

    var nameExists = await context.Suppliers
        .AnyAsync(s => s.Name == request.NewName && s.Id != supplier.Id, ct);
    if (nameExists)
      return SupplierErrors.NameAlreadyExists;

    // بنبعت رقم الهاتف الحالي بتاعه عشان الـDomain method محتاجة الاتنين مع بعض
    var result = supplier.UpdateContactInfo(request.NewName, supplier.Phone);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
