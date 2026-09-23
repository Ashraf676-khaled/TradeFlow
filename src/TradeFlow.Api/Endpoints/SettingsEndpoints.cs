namespace TradeFlow.Api.Endpoints;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Settings;
using TradeFlow.Application.Settings.Commands.UpdateSystemSettings;
using TradeFlow.Application.Settings.Queries.GetSystemSettings;

public sealed class SettingsEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/settings").WithTags("Settings").RequireAuthorization();

    // Always 200: missing keys resolve to safe defaults so the UI is never blocked.
    group.MapGet("/", async (ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(new GetSystemSettingsQuery(), ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });

    group.MapPut("/", async (UpdateSystemSettingsCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
    });
  }
}