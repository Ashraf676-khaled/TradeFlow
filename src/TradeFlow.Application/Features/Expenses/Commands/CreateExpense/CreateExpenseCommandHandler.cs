namespace TradeFlow.Application.Expenses.Commands.CreateExpense;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Expenses;

public sealed class CreateExpenseCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateExpenseCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "تعذر تحديد الشركة الحالية.");

    var amountResult = Money.EGP(request.Amount);
    if (amountResult.IsError)
      return amountResult.Errors;

    var expenseResult = Expense.Create(
        new TenantId(currentUser.TenantId.Value),
        request.Description,
        request.Category,
        request.Classification,
        amountResult.Value,
        request.IncurredAt);
    if (expenseResult.IsError)
      return expenseResult.Errors;

    context.Expenses.Add(expenseResult.Value);
    await context.SaveChangesAsync(ct);
    return expenseResult.Value.Id.Value;
  }
}
