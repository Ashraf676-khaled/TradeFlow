namespace TradeFlow.Api.Endpoints.Inventory;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Inventory.Warehouses.Commands.ActivateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.ChangeWarehouseLocation;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.DeactivateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.RenameWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouseById;
using TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouses;

public sealed class WarehouseEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/warehouses").WithTags("Warehouses").RequireAuthorization();

    group.MapPost("/", async (CreateWarehouseCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });

    group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetWarehouseByIdQuery(id), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapGet("/", async (ISender sender, CancellationToken ct, int pageNumber = 1, int pageSize = 20, bool? isActive = null) =>
    {
      var result = await sender.Send(new GetWarehousesQuery(pageNumber, pageSize, isActive), ct);
      return Results.Ok(result);
    });

    group.MapPut("/{id:guid}/name", async (Guid id, RenameWarehouseRequest request, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new RenameWarehouseCommand(id, request.NewName), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPut("/{id:guid}/location", async (Guid id, ChangeWarehouseLocationRequest request, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ChangeWarehouseLocationCommand(id, request.NewLocation), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/activate", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ActivateWarehouseCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/deactivate", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new DeactivateWarehouseCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });
  }
  public sealed record RenameWarehouseRequest(string NewName);
  public sealed record ChangeWarehouseLocationRequest(string NewLocation);
}
