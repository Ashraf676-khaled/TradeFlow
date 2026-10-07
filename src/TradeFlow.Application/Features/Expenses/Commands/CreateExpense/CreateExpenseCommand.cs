namespace TradeFlow.Application.Expenses.Commands.CreateExpense;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CreateExpenseCommand(
    string Description,
    string Category,
    string Classification,
    decimal Amount,
    DateTimeOffset IncurredAt) : IRequest<Result<Guid>>;
