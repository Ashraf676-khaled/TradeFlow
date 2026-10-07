namespace TradeFlow.Application.Expenses.Queries.GetExpenses;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;

public sealed class GetExpensesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetExpensesQuery, IReadOnlyList<ExpenseDto>>
{
  public async Task<IReadOnlyList<ExpenseDto>> Handle(GetExpensesQuery request, CancellationToken ct)
  {
    var query = context.Expenses.AsNoTracking();
    if (request.From.HasValue)
      query = query.Where(expense => expense.IncurredAt >= request.From.Value);
    if (request.To.HasValue)
      query = query.Where(expense => expense.IncurredAt < request.To.Value);

    var expenses = await query
        .Select(expense => new ExpenseDto(
            expense.Id.Value,
            expense.Description,
            expense.Category,
            expense.Classification,
            expense.Amount.Amount,
            expense.Amount.Currency,
            expense.IncurredAt))
        .ToListAsync(ct);

    // SQLite cannot order DateTimeOffset columns, so sort the tenant-scoped result after projection.
    return expenses.OrderByDescending(expense => expense.IncurredAt).ToArray();
  }
}
