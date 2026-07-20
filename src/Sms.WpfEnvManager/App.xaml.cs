using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Sms.WpfEnvManager.Services;

using System.IO;
using System.Windows;

namespace Sms.WpfEnvManager;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false);

        var logsDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Logs");

        Directory.CreateDirectory(logsDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(
                    logsDirectory,
                    $"test-sms-wpf-app-{DateTime.Now:yyyyMMdd}.log"))
            .CreateLogger();

        builder.Services.AddSingleton(Log.Logger);

        builder.Services.AddSingleton<EnvironmentConfiguration>();

        builder.Services.AddSingleton<IEnvironmentVariableService,
            EnvironmentVariableService>();

        builder.Services.AddSingleton<MainWindow>();

        _host = builder.Build();

        await _host.StartAsync();

        var window = _host.Services.GetRequiredService<MainWindow>();

        window.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
            await _host.StopAsync();

        Log.CloseAndFlush();

        base.OnExit(e);
    }
}