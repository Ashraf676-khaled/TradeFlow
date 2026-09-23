namespace TradeFlow.Api.Endpoints.Customers;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Customers.Commands.ActivateCustomer;
using TradeFlow.Application.Customers.Commands.ChangeCreditLimit;
using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.Customers.Commands.DeactivateCustomer;
using TradeFlow.Application.Customers.Commands.UpdateCustomerContactInfo;
using TradeFlow.Application.Customers.Queries.GetCustomerById;
using TradeFlow.Application.Customers.Queries.GetCustomers;

public sealed class CustomerEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/customers").WithTags("Customers").RequireAuthorization();

    group.MapPost("/", async (CreateCustomerCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });

    group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetCustomerByIdQuery(id), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapGet("/", async (ISender sender, CancellationToken ct, int pageNumber = 1, int pageSize = 20, bool? isActive = null) =>
    {
      var result = await sender.Send(new GetCustomersQuery(pageNumber, pageSize, isActive), ct);
      return Results.Ok(result);
    });

    group.MapPut("/{id:guid}/contact-info", async (Guid id, UpdateCustomerContactInfoCommand body, ISender sender, CancellationToken ct) =>
    {
      var command = body with { CustomerId = id };
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPut("/{id:guid}/credit-limit", async (Guid id, ChangeCreditLimitBody body, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ChangeCreditLimitCommand(id, body.NewLimit), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/activate", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ActivateCustomerCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/deactivate", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new DeactivateCustomerCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });
  }
}

public sealed record ChangeCreditLimitBody(decimal NewLimit);
