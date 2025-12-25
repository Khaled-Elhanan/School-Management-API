using Application.Piplines;
using Application.Wrappers;
using FluentValidation;
using MediatR;

public class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>    , IValidateMe
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );                                 

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures.Select(f => f.ErrorMessage).Distinct().ToList();
            var responseType = typeof(TResponse);

            // Handle ResponseWrapper<T>
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ResponseWrapper<>))
            {
                var resultType = responseType.GetGenericArguments()[0];
                var failMethod = typeof(ResponseWrapper<>)
                    .MakeGenericType(resultType)
                    .GetMethod(nameof(ResponseWrapper<object>.FailAsync), new[] { typeof(List<string>) });

                if (failMethod != null)
                {
                    var task = (Task)failMethod.Invoke(null, new object[] { errors });
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    return (TResponse)resultProperty.GetValue(task);
                }
            }
            // Handle ResponseWrapper (non-generic)
            else if (responseType == typeof(ResponseWrapper))
            {
                return (TResponse)(object)await ResponseWrapper.FailAsync(errors);
            }
            
            // Fallback for non-wrapper types
            throw new ValidationException(failures);
        }

        return await next();
    }
}
                                             