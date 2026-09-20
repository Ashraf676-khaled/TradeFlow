namespace TradeFlow.Application.Purchasing.Suppliers.Commands.ChangeSupplierPhone;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Purchasing;

public sealed class ChangeSupplierPhoneCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ChangeSupplierPhoneCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ChangeSupplierPhoneCommand request, CancellationToken ct)
  {
    var supplier = await context.Suppliers
        .FirstOrDefaultAsync(s => s.Id == new SupplierId(request.SupplierId), ct);

    if (supplier is null)
      return SupplierErrors.NotFound;

    var phoneResult = PhoneNumber.Create(request.NewPhone);
    if (phoneResult.IsError)
      return phoneResult.Errors;

    var phoneExists = await context.Suppliers
        .AnyAsync(s => s.Phone.Value == phoneResult.Value.Value && s.Id != supplier.Id, ct);
    if (phoneExists)
      return SupplierErrors.PhoneAlreadyExists;

    var result = supplier.UpdateContactInfo(supplier.Name, phoneResult.Value);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
