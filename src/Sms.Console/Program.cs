using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Sms.Client;
using Sms.Console.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: true);

    builder.Services.AddSerilog();

    builder.Services.AddSmsClient(builder.Configuration);

    builder.Services.AddHostedService<Application>();

    using var host = builder.Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}