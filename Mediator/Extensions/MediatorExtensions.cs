using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mediator.Extensions;

public static class MediatorExtensions
{
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        var handlerType = typeof(IHandler<,>);
        var notificationType = typeof(INotificationHandler<>);

        foreach (var assembly in assemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(x => x.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(t => t.Interface.IsGenericType && t.Interface.GetGenericTypeDefinition() == handlerType);

            var notifications = assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(x => x.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(t => t.Interface.IsGenericType && t.Interface.GetGenericTypeDefinition() == notificationType);

            foreach (var notification in notifications)
                services.AddScoped(notification.Interface, notification.Type);

            foreach (var handler in handlers)
                services.AddScoped(handler.Interface, handler.Type);

        }        

        return services;
    }
}
