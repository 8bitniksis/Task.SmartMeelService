using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Sms.Client.Contracts;

namespace Sms.Console.Services;

public sealed class Application : BackgroundService
{
    private readonly ISmsClient _smsClient;
    private readonly ILogger<Application> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public Application(
        ISmsClient smsClient,
        ILogger<Application> logger,
        IHostApplicationLifetime lifetime)
    {
        _smsClient = smsClient;
        _logger = logger;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Application started");

            var menu = await _smsClient.GetMenuAsync(stoppingToken);

            foreach (var item in menu)
            {
                _logger.LogInformation(
                    "{Name} : {Price}",
                    item.Name,
                    item.Price);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled application error");
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}