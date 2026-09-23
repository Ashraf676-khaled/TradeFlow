namespace TradeFlow.Application.Sales.Invoices.Commands.RegisterPayment;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Sales;

public sealed class RegisterPaymentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RegisterPaymentCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(RegisterPaymentCommand request, CancellationToken ct)
  {
    var invoice = await context.Invoices
        .Include(i => i.Payments)
        .FirstOrDefaultAsync(i => i.Id == new InvoiceId(request.InvoiceId), ct);

    if (invoice is null)
      return InvoiceErrors.NotFound;

    var amountResult = Money.EGP(request.Amount);
    if (amountResult.IsError)
      return amountResult.Errors;

    var result = invoice.RegisterPayment(amountResult.Value);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
