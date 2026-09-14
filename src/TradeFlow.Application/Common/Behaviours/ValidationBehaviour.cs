using TradeFlow.Domain.Common.Results;
using FluentValidation;
using MediatR;

namespace TradeFlow.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    if (!validators.Any())
      return await next();

    var context = new ValidationContext<TRequest>(request);

    var validationResults = await Task.WhenAll(
        validators.Select(v => v.ValidateAsync(context, cancellationToken)));

    var failures = validationResults
        .SelectMany(r => r.Errors)
        .Where(f => f is not null)
        .ToList();

    if (failures.Count == 0)
      return await next();

    var errors = failures
        .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
        .ToList();

    var resultType = typeof(TResponse);

    if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      var valueType = resultType.GetGenericArguments()[0];

      var failureMethod = typeof(Result<>)
          .MakeGenericType(valueType)
          .GetMethod(nameof(Result<object>.Failure), [typeof(List<Error>)]);

      return (TResponse)failureMethod!.Invoke(null, [errors])!;
    }

    throw new InvalidOperationException(
        $"Handler response type '{resultType.Name}' must be Result<T> to use ValidationBehaviour.");
  }
}
