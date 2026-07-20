using System.Diagnostics;
using Microsoft.Extensions.Logging;

using Sms.Client.Contracts;

namespace Sms.Console.Services;

public sealed class Application
{
    private readonly ISmsClient _smsClient;
    private readonly ILogger<Application> _logger;

    public Application(
        ISmsClient smsClient,
        ILogger<Application> logger)
    {
        _smsClient = smsClient;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading menu...");

        var menu = await _smsClient.GetMenuAsync(cancellationToken);

        foreach (var item in menu)
        {
            Debug.WriteLine($"{item.Name} | {item.Price:C}");
        }
    }
}