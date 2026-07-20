using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Sms.Client;
using Sms.Console.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.json",
    optional: false,
    reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddSmsClient(builder.Configuration);

builder.Services.AddHostedService<Application>();

using var host = builder.Build();

await host.RunAsync();