namespace TradeFlow.Api.IntegrationTests.Warehouses;

using System.Net;
using System.Net.Http.Json;
using TradeFlow.Api.IntegrationTests.Common;
using Xunit;

public class WarehouseEndpointsTests : IntegrationTestBase
{
  public WarehouseEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

  [Fact]
  public async Task PostCreateWarehouse_WithValidData_ShouldReturnOkWithId()
  {
    await RegisterAndLoginAsync();

    var name = $"WH-{Guid.NewGuid():N}"[..10];
    var response = await Client.PostAsJsonAsync("/api/warehouses", new { name, location = "Cairo" });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadFromJsonAsync<IdResponse>();
    Assert.NotNull(body);
    Assert.NotEqual(Guid.Empty, body!.Id);
  }

  [Fact]
  public async Task GetCreatedWarehouse_ById_ShouldReturnCreatedWarehouse()
  {
    await RegisterAndLoginAsync();

    var name = $"WH-{Guid.NewGuid():N}"[..10];
    var createResponse = await Client.PostAsJsonAsync("/api/warehouses", new { name, location = "Cairo" });
    Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
    var id = (await createResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

    var getResponse = await Client.GetAsync($"/api/warehouses/{id}");

    Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    var warehouse = await getResponse.Content.ReadFromJsonAsync<WarehouseResponse>();
    Assert.NotNull(warehouse);
    Assert.Equal(name, warehouse!.Name);
    Assert.Equal("Cairo", warehouse.Location);
  }

  [Fact]
  public async Task GetWarehousesList_ShouldIncludeCreatedWarehouse()
  {
    await RegisterAndLoginAsync();

    var name = $"WH-{Guid.NewGuid():N}"[..10];
    var createResponse = await Client.PostAsJsonAsync("/api/warehouses", new { name, location = "Cairo" });
    Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

    var listResponse = await Client.GetFromJsonAsync<PaginatedWarehousesResponse>("/api/warehouses?pageNumber=1&pageSize=50");

    Assert.NotNull(listResponse);
    Assert.Contains(listResponse!.Items, w => w.Name == name);
  }

  [Fact]
  public async Task PostWarehouse_WithoutToken_ShouldReturnUnauthorized()
  {
    var response = await Client.PostAsJsonAsync("/api/warehouses", new { name = "No Auth WH", location = "Cairo" });

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  private sealed record IdResponse(Guid Id);
  private sealed record WarehouseResponse(Guid Id, string Name, string Location);
  private sealed record PaginatedWarehousesResponse(IReadOnlyList<WarehouseResponse> Items);
}
