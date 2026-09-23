namespace TradeFlow.Api.IntegrationTests.Sales;

using System.Net;
using System.Net.Http.Json;
using TradeFlow.Api.IntegrationTests.Common;
using Xunit;

public class SalesOrderFullFlowTests : IntegrationTestBase
{
  public SalesOrderFullFlowTests(CustomWebApplicationFactory factory) : base(factory) { }

  private static async Task EnsureSuccessWithDetailsAsync(HttpResponseMessage response, string step)
  {
    if (!response.IsSuccessStatusCode)
    {
      var body = await response.Content.ReadAsStringAsync();
      var headers = string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"));
      var authHeader = response.RequestMessage?.Headers.Authorization?.ToString() ?? "NULL";

      Assert.Fail(
          $"Step '{step}' failed with {response.StatusCode}: {body}\n" +
          $"Response Headers: {headers}\n" +
          $"Request Authorization Header: {authHeader}");
    }
  }

  [Fact]
  public async Task FullFlow_CreateProductWarehouseCustomerOrderAndConfirm_ShouldReserveStock()
  {
    await RegisterAndLoginAsync();

    var productResponse = await Client.PostAsJsonAsync("/api/products", new
    {
      name = "Steel Door",
      sku = $"SD-{Guid.NewGuid():N}"[..10],
      sellingPrice = 200m,
      cost = 100m,
      minimumStock = 5
    });
    await EnsureSuccessWithDetailsAsync(productResponse, "Create Product");
    var productId = (await productResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var warehouseResponse = await Client.PostAsJsonAsync("/api/warehouses", new
    {
      name = $"WH-{Guid.NewGuid():N}"[..10],
      location = "Cairo"
    });
    await EnsureSuccessWithDetailsAsync(warehouseResponse, "Create Warehouse");
    var warehouseId = (await warehouseResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var receiveResponse = await Client.PostAsJsonAsync("/api/stock/receive", new
    {
      productId,
      warehouseId,
      quantity = 50
    });
    await EnsureSuccessWithDetailsAsync(receiveResponse, "Receive Stock");

    var customerResponse = await Client.PostAsJsonAsync("/api/customers", new
    {
      name = "Ahmed Trading",
      phone = $"010{new Random().Next(10000000, 99999999)}",
      creditLimit = 10000m
    });
    await EnsureSuccessWithDetailsAsync(customerResponse, "Create Customer");
    var customerId = (await customerResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var orderResponse = await Client.PostAsJsonAsync("/api/sales-orders", new
    {
      customerId,
      warehouseId,
      items = new[] { new { productId, quantity = 10, unitPrice = 200m } }
    });
    await EnsureSuccessWithDetailsAsync(orderResponse, "Create Sales Order");
    var orderId = (await orderResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var confirmResponse = await Client.PostAsync($"/api/sales-orders/{orderId}/confirm", null);
    await EnsureSuccessWithDetailsAsync(confirmResponse, "Confirm Sales Order");

    var stockResponse = await Client.GetAsync($"/api/stock/{warehouseId}/{productId}");
    await EnsureSuccessWithDetailsAsync(stockResponse, "Get Stock Item");
    var stock = await stockResponse.Content.ReadFromJsonAsync<StockItemResponseDto>();

    Assert.Equal(10, stock!.ReservedQuantity);
    Assert.Equal(40, stock.AvailableQuantity);
  }

  [Fact]
  public async Task ConfirmOrder_ExceedingCreditLimit_ShouldReturnConflict()
  {
    await RegisterAndLoginAsync();

    var productResponse = await Client.PostAsJsonAsync("/api/products", new
    {
      name = "Expensive Item",
      sku = $"EX-{Guid.NewGuid():N}"[..10],
      sellingPrice = 1000m,
      cost = 500m,
      minimumStock = 1
    });
    await EnsureSuccessWithDetailsAsync(productResponse, "Create Product");
    var productId = (await productResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var warehouseResponse = await Client.PostAsJsonAsync("/api/warehouses", new
    {
      name = $"WH-{Guid.NewGuid():N}"[..10],
      location = "Cairo"
    });
    await EnsureSuccessWithDetailsAsync(warehouseResponse, "Create Warehouse");
    var warehouseId = (await warehouseResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var receiveResponse = await Client.PostAsJsonAsync("/api/stock/receive", new { productId, warehouseId, quantity = 100 });
    await EnsureSuccessWithDetailsAsync(receiveResponse, "Receive Stock");

    var customerResponse = await Client.PostAsJsonAsync("/api/customers", new
    {
      name = "Low Credit Customer",
      phone = $"010{new Random().Next(10000000, 99999999)}",
      creditLimit = 500m
    });
    await EnsureSuccessWithDetailsAsync(customerResponse, "Create Customer");
    var customerId = (await customerResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var orderResponse = await Client.PostAsJsonAsync("/api/sales-orders", new
    {
      customerId,
      warehouseId,
      items = new[] { new { productId, quantity = 5, unitPrice = 1000m } }
    });
    await EnsureSuccessWithDetailsAsync(orderResponse, "Create Sales Order");
    var orderId = (await orderResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var confirmResponse = await Client.PostAsync($"/api/sales-orders/{orderId}/confirm", null);

    Assert.Equal(HttpStatusCode.Conflict, confirmResponse.StatusCode);
  }

  private sealed record IdResponse(Guid Id);
  private sealed record StockItemResponseDto(Guid Id, Guid WarehouseId, Guid ProductId, int AvailableQuantity, int ReservedQuantity);
}
