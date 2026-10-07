namespace TradeFlow.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;
using TradeFlow.Api.IntegrationTests.Common;
using Xunit;

public sealed class ExpensesEndpointsTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task CreateExpense_ShouldPersistAndUpdateMonthlySummary()
  {
    await RegisterAndLoginAsync();
    var incurredAt = DateTimeOffset.UtcNow;

    var warehouseResponse = await Client.PostAsJsonAsync("/api/warehouses", new
    {
      name = $"WH-{Guid.NewGuid():N}"[..10],
      location = "Cairo"
    });
    Assert.Equal(HttpStatusCode.OK, warehouseResponse.StatusCode);
    var warehouseId = (await warehouseResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var productResponse = await Client.PostAsJsonAsync("/api/products", new
    {
      name = "Profit Test Item",
      sku = $"PT-{Guid.NewGuid():N}"[..10],
      sellingPrice = 200m,
      cost = 100m,
      minimumStock = 1,
      openingStockQuantity = 10,
      openingStockWarehouseId = warehouseId
    });
    Assert.Equal(HttpStatusCode.OK, productResponse.StatusCode);
    var productId = (await productResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var customerResponse = await Client.PostAsJsonAsync("/api/customers", new
    {
      name = "Profit Test Customer",
      phone = $"010{Random.Shared.Next(10000000, 99999999)}",
      creditLimit = 10000m
    });
    Assert.Equal(HttpStatusCode.OK, customerResponse.StatusCode);
    var customerId = (await customerResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var invoiceResponse = await Client.PostAsJsonAsync("/api/invoices/direct", new
    {
      customerId,
      warehouseId,
      isCreditSale = false,
      items = new[] { new { productId, quantity = 2, unitPrice = 200m } }
    });
    Assert.Equal(HttpStatusCode.OK, invoiceResponse.StatusCode);

    var createResponse = await Client.PostAsJsonAsync("/api/expenses", new
    {
      description = "فاتورة كهرباء",
      category = "كهرباء ومياه وإنترنت",
      classification = "ثابت",
      amount = 1250m,
      incurredAt
    });
    Assert.True(
        createResponse.StatusCode == HttpStatusCode.OK,
        $"Create expense failed with {createResponse.StatusCode}: {await createResponse.Content.ReadAsStringAsync()}");

    var listResponse = await Client.GetAsync("/api/expenses");
    Assert.True(
        listResponse.StatusCode == HttpStatusCode.OK,
        $"List expenses failed with {listResponse.StatusCode}: {await listResponse.Content.ReadAsStringAsync()}");
    var expenses = await listResponse.Content.ReadFromJsonAsync<ExpenseResponse[]>();
    Assert.NotNull(expenses);
    var expense = Assert.Single(expenses);
    Assert.Equal("فاتورة كهرباء", expense.Description);
    Assert.Equal(1250m, expense.Amount);

    var monthStart = new DateTimeOffset(incurredAt.Year, incurredAt.Month, 1, 0, 0, 0, TimeSpan.Zero);
    var nextMonth = monthStart.AddMonths(1);
    var summaryResponse = await Client.GetAsync(
        $"/api/expenses/summary?from={Uri.EscapeDataString(monthStart.ToString("O"))}&to={Uri.EscapeDataString(nextMonth.ToString("O"))}");
    Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
    var summary = await summaryResponse.Content.ReadFromJsonAsync<ExpenseSummaryResponse>();
    Assert.NotNull(summary);
    Assert.Equal(200m, summary.GrossProfit);
    Assert.Equal(1250m, summary.ExpenseTotal);
    Assert.Equal(-1050m, summary.NetProfit);
  }

  private sealed record IdResponse(Guid Id);

  private sealed record ExpenseResponse(
      Guid Id,
      string Description,
      string Category,
      string Classification,
      decimal Amount,
      string Currency,
      DateTimeOffset IncurredAt);

  private sealed record ExpenseSummaryResponse(
      decimal GrossProfit,
      decimal ExpenseTotal,
      decimal NetProfit);
}
