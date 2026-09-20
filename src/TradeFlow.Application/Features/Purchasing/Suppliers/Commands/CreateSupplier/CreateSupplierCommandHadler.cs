namespace TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Purchasing;

public sealed class CreateSupplierCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateSupplierCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "لا يمكن تحديد الشركة الحالية.");

    var phoneResult = PhoneNumber.Create(request.Phone);
    if (phoneResult.IsError)
      return phoneResult.Errors;

    var nameExists = await context.Suppliers.AnyAsync(s => s.Name == request.Name, ct);
    if (nameExists)
      return SupplierErrors.NameAlreadyExists;

    var phoneExists = await context.Suppliers
        .AnyAsync(s => s.Phone.Value == phoneResult.Value.Value, ct);
    if (phoneExists)
      return SupplierErrors.PhoneAlreadyExists;

    var result = Supplier.Create(new TenantId(currentUser.TenantId.Value), request.Name, phoneResult.Value);
    if (result.IsError)
      return result.Errors;

    context.Suppliers.Add(result.Value);
    await context.SaveChangesAsync(ct);

    return result.Value.Id.Value;
  }
}
