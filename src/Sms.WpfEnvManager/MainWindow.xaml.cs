using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Sms.WpfEnvManager
{
    public partial class MainWindow : Window
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ObservableCollection<EnvVariableItem> _items = new();

        public MainWindow()
        {
            InitializeComponent();
            _logger = App.Logger;
            _configuration = App.Configuration;

            LoadVariables();
            EnvVariablesGrid.ItemsSource = _items;
        }

        private void LoadVariables()
        {
            var variableNames = _configuration.GetSection("EnvironmentVariables").Get<string[]>() ?? Array.Empty<string>();
            if (variableNames.Length == 0)
            {
                _logger.Warning("No environment variables specified in appsettings.json.");
                MessageBox.Show("Нет переменных в appsettings.json.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _items.Clear();
            foreach (var name in variableNames)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var currentValue = Environment.GetEnvironmentVariable(name) ?? string.Empty;
                    _items.Add(new EnvVariableItem
                    {
                        Name = name,
                        Value = currentValue,
                        Comment = $"Изменено: {DateTime.Now:HH:mm:ss}"
                    });
                }
            }

            _logger.Information("Загружено {Count} переменных среды.", _items.Count);
        }

        private void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            bool anyChanged = false;
            foreach (var item in _items)
            {
                string currentValue = Environment.GetEnvironmentVariable(item.Name) ?? string.Empty;
                if (item.Value != currentValue)
                {
                    try
                    {
                        // Устанавливаем переменную для текущего процесса и для пользователя (Windows)
                        Environment.SetEnvironmentVariable(item.Name, item.Value, EnvironmentVariableTarget.User);
                        // Для Windows: также можно установить в Machine, если нужны права админа
                        // Environment.SetEnvironmentVariable(item.Name, item.Value, EnvironmentVariableTarget.Machine);

                        _logger.Information("Переменная '{Name}' изменена: '{Old}' → '{New}'", item.Name, currentValue, item.Value);
                        anyChanged = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Не удалось обновить переменную '{Name}'", item.Name);
                        MessageBox.Show($"Ошибка при сохранении '{item.Name}': {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (anyChanged)
                MessageBox.Show("Все изменения сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Нет изменений для сохранения.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EnvVariablesGrid_RowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.Item is EnvVariableItem item)
                {
                    // Обновляем комментарий при редактировании
                    item.Comment = $"Изменено: {DateTime.Now:HH:mm:ss}";
                }
            }
        }
    }
}