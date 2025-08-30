using Mediator.Abstractions;
using System.Collections;
using System.Reflection;

namespace Mediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType);

        ArgumentNullException.ThrowIfNull(handler, $"Handler for request type {requestType.Name} not found.");

        var method = handlerType.GetMethod("HandleAsync");

        ArgumentNullException.ThrowIfNull(method, $"HandleAsync method not found in handler for request type {requestType.Name}.");

        var result = method.Invoke(handler, [request, cancellationToken]);

        if (result is not Task<TResponse> task)
            throw new InvalidOperationException($"Handler for request type {requestType.Name} returned invalid result.");

        return await task;
    }   

    public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) 
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var notificationType = notification.GetType();
        var handlerInterfaceType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        var enumerableType = typeof(IEnumerable<>).MakeGenericType(handlerInterfaceType);
        var handlersObj = _serviceProvider.GetService(enumerableType);

        if (handlersObj is not IEnumerable handlers)
            return;

        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            if (handler is null) continue;

            var method = handlerInterfaceType.GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method is null) continue;

            var result = method.Invoke(handler, new object?[] { notification, cancellationToken });

            if (result is null) continue;

            if (result is Task t) tasks.Add(t);

            else if (result is ValueTask vt) tasks.Add(vt.AsTask());

            else
            {               
                if (result.GetType().IsSubclassOf(typeof(Task)) || typeof(Task).IsAssignableFrom(result.GetType()))
                    tasks.Add((Task)result);
                else
                    continue;
            }
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}

