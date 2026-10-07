namespace TradeFlow.Application.Expenses.Queries.GetExpenseSummary;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Sales;

public sealed class GetExpenseSummaryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetExpenseSummaryQuery, ExpenseSummaryDto>
{
  public async Task<ExpenseSummaryDto> Handle(GetExpenseSummaryQuery request, CancellationToken ct)
  {
    var expensesTotal = await context.Expenses
        .Where(expense => expense.IncurredAt >= request.From && expense.IncurredAt < request.To)
        .Select(expense => expense.Amount.Amount)
        .SumAsync(ct);

    var saleLines = await context.SalesOrders
        .Where(order => order.OrderDate >= request.From
            && order.OrderDate < request.To
            && (order.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Completed))
        .SelectMany(order => order.Items)
        .Select(item => new
        {
          item.ProductId,
          UnitPrice = item.UnitPrice.Amount,
          Quantity = item.Quantity.Value,
          DiscountPercentage = item.ItemDiscount.Percentage,
        })
        .ToListAsync(ct);

    var productIds = saleLines.Select(line => line.ProductId).Distinct().ToArray();
    var costs = await context.Products
        .Where(product => productIds.Contains(product.Id))
        .Select(product => new { product.Id, Cost = product.Cost.Amount })
        .ToDictionaryAsync(product => product.Id, product => product.Cost, ct);

    var grossProfit = saleLines.Sum(line =>
    {
      if (!costs.TryGetValue(line.ProductId, out var cost)) return 0m;
      var discountedSellingPrice = line.UnitPrice * (1m - line.DiscountPercentage / 100m);
      return (discountedSellingPrice - cost) * line.Quantity;
    });

    return new ExpenseSummaryDto(grossProfit, expensesTotal, grossProfit - expensesTotal);
  }
}
