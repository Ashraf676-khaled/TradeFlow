namespace TradeFlow.Api.Endpoints.Purchasing;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.ApprovePurchaseOrder;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CancelPurchaseOrder;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CreatePurchaseOrder;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.ReceivePurchaseOrderItem;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.SubmitPurchaseOrder;
using TradeFlow.Application.Purchasing.PurchaseOrders.Queries.GetPurchaseOrderById;
using TradeFlow.Application.Purchasing.PurchaseOrders.Queries.GetPurchaseOrders;

public sealed class PurchaseOrderEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/purchase-orders").WithTags("Purchase Orders").RequireAuthorization();

    group.MapPost("/", async (CreatePurchaseOrderCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });

    group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetPurchaseOrderByIdQuery(id), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapGet("/", async (
        ISender sender, CancellationToken ct,
        int pageNumber = 1, int pageSize = 20, string? status = null, Guid? supplierId = null) =>
    {
      var result = await sender.Send(new GetPurchaseOrdersQuery(pageNumber, pageSize, status, supplierId), ct);
      return Results.Ok(result);
    });

    group.MapPost("/{id:guid}/submit", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new SubmitPurchaseOrderCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/approve", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ApprovePurchaseOrderCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/receive", async (Guid id, ReceiveItemBody body, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(
          new ReceivePurchaseOrderItemCommand(id, body.ProductId, body.ReceivedQuantity), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new CancelPurchaseOrderCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });
  }
}

public sealed record ReceiveItemBody(Guid ProductId, int ReceivedQuantity);
