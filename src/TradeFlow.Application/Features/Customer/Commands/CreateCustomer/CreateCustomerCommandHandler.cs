namespace TradeFlow.Application.Customers.Commands.CreateCustomer;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Customers;

public sealed class CreateCustomerCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "Unable to determine the current company.");

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

    var phoneExists = await context.Customers.AnyAsync(c => c.Phone.Value == phoneResult.Value.Value, ct);
    if (phoneExists)
      return CustomerErrors.PhoneAlreadyExists;

    var creditLimitResult = Money.EGP(request.CreditLimit);
    if (creditLimitResult.IsError)
      return creditLimitResult.Errors;

    var result = Customer.Create(
        new TenantId(currentUser.TenantId.Value), request.Name, phoneResult.Value, creditLimitResult.Value, email);

    if (result.IsError)
      return result.Errors;

    context.Customers.Add(result.Value);
    await context.SaveChangesAsync(ct);

    return result.Value.Id.Value;
  }
}
