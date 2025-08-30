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

var result = await mediator.SendAsync(request);

Console.WriteLine(result);

public class AccountRepository
{
    public void CreateAccount(string accountName)
    {
        // Logic to create an account
        Console.WriteLine($"Account '{accountName}' created.");
    }
}

public class CreateAccountComand : IRequest<string>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
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