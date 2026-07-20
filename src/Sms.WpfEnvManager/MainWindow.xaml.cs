using Sms.WpfEnvManager.Models;
using Sms.WpfEnvManager.Services;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sms.WpfEnvManager;

public partial class MainWindow : Window
{
    private readonly IEnvironmentVariableService _environmentService;

    private readonly ObservableCollection<EnvVariableItem> _items = [];

    public MainWindow(
        IEnvironmentVariableService environmentService)
    {
        InitializeComponent();

        _environmentService = environmentService;

        EnvVariablesGrid.ItemsSource = _items;

        LoadVariables();
    }

    private void LoadVariables()
    {
        _items.Clear();

        foreach (var item in _environmentService.Load())
        {
            _items.Add(item);
        }
    }

    private void SaveAllButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        EnvVariablesGrid.CommitEdit();
        EnvVariablesGrid.CommitEdit(
            DataGridEditingUnit.Row,
            true);

        try
        {
            var changed = _environmentService.Save(_items);

            MessageBox.Show(
                changed == 0
                    ? "Изменений не обнаружено."
                    : $"Сохранено {changed} переменных.",
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void EnvVariablesGrid_RowEditEnding(
        object? sender,
        DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit)
            return;

        if (e.Row.Item is EnvVariableItem item)
        {
            item.Comment = "Modified";
        }
    }

    private void TitleBar_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState =
                WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;

            return;
        }

        DragMove();
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
        => Close();

    private void MinimizeButton_Click(
        object sender,
        RoutedEventArgs e)
        => WindowState = WindowState.Minimized;
}