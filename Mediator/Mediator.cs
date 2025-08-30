using Mediator.Abstractions;

namespace Mediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetService(handlerType);

        ArgumentNullException.ThrowIfNull(handler, $"Handler for request type {requestType.Name} not found.");

        var method = handlerType.GetMethod("HandleAsync");

        ArgumentNullException.ThrowIfNull(method, $"HandleAsync method not found in handler for request type {requestType.Name}.");

        var result = method.Invoke(handler, [request, cancellationToken]);

        if (result is not Task<TResponse> task)
            throw new InvalidOperationException($"Handler for request type {requestType.Name} returned invalid result.");

        return await task;
    }
}
