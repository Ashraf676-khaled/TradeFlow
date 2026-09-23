namespace TradeFlow.Api.Endpoints.Sales;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Sales.SalesOrders.Commands.CancelSalesOrder;
using TradeFlow.Application.Sales.SalesOrders.Commands.CompleteSalesOrder;
using TradeFlow.Application.Sales.SalesOrders.Commands.ConfirmSalesOrder;
using TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;
using TradeFlow.Application.Sales.SalesOrders.Queries.GetSalesOrderById;
using TradeFlow.Application.Sales.SalesOrders.Queries.GetSalesOrders;

public sealed class SalesOrderEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/sales-orders").WithTags("Sales Orders").RequireAuthorization();

    group.MapPost("/", async (CreateSalesOrderCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });

    group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetSalesOrderByIdQuery(id), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapGet("/", async (
        ISender sender, CancellationToken ct,
        int pageNumber = 1, int pageSize = 20, string? status = null, Guid? customerId = null) =>
    {
      var result = await sender.Send(new GetSalesOrdersQuery(pageNumber, pageSize, status, customerId), ct);
      return Results.Ok(result);
    });

    group.MapPost("/{id:guid}/confirm", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ConfirmSalesOrderCommand(id), ct);
      // Returns { salesOrderId, invoiceId } so the UI can open the printable
      // invoice modal immediately after confirmation.
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new CancelSalesOrderCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/complete", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new CompleteSalesOrderCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });
  }
}
