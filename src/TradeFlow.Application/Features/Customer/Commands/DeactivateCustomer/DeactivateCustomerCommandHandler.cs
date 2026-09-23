namespace TradeFlow.Application.Customers.Commands.DeactivateCustomer;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Customers;

public sealed class DeactivateCustomerCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateCustomerCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(DeactivateCustomerCommand request, CancellationToken ct)
  {
    var customer = await context.Customers
        .FirstOrDefaultAsync(c => c.Id == new CustomerId(request.CustomerId), ct);

    if (customer is null)
      return CustomerErrors.NotFound;

    var result = customer.Deactivate();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
