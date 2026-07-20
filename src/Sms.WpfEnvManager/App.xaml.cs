using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core; // Может не понадобиться, но добавим на всякий случай, если понадобится ILogger явно
using System.IO;
using System.Windows;

namespace Sms.WpfEnvManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; } = null!;
        // Изменяем тип на конкретный Serilog.ILogger
        public static Serilog.ILogger Logger { get; private set; } = null!; 

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Настройка Serilog
            var logFileName = $"test-sms-wpf-app-{System.DateTime.UtcNow:yyyyMMdd}.log";
            // Убедимся, что присваиваем Serilog.ILogger
            Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logFileName, rollingInterval: RollingInterval.Day)
                .CreateLogger(); 

            // 2. Загрузка конфигурации
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            // Логирование старта приложения
            Logger.Information("Application started.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Logger.Information("Application exit.");
        }
    }
}