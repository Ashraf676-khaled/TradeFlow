namespace TradeFlow.Application.Sales.Invoices.Commands.CancelInvoice;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Sales;

public sealed class CancelInvoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelInvoiceCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(CancelInvoiceCommand request, CancellationToken ct)
  {
    var invoice = await context.Invoices
        .Include(i => i.Payments)
        .FirstOrDefaultAsync(i => i.Id == new InvoiceId(request.InvoiceId), ct);

    if (invoice is null)
      return InvoiceErrors.NotFound;

    var result = invoice.Cancel();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
