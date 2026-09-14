using TradeFlow.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace TradeFlow.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest>(
    ILogger<LoggingBehaviour<TRequest>> logger,
    ICurrentUserService currentUserService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
  public Task Process(TRequest request, CancellationToken cancellationToken)
  {
    var requestName = typeof(TRequest).Name;
    var userId = currentUserService.UserId;

    logger.LogInformation("Application Request: {Name} {@UserId} {@Request}",
        requestName, userId, request);

    return Task.CompletedTask;
  }
}
