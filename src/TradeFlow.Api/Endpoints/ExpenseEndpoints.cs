namespace TradeFlow.Api.Endpoints;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Expenses.Commands.CreateExpense;
using TradeFlow.Application.Expenses.Queries.GetExpenseSummary;
using TradeFlow.Application.Expenses.Queries.GetExpenses;

public sealed class ExpenseEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/expenses").WithTags("Expenses").RequireAuthorization();

    group.MapGet("/", async (
        ISender sender,
        CancellationToken ct,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null) =>
    {
      var result = await sender.Send(new GetExpensesQuery(from, to), ct);
      return Results.Ok(result);
    });

    group.MapGet("/summary", async (
        ISender sender,
        CancellationToken ct,
        DateTimeOffset from,
        DateTimeOffset to) =>
    {
      if (from >= to)
        return Results.BadRequest(new { detail = "تاريخ بداية الفترة يجب أن يسبق تاريخ نهايتها." });
      var result = await sender.Send(new GetExpenseSummaryQuery(from, to), ct);
      return Results.Ok(result);
    });

    group.MapPost("/", async (CreateExpenseCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });
  }
}
