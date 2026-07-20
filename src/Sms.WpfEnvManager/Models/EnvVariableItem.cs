using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sms.WpfEnvManager.Models;

public sealed class EnvVariableItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _value = string.Empty;
    private string _comment = string.Empty;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public string Comment
    {
        get => _comment;
        set => SetField(ref _comment, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}