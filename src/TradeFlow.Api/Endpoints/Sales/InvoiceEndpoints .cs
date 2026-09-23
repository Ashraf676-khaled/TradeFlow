namespace TradeFlow.Api.Endpoints.Sales;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Sales.Invoices.Commands.CancelInvoice;
using TradeFlow.Application.Sales.Invoices.Commands.CreateInvoiceFromOrder;
using TradeFlow.Application.Sales.Invoices.Commands.RegisterPayment;
using TradeFlow.Application.Sales.Invoices.Queries.GetInvoiceById;
using TradeFlow.Application.Sales.Invoices.Queries.GetInvoices;

public sealed class InvoiceEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/invoices").WithTags("Invoices").RequireAuthorization();

    group.MapPost("/", async (CreateInvoiceFromOrderCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });

    group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetInvoiceByIdQuery(id), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapGet("/", async (ISender sender, CancellationToken ct, int pageNumber = 1, int pageSize = 20, Guid? customerId = null) =>
    {
      var result = await sender.Send(new GetInvoicesQuery(pageNumber, pageSize, customerId), ct);
      return Results.Ok(result);
    });

    group.MapPost("/{id:guid}/payments", async (Guid id, RegisterPaymentBody body, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new RegisterPaymentCommand(id, body.Amount), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new CancelInvoiceCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });
  }
}

public sealed record RegisterPaymentBody(decimal Amount);
