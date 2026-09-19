namespace TradeFlow.Api.Endpoints.Inventory;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Inventory.StockItems.Commands.ReceiveStock;
using TradeFlow.Application.Inventory.StockItems.Commands.TransferStock;
using TradeFlow.Application.Inventory.StockItems.Queries.GetStockByWarehouse;
using TradeFlow.Application.Inventory.StockItems.Queries.GetStockItem;

public sealed class StockItemEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/stock").WithTags("Stock").RequireAuthorization();

    group.MapPost("/receive", async (ReceiveStockCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });

    group.MapPost("/transfer", async (TransferStockCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapGet("/{warehouseId:guid}/{productId:guid}", async (Guid warehouseId, Guid productId, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetStockItemQuery(productId, warehouseId), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapGet("/warehouse/{warehouseId:guid}", async (Guid warehouseId, ISender sender, CancellationToken ct, int pageNumber = 1, int pageSize = 20) =>
    {
      var result = await sender.Send(new GetStockByWarehouseQuery(warehouseId, pageNumber, pageSize), ct);
      return Results.Ok(result);
    });
  }
}
