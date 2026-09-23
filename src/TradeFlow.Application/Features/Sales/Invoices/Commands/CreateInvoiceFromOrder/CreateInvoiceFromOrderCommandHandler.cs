namespace TradeFlow.Application.Sales.Invoices.Commands.CreateInvoiceFromOrder;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Sales;

public sealed class CreateInvoiceFromOrderCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateInvoiceFromOrderCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateInvoiceFromOrderCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "Unable to determine the current company.");

    var order = await context.SalesOrders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == new SalesOrderId(request.SalesOrderId), ct);

    if (order is null)
      return SalesOrderErrors.NotFound;

    if (order.Status is not (OrderStatus.Confirmed or OrderStatus.Completed))
      return SalesOrderErrors.NotConfirmed;

    var alreadyInvoiced = await context.Invoices.AnyAsync(i => i.SalesOrderId == order.Id, ct);
    if (alreadyInvoiced)
      return InvoiceErrors.AlreadyInvoiced;

    var totalResult = order.CalculateTotal();
    if (totalResult.IsError)
      return totalResult.Errors;

    var sequence = await context.Invoices.CountAsync(ct) + 1;
    var invoiceNumberResult = DocumentNumber.Generate("INV", sequence);
    if (invoiceNumberResult.IsError)
      return invoiceNumberResult.Errors;

    var invoiceResult = Invoice.Create(
        new TenantId(currentUser.TenantId.Value),
        order.Id,
        order.CustomerId,
        invoiceNumberResult.Value,
        totalResult.Value,
        DateTimeOffset.UtcNow.AddDays(request.DueInDays));

    if (invoiceResult.IsError)
      return invoiceResult.Errors;

    context.Invoices.Add(invoiceResult.Value);
    await context.SaveChangesAsync(ct);

    return invoiceResult.Value.Id.Value;
  }
}
