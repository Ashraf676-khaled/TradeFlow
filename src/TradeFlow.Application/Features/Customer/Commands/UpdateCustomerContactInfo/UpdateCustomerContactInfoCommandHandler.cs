namespace TradeFlow.Application.Customers.Commands.UpdateCustomerContactInfo;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Customers;

public sealed class UpdateCustomerContactInfoCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCustomerContactInfoCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(UpdateCustomerContactInfoCommand request, CancellationToken ct)
  {
    var customer = await context.Customers
        .FirstOrDefaultAsync(c => c.Id == new CustomerId(request.CustomerId), ct);

    if (customer is null)
      return CustomerErrors.NotFound;

    var phoneResult = PhoneNumber.Create(request.Phone);
    if (phoneResult.IsError)
      return phoneResult.Errors;

    Email? email = null;
    if (!string.IsNullOrWhiteSpace(request.Email))
    {
      var emailResult = Email.Create(request.Email);
      if (emailResult.IsError)
        return emailResult.Errors;
      email = emailResult.Value;
    }

    var result = customer.UpdateContactInfo(phoneResult.Value, email);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
