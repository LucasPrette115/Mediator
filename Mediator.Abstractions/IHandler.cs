namespace Mediator.Abstractions;

public interface IHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellation = default);
}
