namespace TradeFlow.Application.Customers.Commands.ActivateCustomer;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Customers;

public sealed class ActivateCustomerCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateCustomerCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ActivateCustomerCommand request, CancellationToken ct)
  {
    var customer = await context.Customers
        .FirstOrDefaultAsync(c => c.Id == new CustomerId(request.CustomerId), ct);

    if (customer is null)
      return CustomerErrors.NotFound;

    var result = customer.Activate();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
