namespace Sms.WpfEnvManager;

public class EnvVariableItem
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty; // Можно оставить пустым или использовать из конфига
}