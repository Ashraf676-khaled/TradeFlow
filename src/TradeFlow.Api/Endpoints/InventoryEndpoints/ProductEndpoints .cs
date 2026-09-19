namespace TradeFlow.Api.Endpoints.Inventory;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Inventory.Products.Commands.ActivateProduct;
using TradeFlow.Application.Inventory.Products.Commands.ChangeSellingPrice;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Products.Commands.DeactivateProduct;
using TradeFlow.Application.Inventory.Products.Queries.GetProductById;
using TradeFlow.Application.Inventory.Products.Queries.GetProducts;

public sealed class ProductEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/products").WithTags("Products").RequireAuthorization();

    group.MapPost("/", async (CreateProductCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });

    group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetProductByIdQuery(id), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapGet("/", async (ISender sender, CancellationToken ct, int pageNumber = 1, int pageSize = 20, bool? isActive = null) =>
    {
      var result = await sender.Send(new GetProductsQuery(pageNumber, pageSize, isActive), ct);
      return Results.Ok(result);
    });

    group.MapPut("/{id:guid}/price", async (Guid id, decimal newPrice, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ChangeSellingPriceCommand(id, newPrice), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/activate", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ActivateProductCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/deactivate", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new DeactivateProductCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });
  }
}
