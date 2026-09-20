namespace TradeFlow.Api.Endpoints.Purchasing;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Purchasing.Suppliers.Commands.ActivateSupplier;
using TradeFlow.Application.Purchasing.Suppliers.Commands.ChangeSupplierPhone;
using TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;
using TradeFlow.Application.Purchasing.Suppliers.Commands.DeactivateSupplier;
using TradeFlow.Application.Purchasing.Suppliers.Commands.RenameSupplier;
using TradeFlow.Application.Purchasing.Suppliers.Queries.GetSupplierById;
using TradeFlow.Application.Purchasing.Suppliers.Queries.GetSuppliers;

public sealed class SupplierEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/suppliers").WithTags("Suppliers").RequireAuthorization();

    group.MapPost("/", async (CreateSupplierCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(new { id = result.Value }) : result.Errors.ToProblem();
    });

    group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetSupplierByIdQuery(id), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapGet("/", async (ISender sender, CancellationToken ct, int pageNumber = 1, int pageSize = 20, bool? isActive = null) =>
    {
      var result = await sender.Send(new GetSuppliersQuery(pageNumber, pageSize, isActive), ct);
      return Results.Ok(result);
    });

    group.MapPut("/{id:guid}/name", async (Guid id, RenameSupplierBody body, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new RenameSupplierCommand(id, body.NewName), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPut("/{id:guid}/phone", async (Guid id, ChangeSupplierPhoneBody body, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ChangeSupplierPhoneCommand(id, body.NewPhone), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/activate", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new ActivateSupplierCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });

    group.MapPost("/{id:guid}/deactivate", async (Guid id, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new DeactivateSupplierCommand(id), ct);
      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });
  }
}

public sealed record RenameSupplierBody(string NewName);
public sealed record ChangeSupplierPhoneBody(string NewPhone);
