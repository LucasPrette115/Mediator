# Mediator (Implementation & Abstractions)

> Biblioteca **Mediator** — implementação leve de padrões mediator/mediator-abstractions para .NET (organizada em dois pacotes: `Mediator.Abstractions` e `Mediator`).
---

## Sumário

* [Visão Geral](#visão-geral)
* [Pacotes publicados](#pacotes-publicados)
* [Registro em DI](#registro-em-di)
* [Usando Requests (com retorno)](#usando-requests-com-retorno)
* [Usando Commands/Requests sem retorno](#usando-commandsrequests-sem-retorno)
* [Notificações / Events](#notificações--events)
* [Exemplo completo (Minimal API)](#exemplo-completo-minimal-api)

---

# Visão Geral

Esta solução foi separada em dois pacotes:

* `Mediator.Abstractions` — contém contratos públicos, interfaces e tipos que consumidores podem depender sem trazer a implementação concreta.
* `Mediator` — implementação concreta do mediator, que resolve handlers, roda pipelines/middleware e envia requisições/notificações.

Essa separação permite:

* consumidores dependendo só das abstrações quando desejam implementar sua própria versão;
* consumidores simples instalando só a implementação pronta.

---

# Pacotes publicados

* `Mediator.Abstractions` — contratos e interfaces (ex.: `IMediator`, `IRequest<TResponse>`, `IHandler<TRequest,TResponse>`, `INotification`, `INotificationHandler<TNotification>`).
* `Mediator` — implementação concreta que provê a resolução de handlers e integração com `Microsoft.Extensions.DependencyInjection`.

---

# Registro em DI

### 1) Se o pacote `Mediator` expõe uma extensão

Muitas bibliotecas fornecem um `IServiceCollection` extension, ex.:

```csharp
using Microsoft.Extensions.DependencyInjection;
// using Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator(typeof(Program).Assembly);
```
# Usando Requests (com retorno)

```csharp
// Definição de request
public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;

// Handler
public class GetCustomerByIdHandler : IHandler<GetCustomerByIdQuery, CustomerDto>
{
    public Task<CustomerDto> HandleAsync(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        // lógica para buscar cliente
        var dto = new CustomerDto { Id = request.Id, Name = "exemplo" };
        return Task.FromResult(dto);
    }
}
```
Enviar a requisição:

```csharp
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;
    public CustomerController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.SendAsync(new GetCustomerByIdQuery(id));
        return Ok(result);
    }
}
```

# Usando Commands/Requests sem retorno

```csharp
public record CreateOrderCommand(OrderDto Order) : IRequest;

public class CreateOrderHandler : IHandler<CreateOrderCommand>
{
    public Task HandleAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // processa comando
        return Task.CompletedTask;
    }
}
```

Envio:

```csharp
await _mediator.SendAsync(new CreateOrderCommand(new OrderDto { /*...*/ }));
```

---

# Notificações / Events

```csharp
public record OrderCreatedNotification(Guid OrderId) : INotification;

public class NotifyByEmailHandler : INotificationHandler<OrderCreatedNotification>
{
    public Task HandleAsync(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        // enviar email
        return Task.CompletedTask;
    }
}
```

Publisher:

```csharp
await _mediator.PublishAsync(new OrderCreatedNotification(orderId));
```

---

# Exemplo completo (Minimal API)

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Mediator.Abstractions;
using Mediator;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediator(typeof(Program).Assembly);
var app = builder.Build();

app.MapGet("/customers/{id}", async (Guid id, IMediator mediator) =>
{
    var customer = await mediator.SendAsync(new GetCustomerByIdQuery(id));
    return Results.Ok(customer);
});

app.Run();
```

