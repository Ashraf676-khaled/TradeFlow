using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TradeFlow.Api.Infrastructure;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
  {
    // The client aborted the request — nothing meaningful left to write.
    if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
    {
      logger.LogInformation(
          "Request {Method} {Path} was cancelled by the client.",
          httpContext.Request.Method,
          httpContext.Request.Path);
      return true;
    }

    var (statusCode, title, detail) = exception switch
    {
      DbUpdateConcurrencyException => (
          StatusCodes.Status409Conflict,
          "Concurrent update conflict",
          "The data was modified by another operation. Reload and try again."),
      DbUpdateException => (
          StatusCodes.Status409Conflict,
          "Database update conflict",
          "The change could not be saved because it conflicts with existing data."),
      _ => (
          StatusCodes.Status500InternalServerError,
          "Application error",
          "An internal server error occurred."),
    };

    logger.LogError(
        exception,
        "Unhandled exception while handling {Method} {Path} → {StatusCode}",
        httpContext.Request.Method,
        httpContext.Request.Path,
        statusCode);

    httpContext.Response.StatusCode = statusCode;

    // Only expose exception details in Development to avoid leaking internals in production.
    var includeExceptionDetails = httpContext.RequestServices
        .GetService<IHostEnvironment>()?.IsDevelopment() ?? false;

    return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
    {
      HttpContext = httpContext,
      Exception = exception,
      ProblemDetails = new ProblemDetails
      {
        Type = title,
        Title = title,
        Status = statusCode,
        Detail = includeExceptionDetails ? exception.Message : detail,
      },
    });
  }
}
