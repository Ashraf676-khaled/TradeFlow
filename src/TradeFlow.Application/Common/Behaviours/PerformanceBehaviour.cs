using System.Diagnostics;
using TradeFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TradeFlow.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
  private readonly Stopwatch _timer = new();

  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    _timer.Start();

    var response = await next();

    _timer.Stop();

    var elapsedMilliseconds = _timer.ElapsedMilliseconds;

    if (elapsedMilliseconds > 500)
    {
      var requestName = typeof(TRequest).Name;
      var userId = currentUserService.UserId;

      logger.LogWarning(
          "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
          requestName, elapsedMilliseconds, userId, request);
    }

    return response;
  }
}
