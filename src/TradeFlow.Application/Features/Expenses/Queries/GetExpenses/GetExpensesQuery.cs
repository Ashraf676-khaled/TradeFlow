namespace TradeFlow.Application.Expenses.Queries.GetExpenses;

using MediatR;

public sealed record GetExpensesQuery(
    DateTimeOffset? From,
    DateTimeOffset? To) : IRequest<IReadOnlyList<ExpenseDto>>;

public sealed record ExpenseDto(
    Guid Id,
    string Description,
    string Category,
    string Classification,
    decimal Amount,
    string Currency,
    DateTimeOffset IncurredAt);
