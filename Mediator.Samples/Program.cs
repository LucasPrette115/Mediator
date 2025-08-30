using Mediator.Abstractions;
using Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddMediator(typeof(Program).Assembly);
services.AddTransient<AccountRepository>();

var servicesProvider = services.BuildServiceProvider();
var mediator = servicesProvider.GetRequiredService<IMediator>();

var request = new CreateAccountComand
{
    UserName = "BruceWayne",
    Password = "SecurePassword123"
};

var notification = new SendAccountNotification
{
    UserName = request.UserName,
    Password = request.Password
};

var result = await mediator.SendAsync(request);
Console.WriteLine(result);

await mediator.PublishAsync(notification);

public class AccountRepository
{
    public void CreateAccount(string accountName)
    {      
        Console.WriteLine($"Account '{accountName}' created.");
    }
}

public class CreateAccountComand : IRequest<string>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SendAccountNotification : INotification
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime Time { get; set; } = DateTime.Now;
}

public class SendEmailWhenAccountCreatedHandler : INotificationHandler<SendAccountNotification>
{   

    public Task HandleAsync(SendAccountNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Sending email to {notification.UserName}...");
        return Task.CompletedTask;
    }
}

public class SendSmsWhenAccountCreatedHandler : INotificationHandler<SendAccountNotification>
{

    public Task HandleAsync(SendAccountNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Sending SMS to {notification.UserName}...");
        return Task.CompletedTask;
    }
}

public class CreateAccountHandler(AccountRepository accountRepository) : IHandler<CreateAccountComand, string>
{
    public Task<string> HandleAsync(CreateAccountComand request, CancellationToken cancellation = default)
    {
        Console.WriteLine($"Creating {request.UserName} account...");
        accountRepository.CreateAccount(request.UserName);
        return Task.FromResult($"{request.UserName} account created");
    }
}