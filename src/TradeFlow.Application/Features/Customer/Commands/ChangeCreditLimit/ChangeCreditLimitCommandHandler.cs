namespace TradeFlow.Application.Customers.Commands.ChangeCreditLimit;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Customers;

public sealed class ChangeCreditLimitCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ChangeCreditLimitCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ChangeCreditLimitCommand request, CancellationToken ct)
  {
    var customer = await context.Customers
        .FirstOrDefaultAsync(c => c.Id == new CustomerId(request.CustomerId), ct);

    if (customer is null)
      return CustomerErrors.NotFound;

    var limitResult = Money.EGP(request.NewLimit);
    if (limitResult.IsError)
      return limitResult.Errors;

    var result = customer.ChangeCreditLimit(limitResult.Value);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
