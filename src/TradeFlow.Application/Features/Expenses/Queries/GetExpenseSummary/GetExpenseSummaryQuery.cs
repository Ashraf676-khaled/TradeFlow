namespace TradeFlow.Application.Expenses.Queries.GetExpenseSummary;

using MediatR;

public sealed record GetExpenseSummaryQuery(
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<ExpenseSummaryDto>;

public sealed record ExpenseSummaryDto(
    decimal GrossProfit,
    decimal ExpenseTotal,
    decimal NetProfit);
